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
			// HACK: this happens, apparently /models/bla.mdl = models/bla.mdl
			if (file.StartsWith("/")) {
				file = file.Substring(1);	
			}
			
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

	public class VerifierResult
	{
		public VerifierResult()
		{
		}

		public string[] MalformedWadFiles { get; set; }
		public string[] MisnamedModDirs   { get; set; }
		public string[] NotExistingFiles  { get; set; }
		public string[] MissingTextures   { get; set; }

		public string[] UsedSprites       { get; set; }
		public string[] MissingSprites    { get; set; }

		public string[] UsedSounds        { get; set; }
		public string[] MissingSounds     { get; set; }

		public string[] UsedModels        { get; set; }
		public string[] MissingModels     { get; set; }

		public bool ServerCapable {
			get {
				return MissingTextures.Length == 0;
			}
		}

		public bool ClientCapable {
			get {
				return (ServerCapable && (MissingSprites.Length == 0) && (MissingSounds.Length == 0) && (MissingModels.Length == 0));
			}
		}

		public string[] UsedFileList    { get; set; }
		public string[] MissingFileList { get; set; }
	}

	public class VerifierEvents
	{
		public VerifierEvents(Verifier verifier)
		{
			Verifier = verifier;
		}

		public Verifier Verifier { get; protected set; }

		public delegate void FileArrayDelegate(string[] UsedTextures);

		public event FileArrayDelegate MalformedWadFiles;
		public event FileArrayDelegate MisnamedModDirs;
		public event FileArrayDelegate NotExistingFiles;
		public event FileArrayDelegate MissingTextures;

		public event FileArrayDelegate UsedSprites;
		public event FileArrayDelegate MissingSprites;

		public event FileArrayDelegate UsedSounds;
		public event FileArrayDelegate MissingSounds;

		public event FileArrayDelegate UsedModels;
		public event FileArrayDelegate MissingModels;

		public void DispatchEvents(VerifierResult res)
		{
			if (MalformedWadFiles != null) {
				MalformedWadFiles(res.MalformedWadFiles);
			}

			if (MisnamedModDirs != null) {
				MisnamedModDirs(res.MisnamedModDirs);
			}

			if (NotExistingFiles != null) {
				NotExistingFiles(res.NotExistingFiles);
			}

			if (MissingTextures != null) {
				MissingTextures(res.MissingTextures);
			}

			if (UsedSprites != null) {
				UsedSprites(res.UsedSprites);
			}

			if (MissingSprites != null) {
				MissingSprites(res.MissingSprites);
			}

			if (UsedSounds != null) {
				UsedSounds(res.UsedSounds);
			}

			if (MissingSounds != null) {
				MissingSounds(res.MissingSounds);
			}

			if (UsedModels != null) {
				UsedModels(res.UsedModels);
			}

			if (MissingModels != null) {
				MissingModels(res.MissingModels);
			}
		}

		public void DispatchEvents(string map)
		{
			DispatchEvents(Verifier.VerifyMap(map));
		}
	}

	public class Verifier
	{
		public readonly string ValveDir = "valve";
		public readonly string MapDir = "maps";
		public readonly string SoundDir = "sound";
		public readonly string[] SkyEndings = new string[] { "bk", "ft", "dn", "up" , "rt", "lf" };
		public readonly string[] SkyExtensions = new string[] { ".bmp", ".tga" };
		
		public string ModDir { get; protected set; }
		public string BaseDirectory { get; protected set; }
		public string ModDirectory { get; protected set; }
		
		public string ModMapsDirectory { get; protected set; }
		
		public string ValveDirectory { get { return Path.Combine(BaseDirectory, ValveDir); } }
		
		public Verifier(string basedir, string mod)
		{
			BaseDirectory = basedir;
			ModDir = mod;
			ModDirectory = Path.Combine(basedir, ModDir);
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
		
		public bool FileExists(ValveFile file)
		{
			string path = Path.Combine(BaseDirectory, file.ToString());
			return File.Exists(path);
		}
		
		public ValveFile GetFile(string path)
		{
			ValveFile file = null;
			
			file = new ValveFile(ModDir, path);
			if (FileExists(file)) {
				return file;
			}
			
			file = new ValveFile(ValveDir, path);
			if (FileExists(file)) {
				return file;
			}
			
			return null;
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
		
		public VerifierResult VerifyMap(string map)
		{
			VerifierResult res = new VerifierResult();

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

			bp.Close();

			EntityParser ep = new EntityParser(entities);
			
			var ent = ep.ReadEntities();
			
			List<ValveFile> existingWads = new List<ValveFile>();
			List<string> malformedWadFiles = new List<string>();
			List<string> misnamedModDirs = new List<string>();
			List<string> notExistingFiles = new List<string>();

			if (ent.ContainsKey("worldspawn")) {
				string skyname = ent["worldspawn"][0]["skyname"];
				string wadList = ent["worldspawn"][0]["wad"];
				var list = Seperate(wadList);
				
				foreach (string fullReferencedWadFile in list) {
					
					string referencedRelevantWadFile = fullReferencedWadFile.GetRelevantPath();
					
					ValveFile referencedFile = new ValveFile(referencedRelevantWadFile);
					ValveFile existingFile = GetWadFile(wadfiles, referencedFile);
					
					if (referencedRelevantWadFile != fullReferencedWadFile) {
						malformedWadFiles.Add(fullReferencedWadFile);
					}
					
					if (existingFile != null) {
						if (existingFile.Compare(referencedFile) == ValveFileDifference.EqualFile) {
							misnamedModDirs.Add(fullReferencedWadFile);
						}
						
						existingWads.Add(existingFile);
					} else {
						notExistingFiles.Add(fullReferencedWadFile);
					}
				}

				res.MalformedWadFiles = malformedWadFiles.ToArray();
				res.MisnamedModDirs   =   misnamedModDirs.ToArray();
				res.NotExistingFiles  =  notExistingFiles.ToArray();

				#region Textures
				
				List<string> existingFilenames = new List<string>();
				foreach (var wadFile in existingWads) {
					WADParser wp = new WADParser(File.OpenRead(wadFile.ToString(BaseDirectory)));
					wp.OnLoadFile += delegate(WADFile file) {
						existingFilenames.Add(file.Filename.ToLower());
					};
					wp.LoadFiles();
					wp.Close();
				}
				
				List<string> missingTextures = new List<string>();
					
				foreach (var item in textureList) {
					if (!existingFilenames.Contains(item.ToLower())) {
						missingTextures.Add(item);
					}
				}

				res.MissingTextures = missingTextures.ToArray();

				#endregion
				
				
				#region Sprites
				
				List<string> sprites = new List<string>();
				if (ent.ContainsKey("env_sprite")) {
					foreach (var dict in ent["env_sprite"]) {
						string model = dict["model"];
						if (!sprites.Contains(model)) {
							sprites.Add(model);
						}
					}
				}

				res.UsedSprites = sprites.ToArray();

				List<string> missingSprites = new List<string>();
				foreach (string sprite in sprites) {
					ValveFile file = GetFile(sprite);
					if (file == null) {
						missingSprites.Add(sprite);
					}
				}
				res.MissingSprites = missingSprites.ToArray();

				#endregion
				
				#region Sounds
				
				List<string> sounds = new List<string>();
				if (ent.ContainsKey("ambient_generic")) {
					foreach (var ambient in ent["ambient_generic"]) {
						if (ambient.ContainsKey("message")) {
							string message = Path.Combine(SoundDir, ambient["message"]);
							if (!sounds.Contains(message)) {
								sounds.Add(message);
							}
						}
					}
				}

				res.UsedSounds = sounds.ToArray();

				List<string> missingSounds = new List<string>();
				
				foreach (string sound in sounds) {
					ValveFile file = GetFile(sound);
					if (file == null) {
						missingSounds.Add(sound);
					}
				}

				res.MissingSounds = missingSounds.ToArray();

				#endregion
				
				#region Models
				
				List<string> models = new List<string>();
				foreach (var entityPair in ent) {
					foreach (var entity in entityPair.Value) {
						if (entity.ContainsKey("model")) {
							string model = entity["model"];
							if (!model.EndsWith(".mdl")) {
								continue;
							}
							if (!models.Contains(model)) {
								models.Add(model);
							}
						}
					}
				}

				res.UsedModels = models.ToArray();

				List<string> missingModels = new List<string>();
				
				foreach (var model in models) {
					ValveFile file = GetFile(model);
					if (file == null) {
						missingModels.Add(model);
					}
				}

				res.MissingModels = missingModels.ToArray();

				#endregion

			}

			List<string> usedFileList = new List<string>();
			usedFileList.AddRange(res.UsedSprites);
			usedFileList.AddRange(res.UsedSounds);
			usedFileList.AddRange(res.UsedModels);
			usedFileList.Sort();
			res.UsedFileList = usedFileList.ToArray();

			List<string> missingFileList = new List<string>();
			missingFileList.AddRange(res.MissingSprites);
			missingFileList.AddRange(res.MissingSounds);
			missingFileList.AddRange(res.MissingModels);
			missingFileList.Sort();
			res.MissingFileList = missingFileList.ToArray();

			return res;
		}
	}
}
