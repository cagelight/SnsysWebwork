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
		public SnsysUSServer(IPAddress addr, ushort port) : base(new SnsysUSWeb(), addr, port) {}
		public override void HandleGET (HTTPProcessor sp) {
			FileProcessor FP = new FileProcessor(sp, Path.Combine(Environment.CurrentDirectory, "Assets"));
			RestrictionInfo RI = sitelogic.IsURLRestricted(sp.http_url);
			if (!RI || EvaluateClient(sp.clientip, RI.restrictionTitle)) {
				if (!FP.Process()) {
					sp.writeSuccess();
					sp.WriteToClient(sitelogic.Generate(sp.http_host + sp.http_url));
				}
			} else {
				sp.writeSuccess();
				sp.WriteToClient(Generic.SimpleAuth(RI.restrictionTitle,"http://"+sp.http_host+sp.http_url));
			}
		}
	}
}

