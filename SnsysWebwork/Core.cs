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

			if (args.Length > 0) {
				switch (args [0].ToUpper()) {
				case "EDIT":
					if (args.Length > 1) {
						EditDatabase (args [1]);
					}
					break;
				case "DBINFO":
					DatabaseInfo DI = SDBHelper.ReadDatabaseHeader (args [1]);
					Console.WriteLine (String.Format("Name: {0}\nSubtype: {1}", DI.name, DI.subtype));
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
				}
				return 0;
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
				default:
					Console.WriteLine ("Unknown Command.");
					break;
				}
			}
		}

		private static void EditDatabase (string dir, bool fromCurrent = true) {
			try {
				DatabaseInfo DI = SDBHelper.ReadDatabaseHeader(dir, fromCurrent);
				switch(DI.subtype) {
				case "BasicSDB":
					break;
				case "DoubleSDB":
					break;
				default:
					throw new FormatException("This version of the SnsysWebwork does not recognize the subtype of this SnsysDatabase.");
				}
			} catch (Exception e) {
				Console.WriteLine (e);
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