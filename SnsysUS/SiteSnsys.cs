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
        private readonly string rootPath;
        private readonly string thumbRootPath;
		public SnsysUSWeb() {
            rootPath = Path.Combine(Environment.CurrentDirectory, "Assets", "Art");
            thumbRootPath = Path.Combine(Environment.CurrentDirectory, "Assets", "Art", ".thumb");
        }
        public HTML.Webpage Generate(SnsysUSServer parent, SitePass SP, Dictionary<string, string> args) {
            string[] divURL = SP.Host.Split('.');
            switch (divURL[0]) {
			case "art":
				return this.SNSYS_Art(parent, SP, args);
            case "test":
                return this.SNSYS_Test(SP, args);
			case "tag":
				return this.SNSYS_TagArt (parent, SP, args);
            default:
                return this.SNSYS_Main(SP, args);
            }
        }

		public HTML.Webpage SNSYS_Main(SitePass SP, Dictionary<string, string> args) {
			HTML.Webpage WP = new HTML.Webpage("Sensory Systems Main Page");
			WP.Head += HTML.Link().Rel("stylesheet").Href("/snsys.css");
			WP.Body += SnsysUSGeneric.TitleBar("Main Page");
			WP.Body += SnsysUSGeneric.SnsysSub("Hello!", null, HTML.H1("Welcome to Sensory Systems!").Class("light"));
			WP.Body += SnsysUSGeneric.SnsysSub (null, null, HTML.H3("This site is running on an HTTP Server made completely from scratch in C#, accompanied by a C# HTML and CSS interface for web development.").Class("light"), HTML.Attribute(HTML.Span("The project can be found on GitHub through this link.").Class("light")).Class("light").Href("https://github.com/Kallikrates/SnsysWebwork"));
			WP.Body += SnsysUSGeneric.SnsysSub ("Site Links", null, HTML.Attribute("Art Collections").Class("light").Href("http://art.snsys.us/"));
			return WP;
		}
		public HTML.Webpage SNSYS_Test(SitePass SP, Dictionary<string, string> args) {
			HTML.Webpage WP = new HTML.Webpage("Test Page");
			WP.Head += HTML.Link().Rel("stylesheet").Href("/snsys.css");
			WP.Body += SnsysUSGeneric.TitleBar(SP.Path, SP.TotalURL);
			foreach (KeyValuePair<string, string> AP in args) {
				WP.Body += SnsysUSGeneric.SnsysSub(AP.Key, null, HTML.Span(AP.Value));
			}
			return WP;
		}

		public bool IsHiddenGallery(string name, string[] restrictedList) {
			foreach (string s in forbiddenGalleries) {
				if (name == s) { return true; }
			}
			foreach (string s in restrictedList) {
				if (name.ToLower().Contains(s.ToLower())) { return true; }
			}
			return false;
		}
		private static string[] forbiddenGalleries = new string[] { ".thumb" };
		public static bool IsForbiddenGallery(string name) {
			if (name.Contains("..") || name.Contains("/") || name.Contains("\\")) { return true; }
			foreach (string s in forbiddenGalleries) {
				if (name == s) { return true; }
			}
			return false;
		}
	}
    public static class SnsysHelper {
        public static string Isolate(string directory) {
            string r = Path.GetFileName(directory);
            return r;
        }
        public static string[] Isolate(params string[] directoryArray) {
            string[] r = new string[directoryArray.Length];
            for (int i = 0; i < directoryArray.Length; ++i) {
                r[i] = Path.GetFileName(directoryArray[i]);
            }
            return r;
        }
    }
}