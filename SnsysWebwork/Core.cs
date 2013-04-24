using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading ;

using WebBack;
using SnsysUS;

namespace SnsysWebwork
{
	public class Executor {
		public static int Main(String[] args) {
			Console.WriteLine((new SnsysUSWeb()).Generate("URLGOESHERE"));
			SnsysUSServer TS = new SnsysUSServer(IPAddress.Any, 8080);
			TS.Start();
			while (true) {
				Console.ReadLine();
			}
		}
		
	}
}

