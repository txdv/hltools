using System;
using System.IO;
using System.Runtime.InteropServices;

namespace HLTools.Extensions
{
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
}

