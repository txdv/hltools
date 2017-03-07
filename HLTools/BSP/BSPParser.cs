using System;
using System.IO;
using System.Text;

using HLTools.Extensions;

using Vector3f = OpenTK.Vector3;

namespace HLTools.BSP
{
	/*
	 * GoldSrc has bsp map version 30(0x1E)
	 */

	#region Structs

	public struct DirectoryEntry
	{
		public int offset;
		public int size;
	}

	public struct Plane
	{
		public Vector3f normal;
		public float distance;
		public int type;
	}

	public struct BoundBox
	{
		public Vector3f minimum;
		public Vector3f maximum;

		unsafe public bool InBox(Vector3f vec)
		{
			return (
				(
					minimum.X <= vec.X && vec.X <= maximum.X &&
					minimum.Y <= vec.Y && vec.Y <= maximum.Y &&
					minimum.Z <= vec.Z && vec.Z <= maximum.Z
				) || (
					minimum.X >= vec.X && vec.X >= maximum.X &&
					minimum.Y >= vec.Y && vec.Y >= maximum.Y &&
					minimum.Z >= vec.Z && vec.Z >= maximum.Z
				)
			);
		}

		unsafe public override string ToString()
		{
			return string.Format(
				"({0}:{1}) ({2}:{3}) ({4}:{5})",
				minimum.X, maximum.X,
				minimum.Y, maximum.Y,
				minimum.Z, maximum.Z
			);
		}
	}

	public struct MipTexture
	{
		public StructString name;

		public int width;
		public int height;

		public int offset1;
		public int offset2;
		public int offset3;
		public int offset4;
	}

	public struct BSPNode
	{
		public int plane_id;
		public short front;
		public short back;
		public BoundBoxShort bound_box;
		public ushort face_id;
		public ushort face_num;
	}

	public struct BoundBoxShort
	{
		unsafe public fixed short nMin[3];
		unsafe public fixed short nMax[3];

		unsafe public bool InBox(Vector3f vec)
		{
			fixed (short* vMin = nMin)
			fixed (short* vMax = nMax)
			{
				return (
					(
						(float)vMin[0] <= vec.X && vec.X <= (float)vMax[0] &&
						(float)vMin[1] <= vec.Y && vec.Y <= (float)vMax[1] &&
						(float)vMin[2] <= vec.Z && vec.Z <= (float)vMax[2]
					) || (
						(float)vMin[0] >= vec.X && vec.X >= (float)vMax[0] &&
						(float)vMin[1] >= vec.Y && vec.Y >= (float)vMax[1] &&
						(float)vMin[2] >= vec.Z && vec.Z >= (float)vMax[2]
					)
				);
			}
		}

		unsafe public override string ToString()
		{
			fixed (short* vMin = nMin)
			fixed (short* vMax = nMax)
			{
				return string.Format(
					"({0}:{1}) ({2}:{3}) ({4}:{5})",
					vMin[0], vMax[0],
					vMin[1], vMax[1],
					vMin[2], vMax[2]
				);
			}
		}
	}

	public struct FaceTextureInfo
	{
		public Vector3f vectorS;
		public float distS;
		public Vector3f vectorT;
		public float distT;
		public uint texture_id;
		public uint animated;
	}

	public struct Face
	{
		public ushort plane_id;

		public ushort side;

		public uint ledge_id;
		public ushort ledge_num;

		public ushort texinfo_id;

		public byte typelight;
		public byte baselight;
		public byte light1;
		public byte light2;
		public int lightmap;
	}

	public struct ClipNode
	{

		public uint planenum;
		public short front;
		public short back;
	}

	public struct BSPLeaf
	{
		public int nContents;
		public int nVisOffset;
		public BoundBoxShort bound_box;
		public ushort iFirstMarkSurface;
		public ushort nMarkSurfaces;

	}

	public struct Edge
	{
		public ushort vertex0;
		public ushort vertex1;
	}

	public struct Model
	{
		public BoundBoxShort bound_box;
		public int node_id0;
		public int node_id1;
		public int node_id2;
		public int node_id3;
		public int numleafs;
		public int face_id;
		public int face_num;
	}

	#endregion

	public class BSPParser
	{
		protected BinaryReader br;

		public BSPParser(Stream stream)
		{
			br = new BinaryReader(stream, Encoding.ASCII);
		}

		#region Load Functions

		public bool LoadDirectoryTables()
		{
			try {
				Version = br.BReadInt32();

				// directories
				Entities       = br.ReadStruct<DirectoryEntry>();
				Planes         = br.ReadStruct<DirectoryEntry>();
				MipTextures    = br.ReadStruct<DirectoryEntry>();
				Vertices       = br.ReadStruct<DirectoryEntry>();
				VisibilityList = br.ReadStruct<DirectoryEntry>();
				Nodes          = br.ReadStruct<DirectoryEntry>();
				TextureInfo    = br.ReadStruct<DirectoryEntry>();
				Faces          = br.ReadStruct<DirectoryEntry>();
				Lightmaps      = br.ReadStruct<DirectoryEntry>();
				Clipnodes      = br.ReadStruct<DirectoryEntry>();
				Leaves         = br.ReadStruct<DirectoryEntry>();
				FaceList       = br.ReadStruct<DirectoryEntry>();
				Edges          = br.ReadStruct<DirectoryEntry>();
				EdgeList       = br.ReadStruct<DirectoryEntry>();
				Models         = br.ReadStruct<DirectoryEntry>();

				//if (OnLoadDirectoryTable != null) OnLoadDirectoryTable();
			} catch {
				return false;
			}

			return true;
		}

		public string ReadEntities()
		{
			br.BaseStream.Seek(Entities.offset, SeekOrigin.Begin);
			return Encoding.ASCII.GetString(br.ReadBytes((int)Entities.size));
		}

		private T[] LoadArray<T>(DirectoryEntry entry, Func<T> read) where T : struct
		{
			br.BaseStream.Seek(entry.offset, SeekOrigin.Begin);
			int size = SizeHelper.SizeOf<T>();
			int n = entry.size / size;
			var ret = new T[n];
			for (int i = 0; i < n; i++) {
				ret[i] = read();
			}
			return ret;
		}

		public Plane[] LoadPlanesArray()
		{
			return LoadArray<Plane>(Planes, br.ReadStruct<Plane>);
		}

		public bool LoadMipTextureOffsets()
		{
			br.BaseStream.Seek(MipTextures.offset, SeekOrigin.Begin);
			int size = br.BReadInt32();
			MipTextureOffsets = new int[size];
			for (int j = 0; j < size; j++) {
				MipTextureOffsets[j] = br.BReadInt32();
			}
			return true;
		}

		public MipTexture[] LoadMipTexturesArray()
		{
			if (MipTextureOffsets == null) {
				LoadMipTextureOffsets();
			}

			int n = MipTextureOffsets.Length;
			var ret = new MipTexture[n];
			for (int i = 0; i < n; i++) {
				br.BaseStream.Seek(MipTextures.offset + MipTextureOffsets[i], SeekOrigin.Begin);
				ret[i] = br.ReadStruct<MipTexture>();
			}
			return ret;
		}

		public Vector3f[] LoadVerticesArray()
		{
			return LoadArray<Vector3f>(Vertices, br.ReadStruct<Vector3f>);
		}


		public BSPNode[] LoadBSPNodesArray()
		{
			return LoadArray<BSPNode>(Nodes, br.ReadStruct<BSPNode>);
		}

		public FaceTextureInfo[] LoadFaceTextureInfoArray()
		{
			return LoadArray<FaceTextureInfo>(TextureInfo, br.ReadStruct<FaceTextureInfo>);
		}

		public Face[] LoadFaceArray()
		{
			return LoadArray<Face>(Faces, br.ReadStruct<Face>);
		}

		public byte[] LoadLightMapsArray()
		{
			return LoadArray<byte>(Lightmaps, br.ReadByte);
		}

		public ClipNode[] LoadClipNodesArray()
		{
			return LoadArray<ClipNode>(Clipnodes, br.ReadStruct<ClipNode>);
		}

		public BSPLeaf[] LoadBSPLeavesArray()
		{
			return LoadArray<BSPLeaf>(Leaves, br.ReadStruct<BSPLeaf>);
		}

		public short[] LoadFaceListArray()
		{
			return LoadArray<short>(FaceList, br.BReadInt16);
		}

		public Edge[] LoadEdgesArray()
		{
			return LoadArray<Edge>(Edges, br.ReadStruct<Edge>);
		}

		public int[] LoadEdgeListArray()
		{
			return LoadArray<int>(EdgeList, br.BReadInt32);
		}

		public Model[] LoadModelsArray()
		{
			return LoadArray<Model>(Models, br.ReadStruct<Model>);
		}

		#endregion

		#region Public Fields

		public int Version { get; protected set; }
		public DirectoryEntry Entities { get; protected set; }
		public DirectoryEntry Planes { get; protected set; }
		public DirectoryEntry MipTextures { get; protected set; }
		public int[] MipTextureOffsets { get; protected set; }
		public DirectoryEntry Vertices { get; protected set; }
		public DirectoryEntry VisibilityList { get; protected set; }
		public DirectoryEntry Nodes { get; protected set; }
		public DirectoryEntry TextureInfo { get; protected set; }
		public DirectoryEntry Faces { get; protected set; }
		public DirectoryEntry Lightmaps { get; protected set; }
		public DirectoryEntry Clipnodes { get; protected set; }
		public DirectoryEntry Leaves { get; protected set; }
		public DirectoryEntry FaceList { get; protected set; }
		public DirectoryEntry Edges { get; protected set; }
		public DirectoryEntry EdgeList { get; protected set; }
		public DirectoryEntry Models { get; protected set; }

		#endregion

		public void Close()
		{
			br.Close();
		}
	}
}

