using System;

namespace WebFront
{
	public class Generic
	{
		public static string SimpleAuth (string authname, string returnurl) {
			HTMLContent form = HTML.Form().Action(returnurl).Method("post");
			form += HTML.Input().Type("hidden").Name("POSTType").Value("SAUTH");
			form += HTML.Input().Type("text").Name(authname);
			form += HTML.Input().Type("submit").Value("Authorize");
			return form.ToString();
		}
	}
}

