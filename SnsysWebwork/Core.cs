using System;
using System.Collections;
using System.Collections.Generic;
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

			Dictionary<string,string> sortedArgs = new Dictionary<string, string> ();
			foreach (string s in args) {
				try {
					string[] a = s.Split('=');
					if (a.Length != 2) {throw new ArgumentException("Malformed argument in arguments.");}
					sortedArgs[a[0]] = a[1];
				} catch (Exception e) {
					Console.WriteLine (e);
				}
			}

			Console.WriteLine ("Welcome to SnsysWebwork.");
			Thread BPDF = new Thread (new ThreadStart(BrokenPipeDefenseForce));
			BPDF.Start ();
			SnsysUSServer TS = new SnsysUSServer(IPAddress.Any);
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

		private static void BrokenPipeDefenseForce () {
			while (true) {
				Thread.Sleep (5000);
				Console.WriteLine ("BPDF Tick");
			}
		}
	}
}