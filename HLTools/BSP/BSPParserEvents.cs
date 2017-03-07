using System;
using System.IO;

using HLTools.Extensions;

using Vector3f = OpenTK.Vector3;

namespace HLTools.BSP
{
	public class BSPParserEvents : BSPParser
	{
		public BSPParserEvents(Stream stream)
			: base(stream)
		{
		}

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

		private bool Load<T>(DirectoryEntry entry, Func<T> read, Action<T> onLoad) where T : struct
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

		public bool LoadEntities()
		{
			if (OnLoadEntities != null) {
				OnLoadEntities(ReadEntities());
			}

			return true;
		}

		public bool LoadPlanes()
		{
			return Load<Plane>(Planes, br.ReadStruct<Plane>, OnLoadPlane);
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

		public bool LoadVertices()
		{
			return Load<Vector3f>(Vertices, br.ReadStruct<Vector3f>, OnLoadVertex);
		}

		public bool LoadBSPNodes()
		{
			return Load<BSPNode>(Nodes, br.ReadStruct<BSPNode>, OnLoadBSPNode);
		}

		public bool LoadFaceTextureInfo()
		{
			return Load<FaceTextureInfo>(TextureInfo, br.ReadStruct<FaceTextureInfo>, OnLoadFaceTextureInfo);
		}

		public bool LoadFaces()
		{
			return Load<Face>(Faces, br.ReadStruct<Face>, OnLoadFace);
		}

		public bool LoadLightMaps()
		{
			return Load<byte>(Lightmaps, br.ReadByte, OnLoadLightMap);
		}

		public bool LoadClipNodes()
		{
			return Load<ClipNode>(Clipnodes, br.ReadStruct<ClipNode>, OnLoadClipNode);
		}

		public bool LoadBSPLeaves()
		{
			return Load<BSPLeaf>(Leaves, br.ReadStruct<BSPLeaf>, OnLoadBSPLeaf);
		}

		public bool LoadFaceList()
		{
			return Load<short>(FaceList, br.ReadInt16, OnLoadFaceListElement);
		}

		public bool LoadEdges()
		{
			return Load<Edge>(Edges, br.ReadStruct<Edge>, OnLoadEdge);
		}

		public bool LoadEdgeList()
		{
			return Load<int>(EdgeList, br.BReadInt32, OnLoadEdgeListElement);
		}

		public bool LoadModels()
		{
			return Load<Model>(Models, br.ReadStruct<Model>, OnLoadModel);
		}
	}
}

