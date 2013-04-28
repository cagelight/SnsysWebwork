using System;

using WebFront;

namespace SnsysUS
{
	public static class SnsysUSGeneric {
		public static HTMLContent TitleBar (string title, string subtitle = null) {
			HTMLContent MD = HTML.Div().Class("headbar");
			MD += HTML.Attribute( HTML.Image().Src("http://snsys.us/sensory-systems.png") ).Href("http://snsys.us").Class("ulsite");
			MD += HTML.Div( HTML.H1(title).Class("dark") ).Class("headtit");
			if (subtitle != null) {MD += HTML.Div( HTML.Span(subtitle).Class("light") ).Class("headsub");}
			return MD;
		}
		public static HTMLContent SnsysSub (string data, string title = null, string tag = null) {
			HTMLContent MD = HTML.Div().Class("galsub");
			MD += HTML.Div( HTML.Div((title!=null||tag!=null)?HTML.Attribute(title!=null?HTML.H3(title).Class("dark"):null).ID(tag!=null?tag:title):null).Class("subtop") , HTML.Div(HTML.Span(data).Class("light")).Class("subbody") ).Class("sub2");
			return MD;
		}
	}
}