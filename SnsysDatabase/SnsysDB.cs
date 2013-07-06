using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SnsysDatabase
{
	public static class SDBHelper {
		public static byte[] Precode = Encoding.UTF8.GetBytes("SSDB");
		public static DatabaseInfo ReadDatabaseHeader (string dir, bool fromCurrent = true) {
			string path = fromCurrent?Path.Combine(Environment.CurrentDirectory, dir):dir;
			BinaryReader readStream = new BinaryReader(File.OpenRead(path));
			if (Encoding.UTF8.GetString(readStream.ReadBytes(4)) != "SSDB") {throw new FormatException("Not a SnsysDatabase.");}
			string DBType = Encoding.UTF8.GetString(readStream.ReadBytes((int)readStream.ReadByte()));
			string DBName = Encoding.UTF8.GetString(readStream.ReadBytes((int)readStream.ReadByte()));
			readStream.Close ();
			return new DatabaseInfo (DBName, DBType);
		}
	}
	public struct DatabaseInfo {
		public string name;
		public string subtype;
		public DatabaseInfo (string name, string subtype) {
			this.name = name;
			this.subtype = subtype;
		}
	}
	public class SingleLayerSnsysDatabase {
		public static SingleLayerSnsysDatabase NULL = new SingleLayerSnsysDatabase();
		public static string Subtype = "BasicSDB";
		public Dictionary<string, IDatablock[]> Database;
		public string name = "NULL";
		public SingleLayerSnsysDatabase () {
			this.Database = new Dictionary<string, IDatablock[]> ();
		}
		public SingleLayerSnsysDatabase (string name) {
			this.Database = new Dictionary<string, IDatablock[]> ();
			this.name = name;
		}
		public IDatablock[] this[string s] {
			get {return this.Database [s];}
		}
		public void Write (string dir, bool fromCurrent = true) {
			try {
				string path = fromCurrent?Path.Combine(Environment.CurrentDirectory, dir):dir;
				BinaryWriter writeStream = new BinaryWriter(File.Create(path));
				writeStream.Write (SDBHelper.Precode); //SSDB (Type header)
				byte[] subtypeBytes = Encoding.UTF8.GetBytes (Subtype);
				writeStream.Write ((byte)subtypeBytes.Length); //Number of bytes to read for SDB Subtype. (BYTE)
				writeStream.Write (subtypeBytes); //Subtype name bytes.
				byte[] nameBytes = Encoding.UTF8.GetBytes (this.name);
				writeStream.Write ((byte)nameBytes.Length); //Number of bytes to read for SDB Name. (BYTE)
				writeStream.Write (nameBytes); //Subtype name bytes.
				writeStream.Write ((ushort)this.Database.Count); //Number of keys (USHORT)
				foreach (KeyValuePair<string, IDatablock[]> KVP in this.Database) {
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
		public static SingleLayerSnsysDatabase Read(string dir, bool fromCurrent = true) {
			try {
				string path = fromCurrent?Path.Combine(Environment.CurrentDirectory, dir):dir;
				BinaryReader readStream = new BinaryReader(File.OpenRead(path));
				if (Encoding.UTF8.GetString(readStream.ReadBytes(4)) != "SSDB") {throw new FormatException("Not a SnsysDatabase.");}
				string DBType = Encoding.UTF8.GetString(readStream.ReadBytes((int)readStream.ReadByte()));
				if (DBType != SingleLayerSnsysDatabase.Subtype) {throw new FormatException("Not a BasicSDB Subtype SnsysDatabase");}
				string DBName = Encoding.UTF8.GetString(readStream.ReadBytes((int)readStream.ReadByte()));
				SingleLayerSnsysDatabase SLSDB = new SingleLayerSnsysDatabase(DBName);
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
					SLSDB.Database[keyName] = keyValues;
				}
				readStream.Close();
				return SLSDB;
			} catch (Exception e) {
				Console.WriteLine ("--- A FATAL READ ERROR HAS OCCURED ---");
				Console.WriteLine (e);
				return SingleLayerSnsysDatabase.NULL;
			}
		}
		public void Debug () {
			SingleLayerSnsysDatabase DB = SingleLayerSnsysDatabase.Read ("test.sdb");
			Console.WriteLine (DB.name);
			foreach (KeyValuePair<string, IDatablock[]> KVP in DB.Database) {
				Console.WriteLine (String.Format(">{0}", KVP.Key));
				foreach (IDatablock ID in KVP.Value) {
					Console.WriteLine (String.Format("=>{0}", ID.ToString()));
				}
			}
		}
	}

	public class DoubleLayerSnsysDatabase {
		public static DoubleLayerSnsysDatabase NULL = new DoubleLayerSnsysDatabase();
		public static string Subtype = "DoubleSDB";
		public Dictionary<string, Dictionary<string, IDatablock[]>> Database;
		public string name = "NULL";
		public DoubleLayerSnsysDatabase () {
			this.Database = new Dictionary<string, Dictionary<string, IDatablock[]>> ();
		}
		public DoubleLayerSnsysDatabase (string name) {
			this.Database = new Dictionary<string, Dictionary<string, IDatablock[]>> ();
			this.name = name;
		}
		public Dictionary<string, IDatablock[]> this[string s] {
			get {
				if (!this.Database.ContainsKey (s)) {this.Add (s);}
				return this.Database [s];
			}
		}
		public void Add (string keyname) {
			this.Database.Add (keyname, new Dictionary<string, IDatablock[]>());
		}
		public void Write (string dir, bool fromCurrent = true) {
			try {
				string path = fromCurrent?Path.Combine(Environment.CurrentDirectory, dir):dir;
				BinaryWriter writeStream = new BinaryWriter(File.Create(path));
				writeStream.Write (SDBHelper.Precode); //SSDB (Type header)
				byte[] subtypeBytes = Encoding.UTF8.GetBytes (Subtype);
				writeStream.Write ((byte)subtypeBytes.Length); //Number of bytes to read for SDB Subtype. (BYTE)
				writeStream.Write (subtypeBytes); //Subtype name bytes.
				byte[] nameBytes = Encoding.UTF8.GetBytes (this.name);
				writeStream.Write ((byte)nameBytes.Length); //Number of bytes to read for SDB Name. (BYTE)
				writeStream.Write (nameBytes); //Subtype name bytes.
				writeStream.Write ((ushort)this.Database.Count); //Number of keys (USHORT)
				foreach (KeyValuePair<string, Dictionary<string, IDatablock[]>> KVP in this.Database) {
					byte[] keyKeyBytes = Encoding.UTF8.GetBytes(KVP.Key);
					writeStream.Write((byte)keyKeyBytes.Length); //Number of bytes to read for key name. (BYTE)
					writeStream.Write(keyKeyBytes); //Key name bytes.
					writeStream.Write((ushort)KVP.Value.Count); //Number of entries for this key. (USHORT)
					foreach (KeyValuePair <string, IDatablock[]> KVP2 in KVP.Value) {
						byte[] keyValueBytes = Encoding.UTF8.GetBytes(KVP2.Key);
						writeStream.Write((byte)keyValueBytes.Length); //Number of bytes to read for key name. (BYTE)
						writeStream.Write(keyValueBytes); //Key name bytes.
						writeStream.Write((ushort)KVP2.Value.Length); //Number of entries for this key. (USHORT)
						foreach (IDatablock DB in KVP2.Value) {
							writeStream.Write(DB.Callsign); //Type callsign (refer to Datablock.cs) (BYTE)
							byte[] rawBytes = DB.GetRaw();
							if (DB.Callsign == 0x00) {writeStream.Write(rawBytes.Length);} //If type is string, how long is string? (INT)
							writeStream.Write(rawBytes); //Value bytes.
						}
					}
				}
				writeStream.Close();
			} catch (Exception e) {
				Console.WriteLine ("--- A FATAL WRITE ERROR HAS OCCURED ---");
				Console.WriteLine (e);
			}
		}
		public static DoubleLayerSnsysDatabase Read(string dir, bool fromCurrent = true) {
			try {
				string path = fromCurrent?Path.Combine(Environment.CurrentDirectory, dir):dir;
				BinaryReader readStream = new BinaryReader(File.OpenRead(path));
				if (Encoding.UTF8.GetString(readStream.ReadBytes(4)) != "SSDB") {throw new FormatException("Not a SnsysDatabase.");}
				string DBType = Encoding.UTF8.GetString(readStream.ReadBytes((int)readStream.ReadByte()));
				if (DBType != DoubleLayerSnsysDatabase.Subtype) {throw new FormatException("Not a BasicSDB Subtype SnsysDatabase");}
				string DBName = Encoding.UTF8.GetString(readStream.ReadBytes((int)readStream.ReadByte()));
				DoubleLayerSnsysDatabase DLSDB = new DoubleLayerSnsysDatabase(DBName);
				ushort keyIter = readStream.ReadUInt16();
				for (ushort k = 0; k < keyIter; ++k) {
					string keyKeyName = Encoding.UTF8.GetString(readStream.ReadBytes((int)readStream.ReadByte()));
					ushort keyValueIter = readStream.ReadUInt16();
					for (ushort kv = 0; kv < keyValueIter; ++kv){
						string keyValueName = Encoding.UTF8.GetString(readStream.ReadBytes((int)readStream.ReadByte()));
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
						DLSDB[keyKeyName][keyValueName] = keyValues;
					}
				}
				readStream.Close();
				return DLSDB;
			} catch (Exception e) {
				Console.WriteLine ("--- A FATAL READ ERROR HAS OCCURED ---");
				Console.WriteLine (e);
				return DoubleLayerSnsysDatabase.NULL;
			}
		}
		public void Debug () {
			DoubleLayerSnsysDatabase DB = DoubleLayerSnsysDatabase.Read ("test2.sdb");
			Console.WriteLine (DB.name);
			foreach (KeyValuePair<string, Dictionary<string, IDatablock[]>> KVP in DB.Database) {
				Console.WriteLine (String.Format(">{0}", KVP.Key));
				foreach (KeyValuePair<string, IDatablock[]> KVP2 in KVP.Value) {
					Console.WriteLine (String.Format("=>{0}", KVP2.Key));
					foreach (IDatablock ID in KVP2.Value) {
						Console.WriteLine (String.Format("==>{0}", ID.ToString()));
					}
				}
			}
		}
	}
}

