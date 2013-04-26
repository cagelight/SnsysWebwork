using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading ;

using WebBack;
using WebFront;

namespace SnsysUS
{
	public class SnsysUSWeb : ISite {
		public static Dictionary <string, string> LevelKeys = new Dictionary<string, string>() {
			{"L1","DF43D66DE1590F605FCACE63862B73030149931731E914D0A6D1C03571AE3CA83010DDE35865D63EF14B83504DEE4D58846A78F5DC71E1A69C9C68A4CC899C38"}
		};
		private static RestrictionInfo L1 = new RestrictionInfo(true, "L1");
		public string Generate(params string[] parms) {
			HTML.Webpage WP = new HTML.Webpage("Test Page");
			WP.Head += HTML.Link().Rel("stylesheet").Href("http://snsys.us/snsys.css");
			WP.Body += SnsysUSGeneric.TitleBar("Test Page");
			return WP.ToString();
		}

		public RestrictionInfo IsURLRestricted (string url) {
			if (url.Contains(".jpg")) {
				return L1;
			} else {
				return new RestrictionInfo(false, null);
			}
		}
		public bool IsLevelKeyValid (string level, string key) {
			if (LevelKeys.ContainsKey(level) && LevelKeys[level] == KeyHelper.HexFromSHA512Hash(key)) {
				return true;
			}
			return false;
		}
	}
}

