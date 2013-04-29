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

	public class SnsysUSServer : IServer {
		public static Dictionary <string, string> LevelKeys = new Dictionary<string, string>() {
			{"L1","DF43D66DE1590F605FCACE63862B73030149931731E914D0A6D1C03571AE3CA83010DDE35865D63EF14B83504DEE4D58846A78F5DC71E1A69C9C68A4CC899C38"}
		};
		private static RestrictionInfo L1 = new RestrictionInfo(true, "L1");
		public ISite sitelogic; 
		public TcpListener tcpl; 
		public Thread process;
		public bool active = false;
		public Dictionary<IPAddress, ClientAuthorizations> clientAuthorization;
		public SnsysUSServer(IPAddress addr, ushort port){
			sitelogic = new SnsysUSWeb(); 
			tcpl = new TcpListener(addr, port);
			clientAuthorization = new Dictionary<IPAddress, ClientAuthorizations>();
		} 
		public void Run () {
			while (this.active) {
				try {
					TcpClient tcpc = tcpl.AcceptTcpClient();
					HTTPProcessor processor = new HTTPProcessor(tcpc, this);
					Thread handleClient = new Thread(new ThreadStart(processor.process));
					handleClient.Start(); 
					Thread.Sleep(1);
				} catch (Exception e) {
					Console.WriteLine(e);
				}
			}
		}
		public void Start () {
			this.tcpl.Start();
			this.active = true;
			this.process = new Thread(Run);
			this.process.Start();
		}
		public void HandleGET(HTTPProcessor sp) {
			RestrictionInfo RI = IsURLRestricted(sp.http_url);
			if (!RI || EvaluateClient(sp.clientip, sp.clientcookie,  RI.restrictionTitle)) {
				if (!HandleFiles(sp)) {
					sp.writeSuccess();
					sp.WriteToClient(sitelogic.Generate(sp.http_host + sp.http_url));
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
						if (!clientAuthorization.ContainsKey(sp.clientip)) {clientAuthorization.Add(sp.clientip, new ClientAuthorizations()); }
						if (IsLevelKeyValid(KVP.Key, KVP.Value)) {
							SCookie nc = SCookie.GenerateNew(KVP.Key);
							clientAuthorization[sp.clientip].Add(KVP.Key, nc.key);
							sp.writeCookie(nc);
						}
					}
					break;
				default:
					string errstring = "";
					foreach (KeyValuePair<string,string> E in MKVP.Value) {
						errstring += String.Format("{0}={1}\n", E.Key, E.Value);
					}
					Console.WriteLine(String.Format("An unknown POST request type was made: {0}, containing:\n {1}", MKVP.Key, errstring));
					break;
				}
			}
			sp.writeClose();
		}
		public bool HandleFiles (HTTPProcessor sp) {
			FileProcessor FP = new FileProcessor(sp, Path.Combine(Environment.CurrentDirectory, "Assets"));
			FP.processMethodStrings = new List<string>(){"5"};
			return FP.Process(FileProcessor.METHOD.BLACKLIST_CONTAINS, false);
		}
		
		public bool EvaluateClient (IPAddress addr, SCookie c, string authlevel) {
			if (clientAuthorization.ContainsKey(addr) && c.key!=null && clientAuthorization[addr].CheckAuth(authlevel, c.key)) {return true;}
			return false;
		}
		public RestrictionInfo IsURLRestricted (string url) {
			if (url.Contains(".png")) {
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

