using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SnsysDatabase
{
	//0x00 String (String)           2 Byte (UShort) Length Precursor, N Byte Length
	//0x01 Bool   (Boolean)          1 Byte
	//0x02 SByte  (Signed Byte)      1 Byte
	//0x03 Byte   (Unsigned Byte)    1 Byte
	//0x04 Short  (Signed Short)     2 Bytes
	//0x05 UShort (Unsigned Short)   2 Bytes
	//0x06 Int    (Signed Integer)   4 Bytes
	//0x07 UInt   (Unsigned Integer) 4 Bytes
	//0x08 Long   (Signed Long)      8 Bytes
	//0x09 ULong  (Unsigned Long)    8 Bytes
	//0x0A Float  (Float)            4 Bytes
	//0x0B Double (Double)           8 Bytes

	public interface IDatablock {byte[] GetRaw(); byte Callsign{ get;}};

	public struct DBString : IDatablock {
		public byte Callsign {get{return 0x00;}}
		public string value;
		public DBString(string value) {
			this.value = value;
		}
		public static implicit operator DBString(string S) {
			return new DBString (S);
		}
		public static implicit operator string(DBString S) {
			return S.value;
		}
		public byte[] GetRaw() {
			return Encoding.UTF8.GetBytes (this.value);
		}
		public static DBString FromRaw (byte[] raw) {
			return new DBString(Encoding.UTF8.GetString (raw));
		}
		public override string ToString () {
			return this.value;
		}
	}

	public struct DBBool : IDatablock {
		public byte Callsign {get{return 0x01;}}
		public bool value;
		public DBBool(bool value) {
			this.value = value;
		}
		public static implicit operator DBBool(bool B) {
			return new DBBool (B);
		}
		public static implicit operator bool(DBBool B) {
			return B.value;
		}
		public byte[] GetRaw() {
			return BitConverter.GetBytes (this.value);
		}
		public static DBBool FromRaw (byte[] raw) {
			return new DBBool (BitConverter.ToBoolean (raw, 0));
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}

	public struct DBInt : IDatablock {
		public byte Callsign {get{return 0x06;}}
		public int value;
		public DBInt(int value) {
			this.value = value;
		}
		public static implicit operator DBInt(int I) {
			return new DBInt (I);
		}
		public static implicit operator int(DBInt I) {
			return I.value;
		}
		public byte[] GetRaw() {
			return BitConverter.GetBytes (this.value);
		}
		public static DBInt FromRaw (byte[] raw) {
			return new DBInt(BitConverter.ToInt32 (raw, 0));
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}
}

