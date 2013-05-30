using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
        public HTML.Webpage Generate(SitePass SP, Dictionary<string, string> args) {
            string[] divURL = SP.Host.Split('.');
            switch (divURL[0]) {
                case "test":
                    return this.SNSYS_Test(SP, args);
                default:
                    return this.SNSYS_Art(SP, args);
            }
        }
        public HTML.Webpage SNSYS_Art(SitePass SP, Dictionary<string, string> args) {
            string galName = args.ContainsKey("collection") ? args["collection"] : "Art";
            HTML.Webpage WP = new HTML.Webpage(galName + " - Sensory Systems");
            WP.Head += HTML.Link().Rel("stylesheet").Href("/snsys.css");
            string galPath = Path.Combine(rootPath, galName);
            Console.WriteLine(galPath);
            bool homePage = galName == "Art" || SnsysHelper.IsForbiddenGallery(galName) || !Directory.Exists(galPath) ? true : false;
            if (homePage) {
                WP.Body += SnsysUSGeneric.TitleBar(galName);
                List<HTMLContent> LI = new List<HTMLContent>();
                foreach (string f in Directory.GetDirectories(rootPath)) {
                    string collectionName = SnsysHelper.Isolate(f);
                    if (!SnsysHelper.IsHiddenGallery(collectionName)) {
                        LI.Add(HTML.Attribute(HTML.H1(collectionName).Class("light")).Href(SP.TotalURL + "?collection=" + collectionName).Class("light"));
                        LI.Add(HTML.Span(" | ").Class("light"));
                        foreach (string sf in SnsysHelper.Isolate(Directory.GetDirectories(f))) {
                            LI.Add(HTML.Attribute(sf).Class("light").Href(SP.TotalURL + "?collection=" + collectionName + "#" + sf));
                            LI.Add(HTML.Span(" | ").Class("light"));
                        }
                    }
                }
                WP.Body += SnsysUSGeneric.SnsysSub(null, null, LI.ToArray());
            } else {
                string[] level1Subdirs = SnsysHelper.Isolate(Directory.GetDirectories(galPath));
                string subtitle = HTML.Span(" | ").Class("light").ToString();
                foreach (string sf in level1Subdirs) {
                    subtitle += HTML.Attribute(sf).Class("light").Href("#" + sf);
                    subtitle += HTML.Span(" | ").Class("light");
                }
                WP.Body += SnsysUSGeneric.TitleBar(galName, subtitle);
                foreach (string l1s in level1Subdirs) {
                    List<HTMLContent> LI = new List<HTMLContent>();
                    string level2Path = Path.Combine(galPath, l1s);
                    string[] level2Subdirs = SnsysHelper.Isolate(Directory.GetDirectories(level2Path));
                    foreach (string l2s in level2Subdirs) {
                        string level3Path = Path.Combine(level2Path, l2s);
                        LI.Add(SnsysUSGeneric.SnsysBar(l2s));
                        string[] level3Files = Directory.GetFiles(level3Path);
                        string[] level3FilesIsolated = SnsysHelper.Isolate(level3Files);
                        HTMLContent[] tableEntries = new HTMLContent[level3Files.Length];
                        for (int i = 0; i < level3Files.Length; ++i) {
                            string thumbName;
                            if (ArtSnsysHelper.HasValidExtension(level3FilesIsolated[i], out thumbName)) {
                                string thumbPath = Path.Combine(thumbRootPath, galName, l1s, l2s, thumbName);
                                Console.WriteLine(thumbPath);
                                ArtSnsysHelper.HandleThumbs(level3Files[i], thumbPath);

                                string imageURL = String.Join("/", "Art", galName, l1s, l2s, level3FilesIsolated[i]);
                                string thumbURL = String.Join("/", "Art", ".thumb", galName, l1s, l2s, thumbName);
                                tableEntries[i] = HTML.Attribute(HTML.Image().Src(thumbURL).Class("gal")).Href(imageURL);
                            }
                        }
                        LI.Add(HTML.Div(HTML.SimpleTable(3, tableEntries).Class("gallery")).Class("galwrap"));
                    }
                    WP.Body += SnsysUSGeneric.SnsysSub(l1s, null, LI.ToArray());
                }
            }
            return WP;
        }
        public HTML.Webpage SNSYS_Main(SitePass SP, Dictionary<string, string> args) {
            HTML.Webpage WP = new HTML.Webpage("Sensory Systems Main Page");
            WP.Head += HTML.Link().Rel("stylesheet").Href("/snsys.css");
            WP.Body += SnsysUSGeneric.TitleBar("Main Page");
            WP.Body += SnsysUSGeneric.SnsysSub("Hello!", null, HTML.H1("Welcome to Sensory Systems!").Class("light"));
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
        private static string[] hiddenGalleries = new string[] { "Test Hidden" };
        public static bool IsHiddenGallery(string name) {
            foreach (string s in forbiddenGalleries) {
                if (name == s) { return true; }
            }
            foreach (string s in hiddenGalleries) {
                if (name == s) { return true; }
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
            Bitmap oB = new Bitmap(originalPath);
            Size oldSize = oB.Size;
            Size newSize = oldSize.Width > oldSize.Height ? new Size(160, (int)Math.Round(((float)oldSize.Height / (float)oldSize.Width) * 160.0f)) : new Size((int)Math.Round(((float)oldSize.Width / (float)oldSize.Height) * 160.0f), 160);
            Bitmap nB = new Bitmap(oB, newSize);
            nB.Save(destinationPath, ImageFormat.Jpeg);
        }
    }
}