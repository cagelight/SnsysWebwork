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
			return "<html>"+HTML.Body(
				HTML.Title("Hey").Class("hi"),
				HTML.Span("You're attempting to connect to: "),
				HTML.Span(parms[0])
				)+"</html>";
		}
	}
}

