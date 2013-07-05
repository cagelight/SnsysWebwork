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

	public struct StringDatablock : IDatablock {
		public byte Callsign {get{return 0x00;}}
		public int Bytespace {get{return Encoding.UTF8.GetByteCount (this.value);}}
		public string value;
		public StringDatablock(string value) {
			this.value = value;
		}
		public static implicit operator StringDatablock(string S) {
			return new StringDatablock (S);
		}
		public static implicit operator string(StringDatablock S) {
			return S.value;
		}
		public byte[] GetRaw() {
			return Encoding.UTF8.GetBytes (this.value);
		}
		public static StringDatablock FromRaw (byte[] raw) {
			return new StringDatablock(Encoding.UTF8.GetString (raw));
		}
		public override string ToString () {
			return this.value;
		}
	}

	public struct BoolDatablock : IDatablock {
		public byte Callsign {get{return 0x01;}}
		public bool value;
		public BoolDatablock(bool value) {
			this.value = value;
		}
		public static implicit operator BoolDatablock(bool B) {
			return new BoolDatablock (B);
		}
		public static implicit operator bool(BoolDatablock B) {
			return B.value;
		}
		public byte[] GetRaw() {
			return BitConverter.GetBytes (this.value);
		}
		public static BoolDatablock FromRaw (byte[] raw) {
			return new BoolDatablock (BitConverter.ToBoolean (raw, 0));
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}

	public struct SByteDatablock : IDatablock {
		public byte Callsign {get{return 0x02;}}
		public sbyte value;
		public SByteDatablock(sbyte value) {
			this.value = value;
		}
		public static implicit operator SByteDatablock(sbyte B) {
			return new SByteDatablock (B);
		}
		public static implicit operator sbyte(SByteDatablock B) {
			return B.value;
		}
		public byte[] GetRaw() {
			unchecked{
				return new byte[] {(byte)this.value};
			}
		}
		public static SByteDatablock FromRaw (byte[] raw) {
			unchecked{
				return new SByteDatablock ((sbyte)raw [0]);
			}
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}

	public struct ByteDatablock : IDatablock {
		public byte Callsign {get{return 0x03;}}
		public byte value;
		public ByteDatablock(byte value) {
			this.value = value;
		}
		public static implicit operator ByteDatablock(byte B) {
			return new ByteDatablock (B);
		}
		public static implicit operator byte(ByteDatablock B) {
			return B.value;
		}
		public byte[] GetRaw() {
			return new byte[] { this.value };
		}
		public static ByteDatablock FromRaw (byte[] raw) {
			return new ByteDatablock (raw [0]); 
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}

	public struct ShortDatablock : IDatablock {
		public byte Callsign {get{return 0x04;}}
		public short value;
		public ShortDatablock(short value) {
			this.value = value;
		}
		public static implicit operator ShortDatablock(short I) {
			return new ShortDatablock (I);
		}
		public static implicit operator short(ShortDatablock I) {
			return I.value;
		}
		public byte[] GetRaw() {
			return BitConverter.GetBytes (this.value);
		}
		public static ShortDatablock FromRaw (byte[] raw) {
			return new ShortDatablock(BitConverter.ToInt16 (raw, 0));
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}

	public struct UShortDatablock : IDatablock {
		public byte Callsign {get{return 0x05;}}
		public ushort value;
		public UShortDatablock(ushort value) {
			this.value = value;
		}
		public static implicit operator UShortDatablock(ushort I) {
			return new UShortDatablock (I);
		}
		public static implicit operator ushort(UShortDatablock I) {
			return I.value;
		}
		public byte[] GetRaw() {
			return BitConverter.GetBytes (this.value);
		}
		public static UShortDatablock FromRaw (byte[] raw) {
			return new UShortDatablock(BitConverter.ToUInt16 (raw, 0));
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}

	public struct IntDatablock : IDatablock {
		public byte Callsign {get{return 0x06;}}
		public int value;
		public IntDatablock(int value) {
			this.value = value;
		}
		public static implicit operator IntDatablock(int I) {
			return new IntDatablock (I);
		}
		public static implicit operator int(IntDatablock I) {
			return I.value;
		}
		public byte[] GetRaw() {
			return BitConverter.GetBytes (this.value);
		}
		public static IntDatablock FromRaw (byte[] raw) {
			return new IntDatablock(BitConverter.ToInt32 (raw, 0));
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}

	public struct UIntDatablock : IDatablock {
		public byte Callsign {get{return 0x07;}}
		public uint value;
		public UIntDatablock(uint value) {
			this.value = value;
		}
		public static implicit operator UIntDatablock(uint I) {
			return new UIntDatablock (I);
		}
		public static implicit operator uint(UIntDatablock I) {
			return I.value;
		}
		public byte[] GetRaw() {
			return BitConverter.GetBytes (this.value);
		}
		public static UIntDatablock FromRaw (byte[] raw) {
			return new UIntDatablock(BitConverter.ToUInt32 (raw, 0));
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}

	public struct LongDatablock : IDatablock {
		public byte Callsign {get{return 0x08;}}
		public long value;
		public LongDatablock(long value) {
			this.value = value;
		}
		public static implicit operator LongDatablock(long I) {
			return new LongDatablock (I);
		}
		public static implicit operator long(LongDatablock I) {
			return I.value;
		}
		public byte[] GetRaw() {
			return BitConverter.GetBytes (this.value);
		}
		public static LongDatablock FromRaw (byte[] raw) {
			return new LongDatablock(BitConverter.ToInt64 (raw, 0));
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}

	public struct ULongDatablock : IDatablock {
		public byte Callsign {get{return 0x09;}}
		public ulong value;
		public ULongDatablock(ulong value) {
			this.value = value;
		}
		public static implicit operator ULongDatablock(ulong I) {
			return new ULongDatablock (I);
		}
		public static implicit operator ulong(ULongDatablock I) {
			return I.value;
		}
		public byte[] GetRaw() {
			return BitConverter.GetBytes (this.value);
		}
		public static ULongDatablock FromRaw (byte[] raw) {
			return new ULongDatablock(BitConverter.ToUInt64 (raw, 0));
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}

	public struct FloatDatablock : IDatablock {
		public byte Callsign {get{return 0x0A;}}
		public float value;
		public FloatDatablock(float value) {
			this.value = value;
		}
		public static implicit operator FloatDatablock(float I) {
			return new FloatDatablock (I);
		}
		public static implicit operator float(FloatDatablock I) {
			return I.value;
		}
		public byte[] GetRaw() {
			return BitConverter.GetBytes (this.value);
		}
		public static FloatDatablock FromRaw (byte[] raw) {
			return new FloatDatablock(BitConverter.ToSingle (raw, 0));
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}

	public struct DoubleDatablock : IDatablock {
		public byte Callsign {get{return 0x0B;}}
		public double value;
		public DoubleDatablock(double value) {
			this.value = value;
		}
		public static implicit operator DoubleDatablock(double I) {
			return new DoubleDatablock (I);
		}
		public static implicit operator double(DoubleDatablock I) {
			return I.value;
		}
		public byte[] GetRaw() {
			return BitConverter.GetBytes (this.value);
		}
		public static DoubleDatablock FromRaw (byte[] raw) {
			return new DoubleDatablock(BitConverter.ToDouble (raw, 0));
		}
		public override string ToString (){
			return this.value.ToString ();
		}
	}
}

