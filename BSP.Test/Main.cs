using System;
using System.IO;
using HLTools.BSP;

namespace HLTools.BSP.Test
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length > 0) {
				foreach (string map in args) {	
					LoadBSPMap(map);	
				}
			} else {
				LoadBSPMap("de_dust.bsp");
				// LoadEntities("de_dust.bsp");
				// LoadEntities2("de_dust.bsp");
			}
		}
		
		public static void LoadEntities(string file)
		{
			FileStream fs = File.OpenRead(file);
			BSPParser p = new BSPParser(fs);
			if (p.LoadDirectoryTables()) {
				string entities = p.ReadEntities();
				EntityParser ep = new EntityParser(entities);
				
				foreach (var entity in ep.Entities) {
					Console.Write("{");
					foreach (var kvp in entity) {
						Console.Write(kvp);	
						Console.Write(",");
					}
					Console.WriteLine("}");
				}
				
				Console.WriteLine ();
			}
		}
		
		public static void LoadEntities2(string file)
		{
			FileStream fs = File.OpenRead(file);
			BSPParser p = new BSPParser(fs);
			if (p.LoadDirectoryTables()) {
				string entities = p.ReadEntities();
				Console.WriteLine (entities);
				EntityParser ep = new EntityParser(entities);
				var ent = ep.ReadEntities();
				
				Console.WriteLine (ent["worldspawn"][0]["wad"]);
				Console.WriteLine (ent["worldspawn"][0]["skyname"]);
				if (ent.ContainsKey("ambient_generic")) {
					var sounds = ent["ambient_generic"];
					foreach (var sound in sounds) {
						Console.WriteLine (sound["message"]);	
					}
				}
			}
		}
		
		public static void LoadBSPMap(string file)
		{
			FileStream fs = File.OpenRead(file);
			Console.WriteLine ("Parsing bsp file: {0}\n", fs.Name);
			BSPParser p = new BSPParser(fs);
			Console.WriteLine ("Loading Directory table");
			if (p.LoadDirectoryTables())
			{
				p.OnLoadEntities += delegate(string entities) { };
				Console.WriteLine ("Loading Entities");
				p.LoadEntities();
				
				p.OnLoadPlane += delegate(Plane plane) { };
				Console.WriteLine ("Loading Planes");
				p.LoadPlanes();
				
				p.OnLoadMipTexture += delegate(MipTexture texture) { };
				if (p.LoadMipTextureOffsets())
				{
					Console.WriteLine ("Loading Mip Textures");
					p.LoadMipTextures();
				}
				
				p.OnLoadVertex += delegate(Vector3f vertex) { };
				Console.WriteLine ("Loading Vertices");
				p.LoadVertices();
				
				p.OnLoadBSPNode += delegate(BSPNode node) { };
				Console.WriteLine ("Loading BSP nodes");
				p.LoadBSPNodes();
				
				p.OnLoadFaceTextureInfo += delegate(FaceTextureInfo textureInfo) { };
				Console.WriteLine ("Loading Surface Texture Info");
				p.LoadFaceTextureInfo();
				
				p.OnLoadFace += delegate(Face face) { };
				Console.WriteLine ("Loading Faces");
				p.LoadFaces();
				
				p.OnLoadLightMap += delegate(byte lightmap) { };
				Console.WriteLine ("Loading lightmaps");
				p.LoadLightMaps();
				
				p.OnLoadClipNode += delegate(ClipNode node) { };
				Console.WriteLine ("Loading ClipNode");
				p.LoadClipNodes();
				
				p.OnLoadBSPLeaf += delegate(BSPLeaf leaf) { };
				Console.WriteLine ("Loading BSP leaves");
				p.LoadBSPLeaves();
				
				p.OnLoadFaceListElement += delegate(short face_index) { };
				Console.WriteLine ("Loading Face index list");
				p.LoadFaceList();
				
				p.OnLoadEdge += delegate(Edge edge) { };
				Console.WriteLine ("Loading Edges");
				p.LoadEdges();
				
				p.OnLoadEdgeListElement += delegate(short edge) { };
				Console.WriteLine ("Loading Edge List");
				p.LoadEdgeList();
				
				p.OnLoadModel += delegate(Model model) { };
				Console.WriteLine ("Loading Models");
				p.LoadModels();
			}
			
			fs.Close();
			fs.Dispose();
		}
	}
}

