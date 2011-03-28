using System;
using System.IO;
using System.Collections.Generic;
using HLTools.BSP;
using HLTools.WAD;

namespace HLTools
{
	public enum ValveFileDifference
	{
		Equal,
		EqualFile,
		NotEqual
	};
	
	public static class StringExtensions
	{
		public static string GetBareFilename(this string filename)
		{
			var list = filename.Split(new char[] { '/', '\\' });
			return list[list.Length - 1];
		}
		
		public static string GetModDir(this string filename)
		{
			var list = filename.Split(new char[] { '/', '\\' });
			return list[list.Length - 2];
		}
		
		public static string GetRelevantPath(this string filename)
		{
			return Path.Combine(GetModDir(filename), GetBareFilename(filename));
		}
		
	}
	
	public class ValveFile
	{
		public ValveFile(string moddir, string file)
		{
			ModDirectory = moddir;
			File = file;
		}
		
		public ValveFile(string fullpath)
			: this(fullpath.GetModDir(), fullpath.GetBareFilename())
		{
		}
		
		public string ModDirectory { get; protected set; }
		public string File { get; protected set; }
		
		public string ToString(string baseDirectory)
		{
			return baseDirectory + ToString();
		}
		
		public override string ToString()
		{
			return Path.Combine(ModDirectory, File);
		}
		
		public ValveFileDifference Compare(ValveFile file)
		{
			if (File == file.File) {
				if (ModDirectory == file.ModDirectory) { 
					return ValveFileDifference.Equal;
				} else { 
					return ValveFileDifference.EqualFile;
				}
			} else {
				return ValveFileDifference.NotEqual;
			}
		}
	}
	
	public class Verifier
	{
		public readonly string ValveDir = "valve";
		public readonly string MapDir = "maps";
		public readonly string[] SkyEndings = new string[] { "bk", "ft", "dn", "up" , "rt", "lf" };
		public readonly string[] SkyExtensions = new string[] { ".bmp", ".tga" };
		
		public string BaseDirectory { get; protected set; }
		public string ModDirectory { get; protected set; }
		
		public string ModMapsDirectory { get; protected set; }
		
		public string ValveDirectory { get { return Path.Combine(BaseDirectory, ValveDir); } }
		
		public Verifier(string basedir, string mod)
		{
			BaseDirectory = basedir;
			ModDirectory = Path.Combine(basedir, mod);
			ModMapsDirectory = Path.Combine(ModDirectory, MapDir);
		}
		
		public ValveFile[] GetWadFiles()
		{
			List<ValveFile> list = new List<ValveFile>();
			foreach (string file in Directory.GetFiles(ValveDirectory, "*.wad")) {
				list.Add(new ValveFile(file));
			}
			
			foreach (string file in Directory.GetFiles(ModDirectory, "*.wad")) {
				list.Add(new ValveFile(file));
			}
			
			return list.ToArray();
		}
		
		public ValveFile GetWadFile(ValveFile[] existingFiles, ValveFile file)
		{
			ValveFile potential = null;
			foreach (ValveFile existingFile in existingFiles) {
				ValveFileDifference result = file.Compare(existingFile);
				if (result == ValveFileDifference.Equal) {
					return file;
				} else if (result == ValveFileDifference.EqualFile) {
					potential = existingFile;	
				}
			}
			
			return (potential == null ? null : potential);
		}
		
		public string[] Seperate(string wad)
		{
			List<string> list = new List<string>();
			foreach (string wadfile in wad.Split(new char[] { ';' })) {
				if (wadfile.Length == 0) {
					continue;
				}
				list.Add(wadfile);
			}
			return list.ToArray();
		}
		
		public delegate void MalformedWadFileDelegate(string wadfile);
		public event MalformedWadFileDelegate MalformedWadFile;
		public event MalformedWadFileDelegate MisnamedModDir;
		public event MalformedWadFileDelegate FileNotExistent;
		
		public void VerifyMap(string map)
		{
			ValveFile[] wadfiles = GetWadFiles();
			
			var fullmap = Path.Combine(ModMapsDirectory, map);
			
			if (!File.Exists(fullmap)) {
				throw new Exception(string.Format("Map does not exist: {0}", fullmap));
			}
			
			BSPParser bp = new BSPParser(File.OpenRead(fullmap));
			
			if (!bp.LoadDirectoryTables()) {
				throw new Exception("Malformed file");
			}

			List<string> textureList = new List<string>();
			bp.LoadMipTextureOffsets();
			bp.OnLoadMipTexture += delegate(MipTexture texture) {
				if ((texture.offset1 == 0) && (texture.offset2 == 0) && (texture.offset3 == 0) && (texture.offset4 == 0)) {
					textureList.Add(texture.Name);
				}
			};
			bp.LoadMipTextures();
			
			string entities = bp.ReadEntities();
			
			EntityParser ep = new EntityParser(entities);
			
			var ent = ep.ReadEntities();
			
			if (ent.ContainsKey("worldspawn")) {
				string skyname = ent["worldspawn"][0]["skyname"];
				string wadList = ent["worldspawn"][0]["wad"];
				var list = Seperate(wadList);
				
				List<ValveFile> existingWads = new List<ValveFile>();
				foreach (string fullReferencedWadFile in list) {
					
					string referencedRelevantWadFile = fullReferencedWadFile.GetRelevantPath();
					
					ValveFile referencedFile = new ValveFile(referencedRelevantWadFile);
					ValveFile existingFile = GetWadFile(wadfiles, referencedFile);
					
					if (referencedRelevantWadFile != fullReferencedWadFile) {
						if (MalformedWadFile != null) {
							MalformedWadFile(fullReferencedWadFile);
						}
					}
					
					if (existingFile != null) {
						if (existingFile.Compare(referencedFile) == ValveFileDifference.EqualFile) {
							if (MisnamedModDir != null) {	
								MisnamedModDir(fullReferencedWadFile);	
							}
						}
						
						existingWads.Add(existingFile);
					} else {
						if (FileNotExistent != null) {	
							FileNotExistent(fullReferencedWadFile);
						}
					}
				}
				
				List<string> existingFilenames = new List<string>();
				foreach (var wadFile in existingWads) {
					WADParser wp = new WADParser(File.OpenRead(wadFile.ToString(BaseDirectory)));
					wp.OnLoadFile += delegate(WADFile file) {
						existingFilenames.Add(file.Filename.ToLower());
					};
					wp.LoadFiles();
				}
				
				int missingTextures = 0;
				foreach (var item in textureList) {
					if (!existingFilenames.Contains(item.ToLower())) {
						Console.WriteLine (item);
						missingTextures++;
					}
				}
			}
		}
	}
}
