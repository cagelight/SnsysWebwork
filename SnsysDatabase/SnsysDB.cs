using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SnsysDatabase
{
	public static class SDBHelper {
		public static byte[] Precode = Encoding.UTF8.GetBytes("SSDB");
		public static void WriteForType (FileStream FS, Types t, int num) {
			switch (t) {
			case (Types.BYTE):
				FS.Write (BitConverter.GetBytes((byte)num), 0, 1);
				break;
			case (Types.USHORT):
				FS.Write (BitConverter.GetBytes((ushort)num), 0, 2);
				break;
			case (Types.UINT):
				FS.Write (BitConverter.GetBytes((uint)num), 0, 4);
				break;
			case (Types.ULONG):
				FS.Write (BitConverter.GetBytes((ulong)num), 0, 8);
				break;
			}
		}
		public static byte[] ReadForType (ref BinaryReader BR, Types t) {
			switch (t) {
			case (Types.BYTE):
				return BR.ReadBytes ((int)BR.ReadByte());
			case (Types.USHORT):
				return BR.ReadBytes ((int)BitConverter.ToInt16(BR.ReadBytes(2), 0));
			case (Types.UINT):
				return BR.ReadBytes ((int)BitConverter.ToInt32(BR.ReadBytes(4), 0));
			case (Types.ULONG):
				return BR.ReadBytes ((int)BitConverter.ToInt64(BR.ReadBytes(8), 0));
			default:
				return new byte[] { };
			}
		}
	}
	public enum Types {BYTE, USHORT, UINT, ULONG}
	internal struct BoolPair {
		public bool first;
		public bool second;
		public BoolPair(bool first, bool second) {
			this.first = first; this.second = second;
		}
	}
	internal struct TypeInfo {
		public static TypeInfo BYTE = new TypeInfo (1, false, false);
		public static TypeInfo USHORT = new TypeInfo (2, false, true);
		public static TypeInfo UINT = new TypeInfo (4, true, false);
		public static TypeInfo ULONG = new TypeInfo (8, true, true);
		public readonly bool[] bits;
		public readonly int bytesCount;
		public TypeInfo (int bytesCount, bool firstBit, bool secondBit) {
			this.bytesCount = bytesCount;
			this.bits = new bool[] {firstBit, secondBit};
		}
	}
	public struct SDBSettings {
		// SETTINGS FORMAT (four 2-bit values, stored as a byte)
		// 0-1 : Key name lengths type
		// 2-3 : Number of lines type
		// 4-5 : Number of key entries type
		// 6-7 : Value length type
		//
		// 00 = byte
		// 01 = ushort
		// 10 = uint
		// 11 = ulong
		private static Dictionary<Types, TypeInfo> TwoBitInfoTable = new Dictionary<Types, TypeInfo> () {
			{Types.BYTE, TypeInfo.BYTE},
			{Types.USHORT, TypeInfo.USHORT},
			{Types.UINT, TypeInfo.UINT},
			{Types.ULONG, TypeInfo.ULONG},
		};
		private static Dictionary<BoolPair, Types> TwoBitTypeTable = new Dictionary<BoolPair, Types> () {
			{new BoolPair(false, false), Types.BYTE},
			{new BoolPair(false, true), Types.USHORT},
			{new BoolPair(true, false), Types.UINT},
			{new BoolPair(true, true), Types.ULONG},
		};
		internal Types nameLengthType;
		internal Types keyCountType;
		internal Types valueCountType;
		internal Types valueLengthType;
		public SDBSettings (Types nameLength, Types keyCount, Types valueCount, Types valueLength) {
			nameLengthType = nameLength;
			keyCountType = keyCount;
			valueCountType = valueCount;
			valueLengthType = valueLength;
		}
		public byte GetByte () {
			bool[] L1 = TwoBitInfoTable[nameLengthType].bits;
			bool[] L2 = TwoBitInfoTable[keyCountType].bits;
			bool[] L3 = TwoBitInfoTable[valueCountType].bits;
			bool[] L4 = TwoBitInfoTable[valueLengthType].bits;
			bool[] bits = new bool[] { L1[0], L1[1], L2[0], L2[1], L3[0], L3[1], L4[0], L4[1] };
			byte[] b = new byte[1];
			new BitArray(bits).CopyTo (b, 0);
			return b [0];
		}
		public static SDBSettings FromByte(byte b) {
			BitArray B = new BitArray (new byte[] {b});
			return new SDBSettings (TwoBitTypeTable[new BoolPair(B[0], B[1])], TwoBitTypeTable[new BoolPair(B[2], B[3])], TwoBitTypeTable[new BoolPair(B[4], B[5])], TwoBitTypeTable[new BoolPair(B[6], B[7])]);
		}
		public static int GetByteNumForType (Types T) {
			return TwoBitInfoTable [T].bytesCount;
		}
	}
	public class SingleLayerSnsysDataBase {
		public static SingleLayerSnsysDataBase NULL = new SingleLayerSnsysDataBase();
		public static string Type = "BasicSDB";
		public SDBSettings Settings = new SDBSettings(Types.BYTE, Types.USHORT, Types.USHORT, Types.USHORT);
		protected Dictionary<string, IDatablock[]> Contents;
		public string name = "NULL";
		public SingleLayerSnsysDataBase () {
			this.Contents = new Dictionary<string, IDatablock[]> ();
		}
		public SingleLayerSnsysDataBase (string name) {
			this.Contents = new Dictionary<string, IDatablock[]> ();
			this.name = name;
		}
		public SingleLayerSnsysDataBase (SDBSettings SDBS, string name) {
			this.Contents = new Dictionary<string, IDatablock[]> ();
			this.Settings = SDBS;
			this.name = name;
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
				writeStream.Write (BitConverter.GetBytes(BitConverter.IsLittleEndian), 0, 1); //Is this file written little endian? (BOOL)
				writeStream.WriteByte(this.Settings.GetByte()); //Write the settings byte.
				byte[] subtypeBytes = Encoding.UTF8.GetBytes (Type);
				writeStream.WriteByte((byte)subtypeBytes.Length); //Number of bytes to read for SDB Subtype. (BYTE)
				writeStream.Write (subtypeBytes, 0, subtypeBytes.Length); //Subtype name bytes.
				byte[] nameBytes = Encoding.UTF8.GetBytes (this.name);
				SDBHelper.WriteForType(writeStream, Settings.nameLengthType, nameBytes.Length); //Number of bytes to read for SDB Name. (SETTINGS CONTROLLED TYPE)
				writeStream.Write (nameBytes, 0, nameBytes.Length); //Subtype name bytes.
				SDBHelper.WriteForType (writeStream, Settings.keyCountType, this.Contents.Count); //Number of keys (SETTINGS CONTROLLED TYPE)
				foreach (KeyValuePair<string, IDatablock[]> KVP in this.Contents) {
					byte[] keyBytes = Encoding.UTF8.GetBytes(KVP.Key);
					SDBHelper.WriteForType (writeStream, Settings.nameLengthType, keyBytes.Length); //Number of bytes to read for key name. (SETTINGS CONTROLLED TYPE)
					writeStream.Write(keyBytes, 0, keyBytes.Length); //Key name bytes.
					SDBHelper.WriteForType(writeStream, Settings.valueCountType, KVP.Value.Length); //Number of entries for this key. (SETTINGS CONTROLLED TYPE)
					foreach (IDatablock DB in KVP.Value) {
						writeStream.WriteByte(DB.Callsign); //Type callsign (refer to Datablock.cs) (BYTE)
						byte[] rawBytes = DB.GetRaw();
						if (DB.Callsign == 0x00) {SDBHelper.WriteForType(writeStream, Settings.valueLengthType, rawBytes.Length);} //If type is string, how long is string? (SETTINGS CONTROLLED TYPE)
						writeStream.Write(rawBytes, 0, rawBytes.Length); //Value bytes.
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
				bool reverseOrder = false;
				BinaryReader readStream = new BinaryReader(File.OpenRead(path));
				if (Encoding.UTF8.GetString(readStream.ReadBytes(4)) != "SSDB") {throw new FormatException("Not a SnsysDatabase.");}
				if (BitConverter.ToBoolean(readStream.ReadBytes(1), 0) != BitConverter.IsLittleEndian) {reverseOrder = true;}
				SDBSettings SDBS = SDBSettings.FromByte(readStream.ReadByte());
				string DBType = Encoding.UTF8.GetString(readStream.ReadBytes((int)readStream.ReadByte()));
				if (DBType != SingleLayerSnsysDataBase.Type) {throw new FormatException("Not a BasicSDB Subtype SnsysDatabase");}
				string DBName = Encoding.UTF8.GetString(SDBHelper.ReadForType(ref readStream, SDBS.nameLengthType));
				SingleLayerSnsysDataBase SLSDB = new SingleLayerSnsysDataBase(SDBS, DBName);

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
			Console.WriteLine (DB.Settings.nameLengthType.ToString());
			Console.WriteLine (DB.name);
		}
	}
}

