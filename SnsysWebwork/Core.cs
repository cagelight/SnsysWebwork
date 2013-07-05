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
using SnsysDatabase;

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
				case "START":
					TS.Start();
					break;
				case "SDB":
					SingleLayerSnsysDatabase TSDB = new SingleLayerSnsysDatabase ("TESTDATABASELOL");
					TSDB.Database["TEST1"] = new IDatablock[] { new StringDatablock("VALUE1"), new FloatDatablock(0.5f) };
					TSDB.Database["TEST2"] = new IDatablock[] { new ByteDatablock(0x8A), new UShortDatablock(32025) };
					TSDB.Write ("test.sdb");
					TSDB.Debug ();
					break;
				case "DDB":
					DoubleLayerSnsysDatabase TDDB = new DoubleLayerSnsysDatabase ("TESTDATABASELOL");
					TDDB ["TEST1"]["MARK1"] = new IDatablock[] { new FloatDatablock(0.55f), new FloatDatablock(0.25f) };
					TDDB ["TEST1"]["MARK2"] = new IDatablock[] { new DoubleDatablock(0.55d) };
					TDDB ["TEST2"]["MARK1"] = new IDatablock[] { new SByteDatablock(-9) };
					TDDB ["TEST2"]["MARK2"] = new IDatablock[] { new ShortDatablock(-20040) };
					TDDB ["TEST2"]["MARK3"] = new IDatablock[] { new UIntDatablock(4050505050), new LongDatablock(1234567890123456789) };
					TDDB.Write ("test2.sdb");
					TDDB.Debug ();
					break;
				default:
					Console.WriteLine ("Unknown Command.");
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