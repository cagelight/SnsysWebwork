using System;
using System.Collections.Generic;

using WebFront;

namespace SnsysUS {
    public static class SnsysUSGeneric {
        public static HTMLContent TitleBar(string title, string subtitle = null) {
            HTMLContent MD = HTML.Div().Class("headbar");
            MD += HTML.Attribute(HTML.Image().Src("http://snsys.us/sensory-systems.png")).Href("http://snsys.us").Class("ulsite");
            MD += HTML.Div(HTML.H1(title).Class("dark")).Class("headtit");
            if (subtitle != null) { MD += HTML.Div(HTML.Span(subtitle).Class("light")).Class("headsub"); }
            return MD;
        }
        public static HTMLContent SnsysSub(string title = null, string tag = null, params IElement[] data) {
            HTMLContent MD = HTML.Div().Class("galsub");
            MD += HTML.Div(HTML.Div((title != null || tag != null) ? HTML.Attribute(title != null ? HTML.H3(title).Class("dark") : null).ID(tag != null ? tag : title) : null).Class("subtop"), HTML.Div(data).Class("subbody")).Class("sub2");
            return MD;
        }
        public static HTMLContent SnsysBar(string name = null, string tag = null, string link = null) {
            HTMLContent A = HTML.Attribute().Class("light");
            if (tag != null) { A.ID(tag); }
			if (link != null) { A.Href(link); }
            A += HTML.Div(HTML.H3(name).Class("dark")).Class("subbar").Style(name == null ? Style.Width(400) : Style.Width(600));
            HTMLContent MD = HTML.Div(A).Class("bar");
            return MD;
        }
		private static HTMLContent[] footbarStuff = new HTMLContent[] {
			HTML.Attribute("Art Home").Class("dark").Style(Style.FontWeight(Style.FONTWEIGHT.BOLD)).Href("/"),
			HTML.Attribute("Top").Class("dark").Style(Style.FontWeight(Style.FONTWEIGHT.BOLD)).Href("#Top"),
			HTML.Attribute("Hide All").Class("dark").Style(Style.FontWeight(Style.FONTWEIGHT.BOLD)).Href("javascript:hideAll()"),
			HTML.Attribute("Show All").Class("dark").Style(Style.FontWeight(Style.FONTWEIGHT.BOLD)).Href("javascript:showAll()"),
			HTML.Attribute("Toggle Scrollbars").Class("dark").Style(Style.FontWeight(Style.FONTWEIGHT.BOLD)).Href("javascript:toggleScroll()").Title("Toggles scrollbars on galleries larger than 3 rows. Reduces page length when applicable."),
		};
		public static HTMLContent GalFootBar() {
			HTMLContent RD = HTML.Div ().Class ("footbar");
			RD += HTML.Span (" | ").Class ("dark");
			foreach (HTMLContent HC in SnsysUSGeneric.footbarStuff) {
				RD += HC;
				RD += HTML.Span (" | ").Class ("dark");
			}
			return RD;
		}
    }
}