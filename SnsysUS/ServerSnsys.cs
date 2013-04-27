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
	public class SnsysUSServer : GenericServer, IServer {
		public static Dictionary <string, string> LevelKeys = new Dictionary<string, string>() {
			{"L1","DF43D66DE1590F605FCACE63862B73030149931731E914D0A6D1C03571AE3CA83010DDE35865D63EF14B83504DEE4D58846A78F5DC71E1A69C9C68A4CC899C38"}
		};
		private static RestrictionInfo L1 = new RestrictionInfo(true, "L1");
		public SnsysUSServer(IPAddress addr, ushort port) : base(new SnsysUSWeb(), addr, port) {}
		public override bool HandleFiles (HTTPProcessor sp) {
			FileProcessor FP = new FileProcessor(sp, Path.Combine(Environment.CurrentDirectory, "Assets"));
			FP.processMethodStrings = new List<string>(){"5"};
			return FP.Process(FileProcessor.METHOD.BLACKLIST_CONTAINS);
		}
		public override RestrictionInfo IsURLRestricted (string url) {
			if (url.Contains(".png")) {
				return L1;
			} else {
				return new RestrictionInfo(false, null);
			}
		}
		
		public override bool IsLevelKeyValid (string level, string key) {
			if (LevelKeys.ContainsKey(level) && LevelKeys[level] == KeyHelper.HexFromSHA512Hash(key)) {
				return true;
			}
			return false;
		}
	}
}

