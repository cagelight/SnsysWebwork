using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace SnsysDatabase
{
	public static class SDBHelper {
		public static byte[] Precode = Encoding.UTF8.GetBytes("SSDB");
	}
	public class SingleLayerSnsysDataBase {
		public static string Type = "StringSDB";
		private Dictionary<string, IDatablock[]> Contents;
		public SingleLayerSnsysDataBase () {
			this.Contents = new Dictionary<string, IDatablock[]> ();
		}
		public IDatablock[] this[string s] {
			get {return this.Contents [s];}
			set {this.Contents [s] = value;}
		}
		public void Write (string dir, bool fromCurrent = true) {
			try {
				string path = fromCurrent?Path.Combine(Environment.CurrentDirectory, dir):dir;
				FileStream writeStream = File.Create(path);
				writeStream.Write (SDBHelper.Precode, 0, SDBHelper.Precode.Length); //SSDB (Type header)
				byte[] nameBytes = Encoding.UTF8.GetBytes (Type);
				writeStream.Write (BitConverter.GetBytes(BitConverter.IsLittleEndian), 0, 1); //Is this file written little endian? (BOOL)
				writeStream.Write (BitConverter.GetBytes((ushort)nameBytes.Length), 0, 2); //Number of bytes to read for SDB Subtype. (USHORT)
				writeStream.Write (nameBytes, 0, nameBytes.Length); //Subtype name bytes.
				writeStream.Write (BitConverter.GetBytes((ushort)this.Contents.Count), 0, 2); //How many lines in the single layer dictionary. (USHORT)
				foreach (KeyValuePair<string, IDatablock[]> KVP in this.Contents) {
					byte[] keyBytes = Encoding.UTF8.GetBytes(KVP.Key);
					writeStream.Write(BitConverter.GetBytes((ushort)keyBytes.Length), 0, 2); //Number of bytes to read for key name. (USHORT)
					writeStream.Write(keyBytes, 0, keyBytes.Length); //Key name bytes.
					writeStream.Write(BitConverter.GetBytes((ushort)KVP.Value.Length), 0, 2); //Number of entries for this key. (USHORT)
					foreach (IDatablock DB in KVP.Value) {
						writeStream.WriteByte(DB.Callsign); //Type callsign (refer to Datablock.cs) (BYTE)
						byte[] rawBytes = DB.GetRaw();
						if (DB.Callsign == 0x00) {writeStream.Write(BitConverter.GetBytes((ushort)rawBytes.Length), 0, 2);} //If type is string, how long is string? (USHORT)
						writeStream.Write(rawBytes, 0, rawBytes.Length); //Value bytes.
					}
				}

			} catch (Exception e) {
				Console.WriteLine ("--- A FATAL WRITE ERROR HAS OCCURED ---");
				Console.WriteLine (e);
			}
		}
	}
}

