using System;

namespace WebFront
{
	public interface ISite { string Generate(params string[] parms); }
	public class TestSite : ISite {
		public string Generate(params string[] parms) {
			return "<html>"+HTML.Body(
				HTML.Title("Hey").Class("hi"),
				HTML.Span("You're attempting to connect to: "),
				HTML.Span(parms[0])
				)+"</html>";
		}
	}

}

