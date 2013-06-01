using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using WebBack;
using WebFront;

namespace SnsysUS {
    public class SnsysUSWeb {
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
                default:
                    return this.SNSYS_Main(SP, args);
            }
        }
        public HTML.Webpage SNSYS_Art(SnsysUSServer parent, SitePass SP, Dictionary<string, string> args) {
            string galName = args.ContainsKey("collection") ? args["collection"] : "Art";
            HTML.Webpage WP = new HTML.Webpage(galName + " - Sensory Systems");
            WP.Head += HTML.Link().Rel("stylesheet").Href("/snsys.css");
			WP.Head += HTML.Script("").Src("/jquery-1.10.1.min.js");
			WP.Head += HTML.Script("").Src("/gal.js");
            string galPath = Path.Combine(rootPath, galName);
            bool homePage = galName == "Art" || SnsysUSWeb.IsForbiddenGallery(galName) || !Directory.Exists(galPath) ? true : false;
            if (homePage) {
                WP.Body += SnsysUSGeneric.TitleBar(galName);
                List<HTMLContent> LI = new List<HTMLContent>();
                foreach (string f in Directory.GetDirectories(rootPath)) {
                    string collectionName = SnsysHelper.Isolate(f);
					if (!this.IsHiddenGallery(collectionName, parent.restrictedWords)) {
                        LI.Add(HTML.Attribute(HTML.H1(collectionName).Class("light")).Href(SP.TotalURL + "?collection=" + collectionName).Class("light"));
                        LI.Add(HTML.Span(" | ").Class("light"));
                        foreach (string sf in SnsysHelper.Isolate(Directory.GetDirectories(f))) {
                            if (sf != collectionName) {
                                LI.Add(HTML.Attribute(sf).Class("light").Href(SP.TotalURL + "?collection=" + collectionName + "#" + sf));
                                LI.Add(HTML.Span(" | ").Class("light"));
                            }
                        }
                    }
                }
                WP.Body += SnsysUSGeneric.SnsysSub(null, null, LI.ToArray());
            } else {
                string[] level1Subdirs = SnsysHelper.Isolate(Directory.GetDirectories(galPath));
                string subtitle = HTML.Span(" | ").Class("light").ToString();
                foreach (string sf in level1Subdirs) {
                    if (sf != galName) {
                        subtitle += HTML.Attribute(sf).Class("light").Href("#" + sf);
                        subtitle += HTML.Span(" | ").Class("light");
                    }
                }
                WP.Body += SnsysUSGeneric.TitleBar(galName, subtitle);
                List<HTMLContent> WA = new List<HTMLContent>();
                foreach (string l1s in level1Subdirs) {
                    List<HTMLContent> LI = new List<HTMLContent>();
                    string level2Path = Path.Combine(galPath, l1s);
                    string[] level2Subdirs = SnsysHelper.Isolate(Directory.GetDirectories(level2Path));
                    foreach (string l2s in level2Subdirs) {
                        string level3Path = Path.Combine(level2Path, l2s);
						LI.Add(SnsysUSGeneric.SnsysBar(l2s, l1s + "_" + l2s, String.Format("javascript:hideGal('#g{0}')", l1s + "_" + l2s)));
                        string[] level3Files = Directory.GetFiles(level3Path);
                        string[] level3FilesIsolated = SnsysHelper.Isolate(level3Files);
                        HTMLContent[] tableEntries = new HTMLContent[level3Files.Length];
                        for (int i = 0; i < level3Files.Length; ++i) {
                            string thumbName;
                            if (ArtSnsysHelper.HasValidExtension(level3FilesIsolated[i], out thumbName)) {
                                string thumbPath = Path.Combine(thumbRootPath, galName, l1s, l2s, thumbName);
                                ArtSnsysHelper.HandleThumbs(level3Files[i], thumbPath);

                                string imageURL = String.Join("/", "Art", galName, l1s, l2s, level3FilesIsolated[i]);
                                string thumbURL = String.Join("/", "Art", ".thumb", galName, l1s, l2s, thumbName);
                                tableEntries[i] = HTML.Attribute(HTML.Image().Src(thumbURL).Class("gal")).Href(imageURL);
                            }
                        }
						LI.Add(HTML.Div(HTML.SimpleTable(3, tableEntries).Class("gallery")).Class("galwrap").ID("g" + l1s + "_" + l2s));
                    }
                    string[] level2Files = Directory.GetFiles(level2Path);
                    if (level2Files.Length > 0) {
                        string[] level2FilesIsolated = SnsysHelper.Isolate(level2Files);
                        HTMLContent[] l2Loose = new HTMLContent[level2Files.Length];
                        for (int i = 0; i < level2Files.Length; ++i) {
                            string thumbName;
                            if (ArtSnsysHelper.HasValidExtension(level2FilesIsolated[i], out thumbName)) {
                                string thumbPath = Path.Combine(thumbRootPath, galName, l1s, thumbName);
                                ArtSnsysHelper.HandleThumbs(level2Files[i], thumbPath);

                                string imageURL = String.Join("/", "Art", galName, l1s, level2FilesIsolated[i]);
                                string thumbURL = String.Join("/", "Art", ".thumb", galName, l1s, thumbName);
                                l2Loose[i] = HTML.Attribute(HTML.Image().Src(thumbURL).Class("gal")).Href(imageURL);
                            }
                        }
						LI.Add(SnsysUSGeneric.SnsysBar(null,l1s + "_", String.Format("javascript:hideGal('#g{0}')", l1s + "_")));
                        LI.Add(HTML.Div(HTML.SimpleTable(3, l2Loose).Class("gallery")).Class("galwrap").ID("g" + l1s + "_"));
                    }
                    if (galName != l1s) {
                        WA.Add(SnsysUSGeneric.SnsysSub(l1s, null, LI.ToArray()));
                    } else {
                        WA.Insert(0, SnsysUSGeneric.SnsysSub(null, null, LI.ToArray()));
                    }
                }
                foreach (HTMLContent HC in WA) {
                    WP.Body += HC;
                }
                string[] level1Files = Directory.GetFiles(galPath);
                if (level1Files.Length > 0) {
                    List<HTMLContent> LI = new List<HTMLContent>();
                    string[] level1FilesIsolated = SnsysHelper.Isolate(level1Files);
                    HTMLContent[] l2Loose = new HTMLContent[level1Files.Length];
                    for (int i = 0; i < level1Files.Length; ++i) {
                        string thumbName;
                        if (ArtSnsysHelper.HasValidExtension(level1FilesIsolated[i], out thumbName)) {
                            string thumbPath = Path.Combine(thumbRootPath, galName, thumbName);
                            ArtSnsysHelper.HandleThumbs(level1Files[i], thumbPath);

                            string imageURL = String.Join("/", "Art", galName, level1FilesIsolated[i]);
                            string thumbURL = String.Join("/", "Art", ".thumb", galName, thumbName);
                            l2Loose[i] = HTML.Attribute(HTML.Image().Src(thumbURL).Class("gal")).Href(imageURL);
                        }
                    }
					LI.Add(SnsysUSGeneric.SnsysBar(null, "_", "javascript:hideGal('#g_')"));
                    LI.Add(HTML.Div(HTML.SimpleTable(3, l2Loose).Class("gallery")).Class("galwrap").ID("g_"));
                    WP.Body += SnsysUSGeneric.SnsysSub(null, null, LI.ToArray());
                }
				WP.Body += SnsysUSGeneric.GalFootBar ();
            }
            return WP;
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

    public static class ArtSnsysHelper {
        private static string[] validExtensions = new string[] { "bmp", "gif", "jpg", "jpeg", "png", "tiff" };
        public static bool HasValidExtension(string filename, out string filenameThumbJPG) {
            string checkExtension = filename.Substring(filename.LastIndexOf('.') + 1);
            filenameThumbJPG = filename.Substring(0, filename.LastIndexOf('.')) + ".jpg";
            foreach (string validExtension in validExtensions) {
                if (checkExtension == validExtension) {
                    return true;
                }
            }
            return false;
        }
        public static void HandleThumbs(string originalPath, string destinationPath) {
            if (File.Exists(originalPath) && !File.Exists(destinationPath)) {
                Console.WriteLine(String.Format("Creating thumbnail:\n{0}\n", destinationPath));
                if (!Directory.Exists(Directory.GetParent(destinationPath).FullName)) {
                    Directory.CreateDirectory(Directory.GetParent(destinationPath).FullName);
                }
                ArtSnsysHelper.CreateThumb(originalPath, destinationPath);
            }
        }
        private static void CreateThumb(string originalPath, string destinationPath) {
			try {
				Image oB = Bitmap.FromFile (originalPath);
	            Size oldSize = oB.Size;
	            Size newSize = oldSize.Width > oldSize.Height ? new Size(160, (int)Math.Round(((float)oldSize.Height / (float)oldSize.Width) * 160.0f)) : new Size((int)Math.Round(((float)oldSize.Width / (float)oldSize.Height) * 160.0f), 160);
	            Bitmap nB = new Bitmap(newSize.Width, newSize.Height);
				Graphics graph = Graphics.FromImage(nB);
				graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graph.CompositingQuality = CompositingQuality.HighQuality;
				graph.SmoothingMode = SmoothingMode.AntiAlias;
				graph.DrawImage(oB, new Rectangle(0, 0, newSize.Width, newSize.Height));

	            nB.Save(destinationPath, ImageFormat.Jpeg);
	            oB.Dispose();
	            nB.Dispose();
			} catch (Exception e){
				Console.WriteLine ("An error has occured while trying to load an image, or create a thumbnail. Error below:");
				Console.WriteLine (e);
			}
        }
    }
}