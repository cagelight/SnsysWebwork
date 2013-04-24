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
			FileProcessor FP = new FileProcessor(sp, Path.Combine(Environment.CurrentDirectory, "Assets"));
			if (!FP.Process()) {
				sp.writeSuccess();
				sp.WriteToClient(sitelogic.Generate(sp.http_host + sp.http_url));
			}
		}
	}
}

