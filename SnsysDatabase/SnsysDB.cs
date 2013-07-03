using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace SnsysDatabase
{
	public static class SDBHelper {
		public static byte[] Precode = Encoding.UTF8.GetBytes("SSDB");
	}
	public class SnsysStringDataBase {
		public static string Type = "StringSDB";
		private Dictionary<string, string[]> Contents;
		public SnsysStringDataBase () {
			this.Contents = new Dictionary<string, string[]> ();
		}
		public SnsysStringDataBase (Dictionary<string, string[]> D) {
			this.Contents = D;
		}
		public string[] this[string s] {
			get {return this.Contents [s];}
			set {this.Contents [s] = value;}
		}
		public void Write (string dir, bool fromCurrent = true) {
			FileStream writeStream = File.OpenWrite (fromCurrent?Path.Combine(Environment.CurrentDirectory, dir):dir);
			writeStream.Write (SDBHelper.Precode, 0, SDBHelper.Precode.Length);
			byte[] nameBytes = Encoding.UTF8.GetBytes (Type);
			writeStream.Write (BitConverter.GetBytes(BitConverter.IsLittleEndian), 0, 1);
			writeStream.Write (BitConverter.GetBytes((uint)nameBytes.Length), 0, 4);
			writeStream.Write (nameBytes, 0, nameBytes.Length);
		}
	}
}

