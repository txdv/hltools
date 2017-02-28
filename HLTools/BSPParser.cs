using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

using HLTools.Extensions;

using Vector3f = OpenTK.Vector3;

namespace HLTools.BSP
{
	/*
	 * GoldSrc has bsp map version 30(0x1E)
	 */

	public static class BinaryReaderExtensions
	{
		unsafe public static T ReadStruct<T>(this Stream stream) where T : struct
		{
			int size = SizeHelper.SizeOf<T>();
			var buff = new byte[size];
			stream.Read(buff, 0, buff.Length);
			fixed (byte* ptr = buff) {
				return Marshal.PtrToStructure<T>((IntPtr)ptr);
			}
		}

		unsafe public static T ReadStruct<T>(this BinaryReader stream) where T : struct
		{
			int size = SizeHelper.SizeOf<T>();
			var buff = new byte[size];
			stream.Read(buff, 0, buff.Length);
			fixed (byte* ptr = buff) {
				return Marshal.PtrToStructure<T>((IntPtr)ptr);
			}
		}
	}

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
	}

	public struct MipTexture
	{
		unsafe public string Name {
			get {
				fixed (sbyte* ptr = name) {
					return new string(ptr);
				}
			}
		}

		unsafe public fixed byte name[16];

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
		unsafe public fixed short nMin[3];
		unsafe public fixed short nMax[3];
		public ushort face_id;
		public ushort face_num;

		unsafe public bool InBox(Vector3f vec)
		{
			fixed (short* vMin = nMin)
			fixed (short* vMax = nMax)
			{
				return InBox(vec, vMin, vMax);
			}
		}

		unsafe internal static bool InBox(Vector3f vec, short* vMin, short* vMax)
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
			) ;
		}

		unsafe internal static string ToString(short* vMin, short* vMax)
		{
			return string.Format(
				"({0}:{1}) ({2}:{3}) ({4}:{5})",
				vMin[0], vMax[0],
				vMin[1], vMax[1],
				vMin[2], vMax[2]
			);
		}

		unsafe public override string ToString()
		{
			fixed (short* vMin = nMin)
			fixed (short* vMax = nMax)
			{
				return ToString(vMin, vMax);
			}
		}
	}

	public struct BoundBoxShort
	{
		public short min;
		public short max;
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
		unsafe public fixed short nMin[3];
		unsafe public fixed short nMax[3];
		public ushort iFirstMarkSurface;
		public ushort nMarkSurfaces;

		unsafe public override string ToString()
		{
			fixed (short* vMin = nMin)
			fixed (short* vMax = nMax)
			{
				return BSPNode.ToString(vMin, vMax);
			}
		}

		unsafe public bool InBox(Vector3f vec)
		{
			fixed (short* vMin = nMin)
			fixed (short* vMax = nMax)
			{
				return BSPNode.InBox(vec, vMin, vMax);
			}
		}
	}

	public struct Edge
	{
		public ushort vertex0;
		public ushort vertex1;
	}

	public struct Model
	{
		public BoundBox bound;
		public Vector3f origin;
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
		#region Events
		//public event LoadDirectoryTableDelegate OnLoadDirectoryTable;
		public event Action<string>          OnLoadEntities;
		public event Action<Plane>           OnLoadPlane;
		public event Action<MipTexture>      OnLoadMipTexture;
		public event Action<Vector3f>        OnLoadVertex;
		public event Action<BSPNode>         OnLoadBSPNode;
		public event Action<FaceTextureInfo> OnLoadFaceTextureInfo;
		public event Action<Face>            OnLoadFace;
		public event Action<byte>            OnLoadLightMap;
		public event Action<ClipNode>        OnLoadClipNode;
		public event Action<BSPLeaf>         OnLoadBSPLeaf;
		public event Action<short>           OnLoadFaceListElement;
		public event Action<Edge>            OnLoadEdge;
		public event Action<int>             OnLoadEdgeListElement;
		public event Action<Model>           OnLoadModel;
	    #endregion

		private BinaryReader br;

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

		public bool LoadEntities()
		{
			if (OnLoadEntities != null) {
				OnLoadEntities(ReadEntities());
			}

			return true;
		}

		public string ReadEntities()
		{
			br.BaseStream.Seek(Entities.offset, SeekOrigin.Begin);
			return Encoding.ASCII.GetString(br.ReadBytes((int)Entities.size));
		}

		unsafe private T[] LoadArray<T>(DirectoryEntry entry, Func<T> read) where T : struct
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

		unsafe private bool Load<T>(DirectoryEntry entry, Func<T> read, Action<T> onLoad) where T : struct
		{
			if (onLoad != null) {
				br.BaseStream.Seek(entry.offset, SeekOrigin.Begin);
				int size = SizeHelper.SizeOf<T>();
				int n = entry.size / size;

				for (int i = 0; i < n; i++) {
					onLoad(read());
				}
			}

			return true;
		}

		public bool LoadPlanes()
		{
			return Load<Plane>(Planes, br.ReadStruct<Plane>, OnLoadPlane);
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

		public bool LoadMipTextures()
		{
			if (MipTextureOffsets == null) {
				return false;
			}

			foreach (int offset in MipTextureOffsets) {
				br.BaseStream.Seek(MipTextures.offset + offset, SeekOrigin.Begin);
				OnLoadMipTexture(br.ReadStruct<MipTexture>());
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

		unsafe public bool LoadVertices()
		{
			return Load<Vector3f>(Vertices, br.ReadStruct<Vector3f>, OnLoadVertex);
		}

		unsafe public Vector3f[] LoadVerticesArray()
		{
			return LoadArray<Vector3f>(Vertices, br.ReadStruct<Vector3f>);
		}


		public bool LoadBSPNodes()
		{
			return Load<BSPNode>(Nodes, br.ReadStruct<BSPNode>, OnLoadBSPNode);
		}

		public BSPNode[] LoadBSPNodesArray()
		{
			return LoadArray<BSPNode>(Nodes, br.ReadStruct<BSPNode>);
		}

		public bool LoadFaceTextureInfo()
		{
			return Load<FaceTextureInfo>(TextureInfo, br.ReadStruct<FaceTextureInfo>, OnLoadFaceTextureInfo);
		}

		public FaceTextureInfo[] LoadFaceTextureInfoArray()
		{
			return LoadArray<FaceTextureInfo>(TextureInfo, br.ReadStruct<FaceTextureInfo>);
		}

		public bool LoadFaces()
		{
			return Load<Face>(Faces, br.ReadStruct<Face>, OnLoadFace);
		}

		public Face[] LoadFaceArray()
		{
			return LoadArray<Face>(Faces, br.ReadStruct<Face>);
		}

		public bool LoadLightMaps()
		{
			return Load<byte>(Lightmaps, br.ReadByte, OnLoadLightMap);
		}

		public byte[] LoadLightMapsArray()
		{
			return LoadArray<byte>(Lightmaps, br.ReadByte);
		}

		public bool LoadClipNodes()
		{
			return Load<ClipNode>(Clipnodes, br.ReadStruct<ClipNode>, OnLoadClipNode);
		}

		public ClipNode[] LoadClipNodesArray()
		{
			return LoadArray<ClipNode>(Clipnodes, br.ReadStruct<ClipNode>);
		}

		public bool LoadBSPLeaves()
		{
			return Load<BSPLeaf>(Leaves, br.ReadStruct<BSPLeaf>, OnLoadBSPLeaf);
		}

		public BSPLeaf[] LoadBSPLeavesArray()
		{
			return LoadArray<BSPLeaf>(Leaves, br.ReadStruct<BSPLeaf>);
		}

		public bool LoadFaceList()
		{
			return Load<short>(FaceList, br.ReadInt16, OnLoadFaceListElement);
		}

		public short[] LoadFaceListArray()
		{
			return LoadArray<short>(FaceList, br.BReadInt16);
		}

		public bool LoadEdges()
		{
			return Load<Edge>(Edges, br.ReadStruct<Edge>, OnLoadEdge);
		}

		public Edge[] LoadEdgesArray()
		{
			return LoadArray<Edge>(Edges, br.ReadStruct<Edge>);
		}

		public bool LoadEdgeList()
		{
			return Load<int>(EdgeList, br.BReadInt32, OnLoadEdgeListElement);
		}

		public int[] LoadEdgeListArray()
		{
			return LoadArray<int>(EdgeList, br.BReadInt32);
		}

		public bool LoadModels()
		{
			return Load<Model>(Models, br.ReadStruct<Model>, OnLoadModel);
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

