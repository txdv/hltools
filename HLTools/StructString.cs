using System;
using System.Text;

namespace HLTools
{
	unsafe public struct StructString
	{
		public const int Length = 16;

		public StructString(string str)
		{
			var bytes = Encoding.ASCII.GetBytes(str);
			fixed (byte* ptr = @value) {
				int i = 0;
				int n = Math.Max(Math.Min(Length, bytes.Length) - 1, 0);
				for (i = 0; i < n; i++) {
						*(ptr + i) = bytes[i];
				}
				*(ptr + i) = 0;
			}
		}

		public fixed sbyte @value[Length];

		public override string ToString()
		{
			fixed (sbyte* ptr = @value) {
				return new string(ptr, 0, strlen(ptr, Length));
			}
		}

		int strlen(sbyte* ptr, int maxLength)
		{
			for (int i = 0; i < maxLength; i++) {
				if (*ptr == 0) {
					return i;
				}
				ptr++;
			}
			return 0;
		}

		public static explicit operator StructString(string str)
		{
			return new StructString(str);
		}
	}
}

