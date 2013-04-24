using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading ;

using WebBack;

namespace SnsysUS
{
	public class SnsysUSServer : GenericServer, IServer {
		public SnsysUSServer(IPAddress addr, ushort port) : base(new SnsysUSWeb(), addr, port) {}
		public override void HandleGET (HTTPProcessor sp) {
			if (!this.FileHandler(sp)) {
				sp.writeSuccess();
				sp.outputStream.WriteLine(sitelogic.Generate(sp.http_host + sp.http_url));
			}
		}
		public bool FileHandler (HTTPProcessor sp, string urlinfo = null) {
			if (urlinfo == null) {urlinfo = sp.http_url;}
			string[] divurl = urlinfo.Split('/');
			string filename = divurl[divurl.Length-1];
			string filepath = Environment.CurrentDirectory;
			if (divurl.Length > 2) {
				string[] splitfilepath = new string[divurl.Length - 1];
				splitfilepath[0] = Environment.CurrentDirectory;
				for (int i=1;i<divurl.Length-1;i++) {splitfilepath[i] = divurl[i];}
				filepath = Path.Combine(splitfilepath);
			}
			string totalpath = Path.Combine(filepath, filename);
			if (filename != "" && File.Exists(totalpath)) {
				if (filename.EndsWith(".txt")) {
					sp.writeSuccess("plain");
					sp.outputStream.WriteLine(Encoding.UTF8.GetString(File.ReadAllBytes(totalpath)));
					return true;
				}
			}
			return false;
		}
	}
}

