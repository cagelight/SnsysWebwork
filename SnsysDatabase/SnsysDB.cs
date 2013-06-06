using System;
using System.Collections.Generic;

namespace SnsysDatabase
{
	internal struct SnsysString {
		public enum Type {NULL, HEADER, BODY};
		public SnsysString.Type type;
		public string val;
		public SnsysString(string s, Type t = Type.NULL) {
			this.val = s;
			this.type = t;
		}
		public static explicit operator string(SnsysString s) {
			return s.val;
		}
		public static implicit operator SnsysString(string s) {
			return new SnsysString (s);
		}
		public static byte[] GetRaw (SnsysString[] SS) {
			int l = 0;
			foreach (SnsysString SSi in SS) {
				l += SSi.val.Length;
				switch (SSi.type) {
				case SnsysString.Type.NULL:
					break;
				case SnsysString.Type.HEADER:
					++l;
					++l;
					break;
				case SnsysString.Type.BODY:
					++l;
					break;
				}
			}
			byte[] r = new byte[l];
			int li = 0;
			foreach (SnsysString SSi in SS) {
				foreach (byte c in System.Text.Encoding.UTF8.GetBytes(SSi.val)) {
					r [li] = c;
					++li;
				}
				switch (SSi.type) {
				case SnsysString.Type.NULL:
					break;
				case SnsysString.Type.HEADER:
					r [li] = 0x00;
					r [li+1] = 0x00;
					++li;++li;
					break;
				case SnsysString.Type.BODY:
					r [li] = 0x00;
					++li;
					break;
				}
			}
			if (li + 1 != l) {throw new Exception("A formatting anomaly has occured at SnsysString.GetRaw(), please investigate.");}
			return r;
		}
	}
	public class GenericSnsysDatabase {
		protected Dictionary<string, string[]> strings;
		public static void Write (GenericSnsysDatabase GSD) {

		}
	}
	public class SnsysForumDatabase : GenericSnsysDatabase {

	}
}

