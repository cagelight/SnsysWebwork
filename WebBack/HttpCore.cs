using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading ;

using WebFront;

namespace WebBack
{
	public interface IServer {void Start(); void Run(); void HandleGET(HTTPProcessor sp); void HandlePOST(HTTPProcessor sp, StreamReader sr);}

	public abstract class GenericServer : IServer {
		public ISite sitelogic; 
		public TcpListener tcpl; 
		public Thread process;
		public bool active = false;
		public Dictionary<IPAddress, ClientAuthorizations> clientAuthorization;
		public GenericServer(ISite insitelogic, IPAddress addr, ushort port){
			sitelogic = insitelogic; 
			tcpl = new TcpListener(addr, port);
			clientAuthorization = new Dictionary<IPAddress, ClientAuthorizations>();
		} 
		public virtual void Run () {
			while (this.active) {
				TcpClient tcpc = tcpl.AcceptTcpClient();
				HTTPProcessor processor = new HTTPProcessor(tcpc, this);
				Thread handleClient = new Thread(new ThreadStart(processor.process));
				handleClient.Start(); 
				Thread.Sleep(1);
			}
		}
		public virtual void Start () {
			this.tcpl.Start();
			this.active = true;
			this.process = new Thread(Run);
			this.process.Start();
		}
		public virtual void HandleGET(HTTPProcessor sp) {
			FileProcessor FP = new FileProcessor(sp, Path.Combine(Environment.CurrentDirectory, "Assets"));
			RestrictionInfo RI = sitelogic.IsURLRestricted(sp.http_url);
			if (!RI || EvaluateClient(sp.clientip, sp.clientcookie,  RI.restrictionTitle)) {
				if (!FP.Process()) {
					sp.writeSuccess();
					sp.WriteToClient(sitelogic.Generate(sp.http_host + sp.http_url));
				}
			} else {
				sp.writeSuccess();
				sp.WriteToClient(Generic.SimpleAuth(RI.restrictionTitle,"http://"+sp.http_host+sp.http_url));
			}
		}
		public virtual void HandlePOST (HTTPProcessor sp, StreamReader sr) {
			sp.write200();
			sp.writeType("text/html");
			Dictionary<string, bool> SF = new Dictionary<string, bool>();
			foreach (KeyValuePair<string,string> KVP in HTTPProcessor.ProcessPOST(sr.ReadToEnd())) {
				if (!clientAuthorization.ContainsKey(sp.clientip)) {clientAuthorization.Add(sp.clientip, new ClientAuthorizations()); }
				if (sitelogic.IsLevelKeyValid(KVP.Key, KVP.Value)) {
					SCookie nc = SCookie.GenerateNew(KVP.Key);
					clientAuthorization[sp.clientip].Add(KVP.Key, nc.key);
					sp.writeCookie(nc);
					SF.Add(KVP.Key, true);
				} else {
					SF.Add(KVP.Key, false);
				}
			}
			sp.writeClose();
			foreach (KeyValuePair<string, bool> KVP in SF) {
				sp.WriteToClient( HTML.Span(string.Format("Access to {0} {1}.", KVP.Key, KVP.Value?"Granted":"Denied")).ToString() );
			}
		}

		public virtual bool EvaluateClient (IPAddress addr, SCookie c, string authlevel) {
			if (clientAuthorization.ContainsKey(addr) && c.key!=null && clientAuthorization[addr].CheckAuth(authlevel, c.key)) {return true;}
			return false;
		}
	}

	public class StringRandom {
		public const string NumLet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
		public const string Num = "1234567890";
		public const string Let = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		public const string LetL = "abcdefghijklmnopqrstuvwxyz";
		public const string LetU = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		public static string GenerateNumLet (int length) {
			Random r = new Random((int)DateTime.Now.Ticks);
			char[] c = new char[length];
			for (int i=0;i<length;i++) {
				c[i] = NumLet[r.Next(0,NumLet.Length)];
			}
			return new string(c);
		}
		public static string GenerateUnicode (int length) {
			Random r = new Random((int)DateTime.Now.Ticks);
			byte[] b = new byte[length];
			for (int i=0;i<length;i++) {
				b[i] = (byte)r.Next(0,256);
			}
			return Encoding.Unicode.GetString(b);
		}
	}

	public class HTTPProcessor {
		public TcpClient socket;        
		public IServer srv;
		
		private Stream inputStream;
		public StreamWriter outputStream;
		public BinaryWriter outputBinary;
		private BufferedStream outputCore;
		
		public String http_method;
		public String http_url;
		public String http_protocol_versionstring;
		public string http_host;
		public IPAddress clientip;
		public SCookie clientcookie;
		public Hashtable httpHeaders = new Hashtable();
		
		
		private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB
		
		public HTTPProcessor(TcpClient s, IServer srv) {
			this.clientip = ((IPEndPoint)s.Client.RemoteEndPoint).Address;
			this.socket = s;
			this.srv = srv;                   
		}
		
		
		private string streamReadLine(Stream inputStream) {
			int next_char;
			string data = "";
			while (true) {
				next_char = inputStream.ReadByte();
				if (next_char == '\n') { break; }
				if (next_char == '\r') { continue; }
				if (next_char == -1) { Thread.Sleep(1); continue; };
				data += Convert.ToChar(next_char);
			}            
			return data;
		}
		public void process() {                        
			// we can't use a StreamReader for input, because it buffers up extra data on us inside it's
			// "processed" view of the world, and we want the data raw after the headers
			inputStream = new BufferedStream(socket.GetStream());
			
			// we probably shouldn't be using a streamwriter for all output from handlers either
			outputCore = new BufferedStream(socket.GetStream());
			outputStream = new StreamWriter(outputCore);
			outputBinary = new BinaryWriter(outputCore);
			try {
				parseRequest();
				readHeaders();
				if (http_method.Equals("GET")) {
					handleGETRequest();
				} else if (http_method.Equals("POST")) {
					handlePOSTRequest();
				}
			} catch (Exception e) {
				Console.WriteLine("Exception: " + e.ToString());
				writeFailure();
			}
			outputStream.Flush();
			// bs.Flush(); // flush any remaining output
			inputStream = null; outputStream = null; // bs = null;            
			socket.Close();             
		}
		
		public void parseRequest() {
			String request = streamReadLine(inputStream);
			string[] tokens = request.Split(' ');
			if (tokens.Length != 3) {
				throw new Exception("invalid http request line");
			}
			http_method = tokens[0].ToUpper();
			http_url = tokens[1];
			http_protocol_versionstring = tokens[2];
			
			Console.WriteLine("starting: " + request);
		}
		
		public void readHeaders() {
			Console.WriteLine("readHeaders()");
			String line;
			while ((line = streamReadLine(inputStream)) != null) {
				if (line.Equals("")) {
					Console.WriteLine("got headers");
					return;
				}
				if (line.StartsWith("Host:")) {
					http_host = line.Substring(6);
				}
				if (line.StartsWith("Cookie:")) {
					string[] t = line.Substring(8).Split('=');
					clientcookie = new SCookie(t[0],t[1]);
				}
				int separator = line.IndexOf(':');
				if (separator == -1) {
					throw new Exception("invalid http header line: " + line);
				}
				String name = line.Substring(0, separator);
				int pos = separator + 1;
				while ((pos < line.Length) && (line[pos] == ' ')) {
					pos++; // strip any spaces
				}
				
				string value = line.Substring(pos, line.Length - pos);
				Console.WriteLine("header: {0}:{1}",name,value);
				httpHeaders[name] = value;
			}
		}
		
		public void handleGETRequest() {
			srv.HandleGET(this);
		}
		
		private const int BUF_SIZE = 4096;
		public void handlePOSTRequest() {
			// this post data processing just reads everything into a memory stream.
			// this is fine for smallish things, but for large stuff we should really
			// hand an input stream to the request processor. However, the input stream 
			// we hand him needs to let him see the "end of the stream" at this content 
			// length, because otherwise he won't know when he's seen it all! 
			
			Console.WriteLine("get post data start");
			int content_len = 0;
			MemoryStream ms = new MemoryStream();
			if (this.httpHeaders.ContainsKey("Content-Length")) {
				content_len = Convert.ToInt32(this.httpHeaders["Content-Length"]);
				if (content_len > MAX_POST_SIZE) {
					throw new Exception(
						String.Format("POST Content-Length({0}) too big for this simple server",
					              content_len));
				}
				byte[] buf = new byte[BUF_SIZE];              
				int to_read = content_len;
				while (to_read > 0) {  
					Console.WriteLine("starting Read, to_read={0}",to_read);
					
					int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
					Console.WriteLine("read finished, numread={0}", numread);
					if (numread == 0) {
						if (to_read == 0) {
							break;
						} else {
							throw new Exception("client disconnected during post");
						}
					}
					to_read -= numread;
					ms.Write(buf, 0, numread);
				}
				ms.Seek(0, SeekOrigin.Begin);
			}
			Console.WriteLine("get post data end");
			srv.HandlePOST(this, new StreamReader(ms));
			
		}

		public void write200() {
			WriteToClient("HTTP/1.0 200 OK"); 
		}
		public void writeType(string content_type="text/html") {
			WriteToClient("Content-Type: " + content_type);
		}
		public void writeCookie(SCookie c) {
			WriteToClient(String.Format("Set-Cookie: {0}={1}", c.name, c.key)); 
		}
		public void writeClose() {
			WriteToClient("Connection: close");
			WriteToClient("");
		}
		
		public void writeSuccess(string content_type="text/html") {
			write200();
			writeType(content_type);
			writeClose();
		}

		public void writeFailure() {
			WriteToClient("HTTP/1.0 404 File not found");
			writeClose();
		}

		public void writeSuccessOmitMIME () {
			write200();
			writeClose();
		}
		public void WriteToClient (string data, string Enc) {WriteToClient(data, Encoding.GetEncoding(Enc));}
		public void WriteToClient (string data) {WriteToClient(data, Encoding.UTF8);}
		public void WriteToClient (string data, Encoding Enc) {
			WriteToClient(Enc.GetBytes(data+"\n"));
		}
		public void WriteToClient (byte[] data) {
			outputBinary.Write(data);
		}

		public static Dictionary<string,string> ProcessPOST (string POSTstring) {
			Dictionary<string,string> R = new Dictionary<string,string>();
			foreach (string i in POSTstring.Split('&')) {
				string[] P = i.Split('=');
				R.Add(P[0], P[1]);
			}
			return R;
		}
	}

	public class FileProcessor {
		//extension //MIME
		public static Dictionary<string, string> defaultfiletypes = new Dictionary<string, string> {
			//{"", ""},
			{".gif", "image/gif"},
			{".htm", "text/html"},
			{".html", "text/html"},
			{".jpeg", "image/jpeg"},
			{".jpg", "image/jpeg"},
			{".png", "image/png"},
			{".txt", "text/plain"},
		};
		public Dictionary<string, string> filetypes;
		private readonly HTTPProcessor HTTP;
		public readonly string filename;
		public readonly string filepath;
		public readonly string totalpath;
		public FileProcessor(HTTPProcessor sp) : this(sp, Environment.CurrentDirectory, FileProcessor.defaultfiletypes) {}
		public FileProcessor(HTTPProcessor sp, Dictionary<string,string> types) : this(sp, Environment.CurrentDirectory, types) {}
		public FileProcessor(HTTPProcessor sp, string startingdirectory) : this(sp, startingdirectory, FileProcessor.defaultfiletypes) {}
		public FileProcessor(HTTPProcessor sp, string startingdirectory, Dictionary<string,string> types) {
			this.filetypes = types;
			this.HTTP = sp;
			string urlinfo = sp.http_url;
			string[] divurl = urlinfo.Split('/');
			this.filename = divurl[divurl.Length-1];
			this.filepath = startingdirectory;
			if (divurl.Length > 2) {
				string[] splitfilepath = new string[divurl.Length - 1];
				splitfilepath[0] = startingdirectory;
				for (int i=1;i<divurl.Length-1;i++) {splitfilepath[i] = divurl[i];}
				this.filepath = Path.Combine(splitfilepath);
			}
			this.totalpath = Path.Combine(filepath, filename);
		}
		public bool Process () {
			if (filename != "" && File.Exists(totalpath)) {
				foreach (KeyValuePair<string, string> KVP in this.filetypes) {
					if (filename.ToLower().EndsWith(KVP.Key)) {
						HTTP.writeSuccess(KVP.Value);
						byte[] filedata = File.ReadAllBytes(totalpath);
						HTTP.WriteToClient(filedata);
						return true;
					}
				}
				HTTP.writeSuccessOmitMIME();
				HTTP.WriteToClient(File.ReadAllBytes(totalpath));
			}
			return false;
		}
	}
}

