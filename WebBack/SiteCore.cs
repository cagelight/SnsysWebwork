using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebBack
{
	public struct RestrictionInfo {
		bool isRestricted;
		public readonly string restrictionTitle;
		public RestrictionInfo(bool ir, string level) {
			this.isRestricted = ir;
			this.restrictionTitle = level;
		}
		public static implicit operator bool(RestrictionInfo I) {return I.isRestricted;}
	}
	public struct SCookie {
		public string name;
		public string key;
		public SCookie (string name, string val) {
			this.name = name;
			this.key = val;
		}
		public static SCookie GenerateNew (string name) {
			return new SCookie(name, StringRandom.GenerateNumLet(16));
		}
	}
	public class ClientAuthorizations {
		public class TimeKey {
			public DateTime DT;
			public string Key;
			public TimeKey (string Key) {
				this.Key = Key;
				this.DT = DateTime.Now;
			}
		}
		public Dictionary<string, List<TimeKey>> AuthList;
		public ClientAuthorizations() {
			AuthList = new Dictionary<string, List<TimeKey>>();
		}
		public void Add (string authlevel, string key) {
			if (!AuthList.ContainsKey(authlevel)) {this.AuthList.Add(authlevel, new List<TimeKey>(){new TimeKey(key)});}
			else {
				AuthList[authlevel].Add(new TimeKey(key));
				if (AuthList[authlevel].Count > 10) {AuthList[authlevel].RemoveAt(0);}
			}
		}
		public bool CheckAuth (string authlevel, string key) {
			if (AuthList.ContainsKey(authlevel)) {
				foreach (TimeKey TK in AuthList[authlevel]) {
					if (TK.Key != key) {continue;} 
					else if ((DateTime.Now - TK.DT).Seconds > 15) {
						AuthList[authlevel].Remove(TK);
						return false;
					} else {
						return true;
					}
				}
			}
			return false;
		}
	}
	public static class KeyHelper {
		public static string HexFromSHA512Hash (string strin) {
			byte[] medium = System.Text.Encoding.UTF8.GetBytes(strin);
			byte[] emedium = new System.Security.Cryptography.SHA512Managed().ComputeHash(medium);
			StringBuilder hex = new StringBuilder(emedium.Length * 2);
			foreach (byte b in emedium)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString().ToUpper();
		}
	}

	public interface ISite {string Generate(params string[] parms); RestrictionInfo IsURLRestricted(string url); bool IsLevelKeyValid(string l, string k);}
}

