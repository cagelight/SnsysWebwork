using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading ;

using WebBack;
using WebFront;

namespace SnsysUS
{
	public class SnsysUSWeb : ISite {
		public string Generate(params string[] parms) {
			return "<html>"+HTML.Body( HTML.Title("Hey"), HTML.Span(String.Format("You're attempting to connect to: \"{0}\", which does not exist.", parms[0])) )+"</html>";
		}

		public RestrictionInfo IsURLRestricted (string url) {
			if (url.Contains(".jpg")) {
				return new RestrictionInfo(true, "L1");
			} else {
				return false;
			}
		}
	}
}

