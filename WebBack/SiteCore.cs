using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using WebFront;

namespace WebBack
{
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
        public override string ToString() {
            return String.Format("{0}={1};Path=/;", this.name, this.key);
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

    public static class ArgumentHelper {
        public static Dictionary<string,string> Organize(string BaseURL, out string NewURL) {
            if (!BaseURL.Contains("?")) { NewURL = BaseURL; return new Dictionary<string, string>(); }
            string[] ArgSep = BaseURL.Split('?');
            NewURL = ArgSep[0];
            Dictionary<string, string> AP = new Dictionary<string, string>();
            if (!ArgSep[1].Contains("&")) {
                try {
                    string[] S = ArgSep[1].Split('=');
                    AP[S[0]] = S[1];
                } catch { }
            } else {
                string[] Args = ArgSep[1].Split('&');
                foreach (string s in Args) {
                    try {
                        string[] S = s.Split('=');
                        AP[S[0]] = S[1];
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
        public SitePass(string Host, string Path) {
            this.Host = Host;
            this.Path = Path;
        }
    }
}