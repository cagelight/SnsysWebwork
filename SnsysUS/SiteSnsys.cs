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
			WP.Head += HTML.Link().Rel("stylesheet").Href("http://snsys.us/snsys.css");
			WP.Body += SnsysUSGeneric.TitleBar("Test Page");
			return WP.ToString();
		}
	}
}

