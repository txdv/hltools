// 
//     This file is part of Iron.
//     Iron is an extensive game engine written in C# aiming to
//     utilize already exitent game content 
// 
//     Copyright (C) 2010  Andrius Bentkus
//     Copyright (C) 2010  Giedrius Graževičius
// 
//     Iron is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     Iron is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with Iron.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.IO;
using System.Text;

namespace HLTools.BSP
{
	/*
	 * GoldSrc has bsp map version 30(0x1E)
	 */
	
	public static class BinaryReaderExtensions
	{
		#region Big Endian
		
		public static int BReadInt32(this BinaryReader br)
		{
			byte b1 = br.ReadByte();
			byte b2 = br.ReadByte();
			byte b3 = br.ReadByte();
			byte b4 = br.ReadByte();
			
			return ((b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
		}
		
		public static uint BReadUInt32(this BinaryReader br)
		{
			byte b1 = br.ReadByte();
			byte b2 = br.ReadByte();
			byte b3 = br.ReadByte();
			byte b4 = br.ReadByte();
			
			return (uint)((b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
		}
		
		public static short BReadInt16(this BinaryReader br)
		{
			byte b1 = br.ReadByte();
			byte b2 = br.ReadByte();
			return (short)((b2 << 8) | b1);
		}
		
		public static ushort BReadUInt16(this BinaryReader br)
		{
			byte b1 = br.ReadByte();
			byte b2 = br.ReadByte();
			return (ushort)((b2 << 8) | b1);
		}
		
		#endregion
		
		#region Struct Reader
		
		public static Vector3f ReadVector3f(this BinaryReader br)
		{
			return new Vector3f(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
		}
		
		public static DirectoryEntry BReadDirectoryEntry(this BinaryReader br)
		{
			return new DirectoryEntry(br.BReadInt32(), br.BReadInt32());
		}
		
		public static MipTexture BReadMipTexture(this BinaryReader br)
		{
			return new MipTexture(Encoding.ASCII.GetString(br.ReadBytes(16)), br.BReadInt32(), br.BReadInt32(),
			                      br.BReadInt32(), br.BReadInt32(), br.BReadInt32(), br.BReadInt32());
		}
		
		public static float BReadFloat(this BinaryReader br)
		{
			return Convert.ToSingle(br.BReadInt32());
		}
		
		public static BoundBoxShort BReadBoundBoxShort(this BinaryReader br)
		{
			return new BoundBoxShort(br.ReadInt16(), br.ReadInt16());
		}
		
		public static BSPNode BReadBSPNode(this BinaryReader br)
		{
			return new BSPNode(br.BReadInt32(), br.ReadUInt16(), br.ReadUInt16(),
			                   br.BReadBoundBoxShort(), br.ReadUInt16(), br.ReadUInt16());
		}
		
		public static FaceTextureInfo BReadFaceTextureInfo(this BinaryReader br)	
		{
			return new FaceTextureInfo(br.ReadVector3f(),br.BReadFloat(),
                                       br.ReadVector3f(), br.BReadFloat(),
                                       br.BReadUInt32(), br.BReadUInt32());
		}
		
		public static Face BReadFace(this BinaryReader br)
		{
			return new Face(br.BReadUInt16(), br.BReadUInt16(), br.BReadUInt32(), br.BReadUInt16(),
			                br.BReadUInt16(), br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte(),
			                br.ReadInt32());
		}
		
		public static ClipNode BReadClipNode(this BinaryReader br)
		{
			return new ClipNode(br.BReadUInt32(), br.BReadInt16(), br.BReadInt16());
		}
		
		public static BSPLeaf BReadBSPLeaf(this BinaryReader br)
		{
			return new BSPLeaf(br.ReadInt32(), br.ReadInt32(), br.BReadBoundBoxShort(), br.BReadUInt16(),
			                   br.BReadUInt16(), br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte());
		}
		
		public static Edge BReadEdge(this BinaryReader br)
		{
			return new Edge(br.BReadUInt16(), br.BReadUInt16());
		}
		
		public static BoundBox BReadBoundBox(this BinaryReader br)
		{
			return new BoundBox(br.ReadVector3f(), br.ReadVector3f());
		}
		
		public static Model BReadModel(this BinaryReader br)
		{
			return new Model(br.BReadBoundBox(), br.ReadVector3f(), br.ReadInt32(), br.ReadInt32(), 
			                 br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
	
		}
		
		#endregion
	}

	#region Structs
	
	public struct DirectoryEntry
	{
		public DirectoryEntry(int offset, int size)
		{
			this.offset = offset;
			this.size = size;
		}
		
		public int offset;
		public int size;
	}
		
	public struct Vector3f
	{
		public Vector3f(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		
		public float x,y,z;
		
		public static int Size { get { return sizeof(float) * 3; } }
	}
	
	public struct Plane
	{
		public Vector3f normal;
		public float distance;
		public int type;
		
		public static int Size
		{
			get { return (4+4+4)+4+8; }
		}
	}
	
	public struct BoundBox
	{
		public BoundBox(Vector3f minimum, Vector3f maximum)
		{
			this.minimum = minimum;
			this.maximum = maximum;
		}
		public Vector3f minimum;
		public Vector3f maximum;
		
		public static int Size { get { return Vector3f.Size + Vector3f.Size; } }
	}
	
	public struct MipTexture
	{
		public MipTexture(string name, int width, int height, int offset1, int offset2, int offset3, int offset4)
		{
			this.name = name;
			
			this.width = width;
			this.height = height;
			
			this.offset1 = offset1;
			this.offset2 = offset2;
			this.offset3 = offset3;
			this.offset4 = offset4;
		}
		
		public string name;
		
		public int width;
		public int height;
		
		public int offset1;
		public int offset2;
		public int offset3;
		public int offset4;
		
		public static int Size
		{
			get { return 16 + 8 + 4 * 8; }
		}
	}
	
	public struct BSPNode
	{
		public BSPNode(int plane_id, ushort front, ushort back, BoundBoxShort box, ushort face_id, ushort face_num)
		{
			this.plane_id = plane_id;
			this.front = front;
			this.back = back;
			this.box = box;
			this.face_id = face_id;
			this.face_num = face_num;
		}
		
		public int plane_id;
		public ushort front;
		public ushort back;
		public BoundBoxShort box;
		public ushort face_id;
		public ushort face_num;
		
		public static int Size
		{
			get { return 4 + 2 + 2 + BoundBoxShort.Size + 2 + 2; }
		}
	}
	
	public struct BoundBoxShort
	{
		public BoundBoxShort(short min, short max)
		{
			this.min = min;
			this.max = max;
		}
		
		public short min;
		public short max;
		
		public static int Size
		{
			get { return 2 + 2; }
		}
	}
	
	public struct FaceTextureInfo
	{
		public FaceTextureInfo(Vector3f vectorS, float distS, Vector3f vectorT, 
		               float distT, uint texture_id, uint animated)
		{
			this.vectorS = vectorS;
			this.distS = distS;
			this.vectorT = vectorT;
			this.distT = distT;
			this.texture_id = texture_id;
			this.animated = animated;
		}
		
		public Vector3f vectorS;
		public float distS;
		public Vector3f vectorT;
		public float distT;
		public uint texture_id;
		public uint animated;
		
		
		public static int Size { get { return (3 * 4 + 4) * 2 + 4 + 4; } }
	}
	
	public struct Face
	{
		public Face(ushort plane_id, ushort side, uint ledge_id, ushort ledge_num,
		            ushort texinfo_id, byte typelight, byte baselight, byte light1,
		            byte light2, int lightmap)
		{
			this.plane_id = plane_id;
			
			this.side = side;
			this.ledge_id = ledge_id;
			
			this.ledge_num = ledge_num;
			this.texinfo_id = texinfo_id;
			
			this.typelight = typelight;
			this.baselight = baselight;
			this.light1 = light1;
			this.light2 = light2;
			this.lightmap = lightmap;
		}
		            
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
		
		public static int Size { get { return 2 + 2 + 4 + 2 + 2 + 4 + 4; } }
	}
	
	public struct ClipNode
	{
		public ClipNode(uint planenum, short front, short back)
		{
			this.planenum = planenum;
			this.front = front;
			this.back = back;
		}
		
		public uint planenum;
		public short front;
		public short back;
		
		public static int Size { get { return 4 + 2 + 2; } }
	}
	
	public struct BSPLeaf
	{
		public BSPLeaf(int type, int vislist, BoundBoxShort bound, ushort lface_id, ushort lface_num,
		               byte sndwater, byte sndsky, byte sndslime, byte sndlava)
		{
			this.type = type;
			this.vislist = vislist;
			this.bound = bound;
			this.lface_id = lface_id;
			this.lface_num = lface_num;
			this.sndwater = sndwater;
			this.sndsky = sndsky;
			this.sndslime = sndslime;
			this.sndlava = sndlava;
		}
		
		public int type;
		public int vislist;
		public BoundBoxShort bound;
		public ushort lface_id;
		public ushort lface_num;
		public byte sndwater;
		public byte sndsky;
		public byte sndslime;
		public byte sndlava;
		
		public static int Size { get { return 4 + 4 + BoundBoxShort.Size + 2 + 2 + 4; } }
	}
	
	public struct Edge
	{
		public Edge(ushort vertex0, ushort vertex1)
		{
			this.vertex0 = vertex0;
			this.vertex1 = vertex1;
		}
		
		public ushort vertex0;
		public ushort vertex1;
		
		public static int Size { get { return 2 + 2; } }
	}
	
	public struct Model
	{
		public Model(BoundBox bound, Vector3f origin, int node_id0, int node_id1,
		             int node_id2, int node_id3, int numleafs, int face_id, int face_num)
		{
			this.bound = bound;
			this.origin = origin;
			this.node_id0 = node_id0;
			this.node_id1 = node_id1;
			this.node_id2 = node_id2;
			this.node_id3 = node_id3;
			this.numleafs = numleafs;
			this.face_id = face_id;
			this.face_num = face_num;
		}
		
		public BoundBox bound;
		public Vector3f origin;
		public int node_id0;
		public int node_id1;
		public int node_id2;
		public int node_id3;
		public int numleafs;
		public int face_id;
		public int face_num;
		
		public static int Size { get { return BoundBox.Size + Vector3f.Size + 4 * 4 + 4 * 3; } }
	}
	
	#endregion
	
	public class BSPParser
	{
		#region Delegates
		//public delegate void LoadDirectoryTableDelegate();
		public delegate void LoadEntitiesDelegate(string entities);
		public delegate void LoadPlaneDelegate(Plane plane);
		public delegate void LoadMipTextureDelegate(MipTexture texture);
		public delegate void LoadVertexDelegate(Vector3f vertex);
		public delegate void LoadBSPNode(BSPNode node);
		public delegate void LoadFaceTextureInfoDelegate(FaceTextureInfo textureInfo);
		public delegate void LoadFaceDelegate(Face face);
		public delegate void LoadLightMapDelegate(byte lightmap);
		public delegate void LoadClipNodeDelegate(ClipNode node);
		public delegate void LoadBSPLeafDelegate(BSPLeaf leaf);
		public delegate void LoadFaceListElementDelegate(short face_index);
		public delegate void LoadEdgeDelegate(Edge edge);
		public delegate void LoadEdgeListElementDelegate(short edge);
		public delegate void LoadModelDelegate(Model model);
		#endregion
		
		#region Events
		//public event LoadDirectoryTableDelegate OnLoadDirectoryTable;
		public event LoadEntitiesDelegate        OnLoadEntities;
		public event LoadPlaneDelegate           OnLoadPlane;
		public event LoadMipTextureDelegate      OnLoadMipTexture;
		public event LoadVertexDelegate          OnLoadVertex;
		public event LoadBSPNode                 OnLoadBSPNode;
		public event LoadFaceTextureInfoDelegate OnLoadFaceTextureInfo;
		public event LoadFaceDelegate            OnLoadFace;
		public event LoadLightMapDelegate        OnLoadLightMap;
		public event LoadClipNodeDelegate        OnLoadClipNode;
		public event LoadBSPLeafDelegate         OnLoadBSPLeaf;
		public event LoadFaceListElementDelegate OnLoadFaceListElement;
		public event LoadEdgeDelegate            OnLoadEdge;
		public event LoadEdgeListElementDelegate OnLoadEdgeListElement;
		public event LoadModelDelegate           OnLoadModel;	    
	    #endregion
		
		private BinaryReader br;
		
		public BSPParser(Stream stream)
		{
			br = new BinaryReader(stream, Encoding.ASCII);
		}
		
		#region Load Functions
		
		public bool LoadDirectoryTables()
		{
			try 
			{
				Version = br.BReadInt32();
				
				// directories
				Entities       = br.BReadDirectoryEntry();
				Planes         = br.BReadDirectoryEntry();
				MipTextures    = br.BReadDirectoryEntry();
				Vertices       = br.BReadDirectoryEntry();
				VisibilityList = br.BReadDirectoryEntry();
				Nodes          = br.BReadDirectoryEntry();
				TextureInfo    = br.BReadDirectoryEntry();
				Faces          = br.BReadDirectoryEntry();
				Lightmaps      = br.BReadDirectoryEntry();
				Clipnodes      = br.BReadDirectoryEntry();
				Leaves         = br.BReadDirectoryEntry();
				FaceList       = br.BReadDirectoryEntry();
				Edges          = br.BReadDirectoryEntry();
				EdgeList       = br.BReadDirectoryEntry();
				Models         = br.BReadDirectoryEntry();
				
				//if (OnLoadDirectoryTable != null) OnLoadDirectoryTable();
			} catch { return false; }
			
			return true;
		}
		
		public bool LoadEntities()
		{
			if (OnLoadEntities != null) OnLoadEntities(ReadEntities());
			
			return true;
		}
		
		public string ReadEntities()
		{
			br.BaseStream.Seek(Entities.offset, SeekOrigin.Begin);			
			return Encoding.ASCII.GetString(br.ReadBytes((int)Entities.size));
		}
		
		public bool LoadPlanes()
		{
			br.BaseStream.Seek(Planes.offset, SeekOrigin.Begin);
			for (int i = Planes.offset; i < Planes.offset + Planes.size; i += Plane.Size)
			{
				Plane p;
				p.normal = br.ReadVector3f();
				p.distance = br.ReadSingle();
				p.type = br.BReadInt32();
				if (OnLoadPlane != null) OnLoadPlane(p);
			}
			return true;
		}
		
		public bool LoadMipTextureOffsets()
		{
			br.BaseStream.Seek(MipTextures.offset, SeekOrigin.Begin);
			int size = br.BReadInt32();
			MipTextureOffsets = new int[size];
			for (int j = 0; j < size; j++) MipTextureOffsets[j] = br.BReadInt32();
			return true;
		}
		public bool LoadMipTextures()
		{
			if (MipTextureOffsets == null) return false;
			foreach(int offset in MipTextureOffsets)
			{
				br.BaseStream.Seek(MipTextures.offset + offset, SeekOrigin.Begin);
				OnLoadMipTexture(br.BReadMipTexture());
			}
			return true;
		}
		
		public bool LoadVertices()
		{
			br.BaseStream.Seek(Vertices.offset, SeekOrigin.Begin);
			for (int i = Vertices.offset; i < Vertices.offset + Vertices.size; i += Vector3f.Size)
			{
				OnLoadVertex(br.ReadVector3f());
			}
			return true;
		}
		
		public bool LoadBSPNodes()
		{
			br.BaseStream.Seek(Nodes.offset, SeekOrigin.Begin);
			
			for (int i = Nodes.offset; i < Nodes.offset + Nodes.size; i += BSPNode.Size)
			{
				OnLoadBSPNode(br.BReadBSPNode());
			}
			
			return true;
		}
		
		public bool LoadFaceTextureInfo()
		{
			br.BaseStream.Seek(TextureInfo.offset, SeekOrigin.Begin);
			for (int i = TextureInfo.offset; i < TextureInfo.offset + TextureInfo.size; i += FaceTextureInfo.Size)
			{
				if (OnLoadFaceTextureInfo != null) OnLoadFaceTextureInfo(br.BReadFaceTextureInfo());
			}
			return true;
		}
		
		public bool LoadFaces()
		{
			br.BaseStream.Seek(Faces.offset, SeekOrigin.Begin);
			for (int i = Faces.offset; i < Faces.offset + Faces.size; i += Face.Size)
			{
				if (OnLoadFace != null) OnLoadFace(br.BReadFace());
			}
			return true;
		}
		
		public bool LoadLightMaps()
		{
			if (OnLoadLightMap != null)
			{
				br.BaseStream.Seek(Lightmaps.offset, SeekOrigin.Begin);
				for (int i = Lightmaps.offset; i < Lightmaps.offset + Lightmaps.size; i++) // one byte
					OnLoadLightMap(br.ReadByte());
			}
			return true;
		}
		
		public bool LoadClipNodes()
		{
			if (OnLoadClipNode != null)
			{
				br.BaseStream.Seek(Clipnodes.offset, SeekOrigin.Begin);
				for (int i = Clipnodes.offset; i < Clipnodes.offset + Clipnodes.size; i += ClipNode.Size)
					OnLoadClipNode(br.BReadClipNode());
			}
			return true;
		}
		
		public bool LoadBSPLeaves()
		{
			if (OnLoadBSPLeaf != null)
			{
				br.BaseStream.Seek(Leaves.offset, SeekOrigin.Begin);
				for (int i = Leaves.offset; i < Leaves.offset + Leaves.size; i += BSPLeaf.Size)
					OnLoadBSPLeaf(br.BReadBSPLeaf());
				
			}
			return true;
		}
		
		public bool LoadFaceList()
		{
			if (OnLoadFaceListElement != null)
			{
				br.BaseStream.Seek(FaceList.offset, SeekOrigin.Begin);
				for (int i = FaceList.offset; i < FaceList.offset + FaceList.size; i+= Face.Size)
					OnLoadFaceListElement(br.BReadInt16());
			}
			return true;
		}
		
		public bool LoadEdges()
		{
			if (OnLoadEdge != null)
			{
				br.BaseStream.Seek(Edges.offset, SeekOrigin.Begin);
				for (int i = Edges.offset; i < Edges.offset + Edges.size; i += Edge.Size)
					OnLoadEdge(br.BReadEdge());
			}
			return true;
		}
		
		public bool LoadEdgeList()
		{
			if (OnLoadEdgeListElement != null)
			{
				br.BaseStream.Seek(EdgeList.offset, SeekOrigin.Begin);
				for (int i = EdgeList.offset; i < EdgeList.offset + EdgeList.size; i += 2)
					OnLoadEdgeListElement(br.BReadInt16());
			}
			return true;
		}
		
		public bool LoadModels()
		{
			if (OnLoadModel != null)
			{
				br.BaseStream.Seek(Models.offset, SeekOrigin.Begin);
				for (int i = Models.offset; i < Models.offset + Models.size; i += Model.Size)
					OnLoadModel(br.BReadModel());
				
			}
			return true;
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
	}
}

