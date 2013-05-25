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
			SnsysUSServer TS = new SnsysUSServer(IPAddress.Any, 8080);
			TS.Start();
			while (true) {
				string instr = Console.ReadLine();
				string[] instrS = instr.Split(' ');
				switch (instrS[0].ToUpper()) {
				case "EXIT":
					return 0;
				}
			}
		}
	}
}