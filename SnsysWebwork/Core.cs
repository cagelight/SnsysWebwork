using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using WebBack;
using SnsysUS;

namespace SnsysWebwork
{
	public class Executor {
		public static int Main(String[] args) {
			Console.WriteLine ("Welcome to SnsysWebwork.");
			SnsysUSServer TS = new SnsysUSServer(IPAddress.Any, 80);
			TS.Start();
			while (true) {
				string instr = Console.ReadLine();
				string[] instrS = instr.Split(' ');
				switch (instrS[0].ToUpper()) {
				case "EXIT":
					return 0;
				case "RELOAD":
					TS.ReloadConfigurations ();
					Console.WriteLine ("Configurations Reloaded");
					break;
				}
			}
		}
	}
}