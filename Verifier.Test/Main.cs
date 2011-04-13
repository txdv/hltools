using System;
using System.IO;
using System.Collections.Generic;
using HLTools;

namespace HLTools.Test
{
	class MainClass
	{
		public static int CalculateBadPoints(VerifierResult res)
		{
			return res.MalformedWadFiles.Length +
				   res.MisnamedModDirs.Length * 2 +
				   res.NotExistingFiles.Length * 4;
		}

		public static void Handle(Verifier v, string file)
		{
			var res = v.VerifyMap(file);

			Console.WriteLine("{0}:", file);

			if (res.MalformedWadFiles.Length > 0) {
				Console.WriteLine("The referenced wad files are malformed:");
				foreach (string wadfile in res.MalformedWadFiles) {
					Console.WriteLine("  {0}", wadfile);
				}
			}

			if (res.MisnamedModDirs.Length > 0) {
				Console.WriteLine("The referenced mod directories do not exist (misnamed):");
				foreach (string wadfile in res.MisnamedModDirs) {
					Console.WriteLine("  {0}", wadfile);
				}
			}

			if (res.NotExistingFiles.Length > 0) {
				Console.WriteLine("The referenced wad files do not exist:");
				foreach (string wadfile in res.NotExistingFiles) {
					Console.WriteLine("  {0}", wadfile);
				}
			}

			if (res.MissingTextures.Length > 0) {
				Console.WriteLine("Missing textures: ");
				foreach (string texture in res.MissingTextures) {
					Console.WriteLine("  {0}", texture);
				}
				Console.WriteLine("Could be in these wad files:");
				foreach (string missingWad in res.NotExistingFiles) {
					Console.WriteLine("  {0}", missingWad);
				}
			}

			if (res.MissingFileList.Length > 0) {
				Console.WriteLine("These file are missing in the installation:");
				foreach (string missingFile in res.MissingFileList) {
					Console.WriteLine("  {0}", missingFile);
				}
			}

			Console.WriteLine("  Bad points: {0}", CalculateBadPoints(res));

			Console.WriteLine();
		}

		public static void Main(string[] args)
		{
			Verifier v = new Verifier(args[0], args[1]);

			if (args.Length > 2) {
				Handle(v, args[2]);
			} else {
				foreach (var file in Directory.GetFiles(v.ModMapsDirectory, "*.bsp")) {
					FileInfo fi = new FileInfo(file);
					Handle(v, fi.Name);
				}
			}
		}
	}
}

