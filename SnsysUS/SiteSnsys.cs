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
		public string Generate(SitePass SP, params ArgumentPair[] args) {
			HTML.Webpage WP = new HTML.Webpage("Test Page");
			WP.Head += HTML.Link().Rel("stylesheet").Href("/snsys.css");
			WP.Body += SnsysUSGeneric.TitleBar(SP.Path, SP.TotalURL);
            foreach (ArgumentPair AP in args) {
                WP.Body += SnsysUSGeneric.SnsysSub(AP.Value, AP.Key);
            }
			return WP.ToString();
		}
	}
}