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
	public class SnsysUSWeb : ISite {

		public string Generate(params string[] parms) {
			HTML.Webpage WP = new HTML.Webpage("Test Page");
			WP.Head += HTML.Link().Rel("stylesheet").Href("/snsys.css");
			if (parms.Length >= 2 && parms[1]!=null) {WP.Body += HTML.Div( HTML.Span(parms[1]) );}
			WP.Body += SnsysUSGeneric.TitleBar("Test Page");
			WP.Body += SnsysUSGeneric.SnsysSub("Testing a sub.", "Test Title");
			return WP.ToString();
		}
	}
}

