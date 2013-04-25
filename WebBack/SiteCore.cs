using System;
using System.IO;

namespace WebBack
{
	public struct RestrictionInfo {
		bool isRestricted;
		public readonly string restrictionTitle;
		public RestrictionInfo(bool ir, string title = "") {
			this.isRestricted = ir;
			this.restrictionTitle = title;
		}
		public static implicit operator bool(RestrictionInfo I) {return I.isRestricted;}
		public static implicit operator RestrictionInfo(bool B) {return new RestrictionInfo(B);}
	}
	public interface ISite {string Generate(params string[] parms); RestrictionInfo IsURLRestricted(string url);}
}

