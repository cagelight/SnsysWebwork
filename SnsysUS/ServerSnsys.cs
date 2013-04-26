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
	}
}

