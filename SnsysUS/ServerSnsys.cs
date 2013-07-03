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
using SnsysDatabase;

namespace SnsysUS
{

	public class SnsysUSServer : IServer {
		public static Dictionary <string, string> LevelKeys = new Dictionary<string, string>() {
			{"L1","DF43D66DE1590F605FCACE63862B73030149931731E914D0A6D1C03571AE3CA83010DDE35865D63EF14B83504DEE4D58846A78F5DC71E1A69C9C68A4CC899C38"}
		};
		private static RestrictionInfo L1 = new RestrictionInfo(true, "L1");
        public SnsysUSWeb sitelogic = new SnsysUSWeb();
		public TcpListener httptcpl;
		public Thread httpprocess;
		public bool active = false;
		public Dictionary<IPAddress, ClientAuthorizations> clientAuthorization;

		public string[] restrictedWords;

		public SnsysUSServer(IPAddress addr){
			httptcpl = new TcpListener(addr, 80);
			clientAuthorization = new Dictionary<IPAddress, ClientAuthorizations>();
			this.restrictedWords = SnsysUSServer.LoadRestrictedWordsFile ();

		} 
		public void ReloadConfigurations () {
			this.restrictedWords = SnsysUSServer.LoadRestrictedWordsFile ();
		}
		public void HTTPRun () {
			while (this.active) {
				try {
					TcpClient tcpc = httptcpl.AcceptTcpClient();
					HTTPProcessor processor = new HTTPProcessor(tcpc, this);
					Thread handleClient = new Thread(new ThreadStart(processor.process));
					handleClient.Start(); 
				} catch (Exception e) {
					Console.WriteLine(e);
				}
			}
		}
		public void Start () {
			this.httptcpl.Start();
			this.active = true;
			this.httpprocess = new Thread(HTTPRun);
			this.httpprocess.Start();
		}
		public void HandleGET(HTTPProcessor sp) {
			/*if (sp.http_host == "etherpad.snsys.us") {
				WebRequest WReq = HttpWebRequest.Create(new Uri("http://127.0.0.1:9001/" + sp.http_url));
				WebResponse WRes = WReq.GetResponse ();
				sp.WriteToClient (new StreamReader(WRes.GetResponseStream()).ReadToEnd());
				return;
			}*/
            RestrictionInfo RI;
			if (!IsURLRestricted(sp.http_url, out RI) || EvaluateClient(sp.clientip, sp.clientcookies,  RI.restrictionTitle)) {
				if (!HandleFiles(sp)) {
                    SitePass SP = new SitePass(sp.http_host, sp.http_url);
                    Dictionary<string,string> AP = ArgumentHelper.Organize(SP.Path, out SP.Path);
                    sp.writeSuccess();
                    sp.WriteToClient(sitelogic.Generate(this, SP, AP).ToString());
					sp.WriteToClient(String.Format("<!-- {0}ms elapsed. -->", sp.elapsedTime.TotalMilliseconds));
				}
			} else {
				sp.writeSuccess();
				sp.WriteToClient(Generic.SimpleAuth(RI.restrictionTitle,"http://"+sp.http_host+sp.http_url));
			}
		}
		public void HandlePOST (HTTPProcessor sp, StreamReader sr) {
			sp.write200();
			sp.writeType("text/html");
			List<string> responseText = new List<string>();
			foreach (KeyValuePair<string,Dictionary<string,string>> MKVP in HTTPProcessor.ProcessPOST(sr.ReadToEnd())) {
				switch (MKVP.Key) {
				case "SAUTH":
					foreach (KeyValuePair<string,string> KVP in MKVP.Value) {
						if (!clientAuthorization.ContainsKey(sp.clientip)) { clientAuthorization.Add(sp.clientip, new ClientAuthorizations()); }
						if (IsLevelKeyValid(KVP.Key, KVP.Value)) {
							SCookie nc = SCookie.GenerateNew(KVP.Key);
							clientAuthorization[sp.clientip].Add(KVP.Key, nc.key);
							sp.writeCookie(nc);
							responseText.Add(String.Format("Authorization \"{0}\" Granted.", KVP.Key));
						} else {
							responseText.Add(String.Format("Authorization \"{0}\" Denied.", KVP.Key));
						}
					}
					break;
				default:
					string errstring = "";
					foreach (KeyValuePair<string,string> E in MKVP.Value) {
						errstring += String.Format("{0}={1}\n", E.Key, E.Value);
					}
					string errtext = String.Format("An unknown POST request type was made: {0}, containing: \n{1}", MKVP.Key, errstring);
					responseText.Add(errtext); Console.WriteLine(errtext);
					break;
				}
			}
			sp.writeClose();
			HTML.Webpage RC = new HTML.Webpage("POST");
			HTMLContent MDV = HTML.Div().Style(Style.BackgroundColor(new HexColor(192,192,255)));
			foreach (string st in responseText) {
				MDV += HTML.Span(st);
				MDV += HTML.Breakline;
			}
			MDV += HTML.Attribute( new TextElement("Click here to attempt access.") ).Href("http://" + sp.http_host + sp.http_url);
			RC.Body += MDV;
			sp.WriteToClient(RC.ToString());
		}
		public bool HandleFiles (HTTPProcessor sp) {
			FileProcessor FP = new FileProcessor(sp, Path.Combine(Environment.CurrentDirectory, "Assets"));
			//FP.processMethodStrings = new List<string>(){"RESTRICTED"};
			return FP.Process(FileProcessor.METHOD.GREY, false, false);
		}
		
		public bool EvaluateClient (IPAddress addr, List<SCookie> cs, string authlevel) {
			foreach (SCookie c in cs) {
				if (clientAuthorization.ContainsKey(addr) && c.key!=null && clientAuthorization[addr].CheckAuth(authlevel, c.key)) {return true;}
			}
			return false;
		}
		public bool IsURLRestricted (string url, out RestrictionInfo RI) {
			foreach (string restr in this.restrictedWords) {
				if (url.ToLower().Contains (restr.ToLower())) {
					RI = L1;
					return true;
				}
			}
	        RI = RestrictionInfo.NONE;
	        return false;
		}
		
		public bool IsLevelKeyValid (string level, string key) {
			if (LevelKeys.ContainsKey(level) && LevelKeys[level] == KeyHelper.HexFromSHA512Hash(key)) {
				return true;
			}
			return false;
		}

		public static string[] LoadRestrictedWordsFile () {
			try {
				string rawRestriction = new StreamReader (File.OpenRead(Path.Combine(Environment.CurrentDirectory, "RestrictedWords.conf"))).ReadToEnd();
				return rawRestriction.Replace("\n","").Split (';');
			} catch (Exception e){
				Console.WriteLine (e);
				return new string[] {};
			}
		}
	}
}

