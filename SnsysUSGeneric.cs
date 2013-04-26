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
	}
}

