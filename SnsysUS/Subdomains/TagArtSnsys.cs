using System;
using System.Collections;
using System.Collections.Generic;
using Gdk;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using WebBack;
using WebFront;
using SnsysDatabase;

namespace SnsysUS {
	public partial class SnsysUSWeb {
		private Dictionary<string, List<string>> processedImages;
		private Dictionary<string, List<string>> tagReference;
		private void InitializeTagArt () {
			if (File.Exists (Path.Combine (Environment.CurrentDirectory, "tagart.sdb"))) {
				SingleLayerSnsysDatabase SLSD = SingleLayerSnsysDatabase.Read ("tagart.sdb");
				this.processedImages = TagArtHelper.ImageDictionaryFromSnsysDatabase (SLSD);
				this.tagReference = TagArtHelper.TagReferenceFromImageDictionary (this.processedImages);
			} else {
				this.processedImages = new Dictionary<string, List<string>> ();
				this.tagReference = new Dictionary<string, List<string>> ();
			}
		}
		public HTML.Webpage SNSYS_TagArt(SnsysUSServer parent, SitePass SP, Dictionary<string, string> args) {
			return new HTML.Webpage ("Tag Art Test");
		}
	}

	internal static class TagArtHelper {
		public static Dictionary<string, List<string>> ImageDictionaryFromSnsysDatabase (SingleLayerSnsysDatabase SLSD) {
			Dictionary<string, List<string>> rDic = new Dictionary<string, List<string>> ();
			foreach (KeyValuePair<string, IDatablock[]> KVP in SLSD.Database) {
				rDic [KVP.Key] = new List<string> ();
				foreach (IDatablock SD in KVP.Value) {
					if (SD.Callsign == 0x00) {
						rDic [KVP.Key].Add (((StringDatablock)SD).value);
					}
				}
			}
			return rDic;
		}
		public static Dictionary<string, List<string>> TagReferenceFromImageDictionary (Dictionary<string, List<string>> ID) {
			Dictionary<string, List<string>> rDic = new Dictionary<string, List<string>> ();
			foreach (KeyValuePair<string, List<string>> KVP in ID) {
				foreach (string s in KVP.Value) {
					if (!rDic.ContainsKey (s)) {
						rDic [s] = new List<string> ();
						rDic [s].Add (KVP.Key);
					} else {
						if (!rDic [s].Contains (KVP.Key)) {
							rDic [s].Add (KVP.Key);
						}
					}
				}
			}
			return rDic;
		}
	}
}

