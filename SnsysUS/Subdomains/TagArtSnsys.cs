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
		public HTML.Webpage SNSYS_TagArt(SnsysUSServer parent, SitePass SP, Dictionary<string, string> args) {
			return new HTML.Webpage ("Tag Art Test");
		}
	}
}

