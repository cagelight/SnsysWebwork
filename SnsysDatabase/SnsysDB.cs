using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SnsysDatabase
{
	public static class SDBHelper {
		public static byte[] Precode = Encoding.UTF8.GetBytes("SSDB");
	}
	public class SingleLayerSnsysDataBase {
		public static SingleLayerSnsysDataBase NULL = new SingleLayerSnsysDataBase();
		public static string Type = "BasicSDB";
		protected Dictionary<string, IDatablock[]> Contents;
		public Dictionary<string, IDatablock[]> Database {
			get {return this.Contents;}
		}
		public string name = "NULL";
		public SingleLayerSnsysDataBase () {
			this.Contents = new Dictionary<string, IDatablock[]> ();
		}
		public SingleLayerSnsysDataBase (string name) {
			this.Contents = new Dictionary<string, IDatablock[]> ();
			this.name = name;
		}
		public IDatablock[] this[string s] {
			get {return this.Contents [s];}
			set {this.Contents [s] = value;}
		}
		public void Write (string dir, bool fromCurrent = true) {
			try {
				string path = fromCurrent?Path.Combine(Environment.CurrentDirectory, dir):dir;
				BinaryWriter writeStream = new BinaryWriter(File.Create(path));
				writeStream.Write (SDBHelper.Precode); //SSDB (Type header)
				writeStream.Write (BitConverter.IsLittleEndian); //Is this file written little endian? (BOOL)
				byte[] subtypeBytes = Encoding.UTF8.GetBytes (Type);
				writeStream.Write ((byte)subtypeBytes.Length); //Number of bytes to read for SDB Subtype. (BYTE)
				writeStream.Write (subtypeBytes); //Subtype name bytes.
				byte[] nameBytes = Encoding.UTF8.GetBytes (this.name);
				writeStream.Write ((byte)nameBytes.Length); //Number of bytes to read for SDB Name. (BYTE)
				writeStream.Write (nameBytes); //Subtype name bytes.
				writeStream.Write ((ushort)this.Contents.Count); //Number of keys (USHORT)
				foreach (KeyValuePair<string, IDatablock[]> KVP in this.Contents) {
					byte[] keyBytes = Encoding.UTF8.GetBytes(KVP.Key);
					writeStream.Write((byte)keyBytes.Length); //Number of bytes to read for key name. (BYTE)
					writeStream.Write(keyBytes); //Key name bytes.
					writeStream.Write((ushort)KVP.Value.Length); //Number of entries for this key. (USHORT)
					foreach (IDatablock DB in KVP.Value) {
						writeStream.Write(DB.Callsign); //Type callsign (refer to Datablock.cs) (BYTE)
						byte[] rawBytes = DB.GetRaw();
						if (DB.Callsign == 0x00) {writeStream.Write(rawBytes.Length);} //If type is string, how long is string? (INT)
						writeStream.Write(rawBytes); //Value bytes.
					}
				}
				writeStream.Close();
			} catch (Exception e) {
				Console.WriteLine ("--- A FATAL WRITE ERROR HAS OCCURED ---");
				Console.WriteLine (e);
			}
		}
		public static SingleLayerSnsysDataBase Read(string dir, bool fromCurrent = true) {
			try {
				string path = fromCurrent?Path.Combine(Environment.CurrentDirectory, dir):dir;
				//bool reverseOrder = false;
				BinaryReader readStream = new BinaryReader(File.OpenRead(path));
				if (Encoding.UTF8.GetString(readStream.ReadBytes(4)) != "SSDB") {throw new FormatException("Not a SnsysDatabase.");}
				if (BitConverter.ToBoolean(readStream.ReadBytes(1), 0) != BitConverter.IsLittleEndian) {}//reverseOrder = true;}
				string DBType = Encoding.UTF8.GetString(readStream.ReadBytes((int)readStream.ReadByte()));
				if (DBType != SingleLayerSnsysDataBase.Type) {throw new FormatException("Not a BasicSDB Subtype SnsysDatabase");}
				string DBName = Encoding.UTF8.GetString(readStream.ReadBytes((int)readStream.ReadByte()));
				SingleLayerSnsysDataBase SLSDB = new SingleLayerSnsysDataBase(DBName);
				ushort keyIter = readStream.ReadUInt16();
				for (ushort k = 0; k < keyIter; ++k) {
					string keyName = Encoding.UTF8.GetString(readStream.ReadBytes((int)readStream.ReadByte()));
					ushort valueIter = readStream.ReadUInt16();
					IDatablock[] keyValues = new IDatablock[valueIter];
					for (ushort v = 0; v < valueIter; ++v) {
						switch(readStream.ReadByte()) {
						case 0x00:
							keyValues[v] = StringDatablock.FromRaw(readStream.ReadBytes(readStream.ReadInt32()));
							break;
						case 0x01:
							keyValues[v] = BoolDatablock.FromRaw(readStream.ReadBytes(1));
							break;
						case 0x02:
							keyValues[v] = SByteDatablock.FromRaw(readStream.ReadBytes(1));
							break;
						case 0x03:
							keyValues[v] = ByteDatablock.FromRaw(readStream.ReadBytes(1));
							break;
						case 0x04:
							keyValues[v] = ShortDatablock.FromRaw(readStream.ReadBytes(2));
							break;
						case 0x05:
							keyValues[v] = UShortDatablock.FromRaw(readStream.ReadBytes(2));
							break;
						case 0x06:
							keyValues[v] = IntDatablock.FromRaw(readStream.ReadBytes(4));
							break;
						case 0x07:
							keyValues[v] = UIntDatablock.FromRaw(readStream.ReadBytes(4));
							break;
						case 0x08:
							keyValues[v] = LongDatablock.FromRaw(readStream.ReadBytes(8));
							break;
						case 0x09:
							keyValues[v] = ULongDatablock.FromRaw(readStream.ReadBytes(8));
							break;
						case 0x0A:
							keyValues[v] = FloatDatablock.FromRaw(readStream.ReadBytes(4));
							break;
						case 0x0B:
							keyValues[v] = DoubleDatablock.FromRaw(readStream.ReadBytes(8));
							break;
						default:
							throw new FormatException("An invalid type byte was detected, valid type bytes are 0x00 to 0x0A.");
						}
					}
					SLSDB[keyName] = keyValues;
				}
				readStream.Close();
				return SLSDB;
			} catch (Exception e) {
				Console.WriteLine ("--- A FATAL READ ERROR HAS OCCURED ---");
				Console.WriteLine (e);
				return SingleLayerSnsysDataBase.NULL;
			}
		}
		public void Debug () {
			SingleLayerSnsysDataBase DB = SingleLayerSnsysDataBase.Read ("test.sdb");
			Console.WriteLine (DB.name);
			foreach (KeyValuePair<string, IDatablock[]> KVP in DB.Database) {
				Console.WriteLine (KVP.Key);
				foreach (IDatablock ID in KVP.Value) {
					Console.WriteLine (ID.ToString());
				}
			}
		}
	}
}

