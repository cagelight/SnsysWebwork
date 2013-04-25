using System;

namespace WebFront
{
	public class Generic
	{
		public static string SimpleAuth (string authname, string returnurl) {
			return HTML.Form( HTML.Input().Type("text").Name(authname) , HTML.Input().Type("text").Name(authname+"D") , HTML.Input().Type("submit").Value("Authorize") ).Action(returnurl).Method("post").ToString();
		}
	}
}

