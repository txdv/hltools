using System;
using System.IO;
using HLTools;

namespace HLTools.Test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Verifier v = new Verifier(args[0], args[1]);
			
			int badpoints = 0;
			
			v.MalformedWadFile += delegate(string wadfile) {
				Console.WriteLine ("  The referenced wad file is malformed: {0}", wadfile);
				badpoints += 1;
			};
			
			v.MisnamedModDir += delegate(string wadfile) {
				Console.WriteLine ("  Mod dir does not exist: {0}", wadfile);
				badpoints += 2;
			};
			
			v.FileNotExistent += delegate(string wadfile) {
				Console.WriteLine ("  File does not exist: {0}", wadfile);
				badpoints += 4;
			};
			
			if (args.Length > 2) {
				v.VerifyMap(args[2]);
				
			} else {
				foreach (var file in Directory.GetFiles(v.ModMapsDirectory, "*.bsp")) {
					FileInfo fi = new FileInfo(file);
					
					Console.WriteLine (fi.Name);
					v.VerifyMap(fi.Name);
					Console.WriteLine (badpoints);
					badpoints = 0;
				}
			}
		}
	}
}

