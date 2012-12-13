
using System;
using System.IO;
using System.Collections.Generic;
using HLTools.WAD;

namespace HLTools.WAD.Test
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length > 0) {
				foreach (string wad in args) {
					LoadWADFile(wad);
				}
			} else {
				LoadWADFile("halflife.wad");
			}
		}
		public static void LoadWADFile(string filename)
		{
			Console.WriteLine("Parsing WAD file: {0}\n", filename);
			FileStream fs = File.OpenRead(filename);

			WADParser wadp = new WADParser(fs);
			Console.WriteLine("Files in directory: {0}", wadp.FileCount);

			wadp.OnLoadFile += (file) => { };
			Console.WriteLine("Loading file information");
			wadp.LoadFiles();

			fs.Close();
			fs.Dispose();
		}
	}
}

