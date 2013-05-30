using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using WebBack;
using WebFront;

namespace SnsysUS {
    public class SnsysUSWeb {

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
            string rootPath = Path.Combine(Environment.CurrentDirectory, "Assets", "Art");
            string thumbPath = Path.Combine(Environment.CurrentDirectory, "Assets", ".thumb");
            string galPath = Path.Combine(rootPath, galName);
            bool homePage = galName == "Art" || !Directory.Exists(galPath) ? true : false;
            if (homePage) {
                WP.Body += SnsysUSGeneric.TitleBar(galName);
                List<HTMLContent> LI = new List<HTMLContent>();
                foreach (string f in Directory.GetDirectories(rootPath)) {
                    if (!SnsysHelper.IsHiddenGallery(f)) {
                        string collectionName = SnsysHelper.Isolate(f);
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
                            tableEntries[i] = HTML.H3(level3FilesIsolated[i]).Class("light");
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
            directory = Path.GetFileName(directory);
            return directory;
        }
        public static string[] Isolate(params string[] directoryArray) {
            for (int i = 0; i < directoryArray.Length; ++i) {
                directoryArray[i] = Path.GetFileName(directoryArray[i]);
            }
            return directoryArray;
        }
        public static void Isolate(ref string directory) {
            directory = Path.GetFileName(directory);
            return;
        }
        public static void Isolate(ref string[] directoryArray) {
            for (int i = 0; i < directoryArray.Length; ++i) {
                directoryArray[i] = Path.GetFileName(directoryArray[i]);
            }
            return;
        }
        private static string[] hiddenGalleries = new string[] { ".thumb" };
        public static bool IsHiddenGallery(string name) {
            foreach (string s in hiddenGalleries) {
                if (name == s) { return true; }
            }
            return false;
        }
    }
}