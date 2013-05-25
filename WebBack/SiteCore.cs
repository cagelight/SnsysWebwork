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
			return new SCookie(name, StringRandom.GenerateNumLet(32));
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
					else if ((DateTime.Now - TK.DT).Minutes > 30) {
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

    public struct ArgumentPair {
        public string Key { get { return this.KVP.Key; } }
        public string Value { get { return this.KVP.Value; } }
        private KeyValuePair<string, string> KVP;
        public ArgumentPair (string Key, string Value) {
            this.KVP = new KeyValuePair<string, string> (Key, Value);
        }
        public ArgumentPair(KeyValuePair<string,string> KVP) {
            this.KVP = KVP;
        }
        public static implicit operator ArgumentPair(KeyValuePair<string,string> KVP) {
            return new ArgumentPair(KVP);
        }
        public static implicit operator KeyValuePair<string, string>(ArgumentPair AP) {
            return new KeyValuePair<string, string>(AP.Key, AP.Value);
        }
    }

    public static class ArgumentHelper {
        public static List<ArgumentPair> Organize(string BaseURL, out string NewURL) {
            if (!BaseURL.Contains("?")) { NewURL = BaseURL; return new List<ArgumentPair>(); }
            string[] ArgSep = BaseURL.Split('?');
            NewURL = ArgSep[0];
            List<ArgumentPair> AP = new List<ArgumentPair>();
            if (!ArgSep[1].Contains("&")) {
                try {
                    string[] S = ArgSep[1].Split('=');
                    AP.Add(new ArgumentPair(S[0], S[1]));
                } catch { }
            } else {
                string[] Args = ArgSep[1].Split('&');
                foreach (string s in Args) {
                    try {
                        string[] S = s.Split('=');
                        AP.Add(new ArgumentPair(S[0], S[1]));
                    } catch { }
                }
            }
            return AP;
        }
    }

    public struct SitePass {
        public string Host;
        public string Path;
        public string URL {
            get {
                return Host + Path;
            }
        }
        public string TotalURL {
            get {
                return "http://" + Host + Path;
            }
        }
        public SitePass(string Website, string Path) {
            this.Host = Website;
            this.Path = Path;
        }
    }

    public interface ISite { string Generate(SitePass URLInfo, params ArgumentPair[] args);}
}