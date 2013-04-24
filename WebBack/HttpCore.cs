using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading ;

namespace WebBack
{
	public interface IServer {void Start(); void Run(); void HandleGET(HTTPProcessor sp); void HandlePOST(HTTPProcessor sp, StreamReader sr);}

	public abstract class GenericServer : IServer {
		public ISite sitelogic; 
		public TcpListener tcpl; 
		public Thread process;
		public bool active = false;
		public GenericServer(ISite insitelogic, IPAddress addr, ushort port){
			sitelogic = insitelogic; 
			tcpl = new TcpListener(addr, port);
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
			sp.writeSuccess();
			sp.outputStream.WriteLine(sitelogic.Generate(sp.http_host + sp.http_url));
		}
		public virtual void HandlePOST (HTTPProcessor sp, StreamReader sr) {
			
		}
	}

	public class HTTPProcessor {
		public TcpClient socket;        
		public IServer srv;
		
		private Stream inputStream;
		public StreamWriter outputStream;
		
		public String http_method;
		public String http_url;
		public String http_protocol_versionstring;
		public string http_host;
		public Hashtable httpHeaders = new Hashtable();
		
		
		private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB
		
		public HTTPProcessor(TcpClient s, IServer srv) {
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
			outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));
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
				if (line.Substring(0,5) == "Host:") {
					http_host = line.Substring(5);
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
		
		public void writeSuccess(string content_type="text/html") {
			outputStream.WriteLine("HTTP/1.0 200 OK");            
			outputStream.WriteLine("Content-Type: " + content_type);
			outputStream.WriteLine("Connection: close");
			outputStream.WriteLine("");
		}

		public void writeFailure() {
			outputStream.WriteLine("HTTP/1.0 404 File not found");
			outputStream.WriteLine("Connection: close");
			outputStream.WriteLine("");
		}

		public void writeSuccessOmitMIME () {
			outputStream.WriteLine("HTTP/1.0 200 OK");            
			outputStream.WriteLine("Connection: close");
			outputStream.WriteLine("");
		}
	}

	public class FileProcessor {
		//extension //MIME
		public static Dictionary<string, string> defaultfiletypes = new Dictionary<string, string> {
			//{"", ""},
			{"htm", "html"},
			{"html", "html"},
			{"txt", "plain"},
		};
		public Dictionary<string, string> filetypes;
		private readonly HTTPProcessor HTTP;
		public readonly string filename;
		public readonly string filepath;
		public readonly string totalpath;
		public FileProcessor(HTTPProcessor sp, Dictionary<string,string> types = null) {
			if (types == null) {this.filetypes = FileProcessor.defaultfiletypes;} else {this.filetypes = types;}
			this.HTTP = sp;
			string urlinfo = this.HTTP.http_url;
			string[] divurl = urlinfo.Split('/');
			this.filename = divurl[divurl.Length-1];
			this.filepath = Environment.CurrentDirectory;
			if (divurl.Length > 2) {
				string[] splitfilepath = new string[divurl.Length - 1];
				splitfilepath[0] = Environment.CurrentDirectory;
				for (int i=1;i<divurl.Length-1;i++) {splitfilepath[i] = divurl[i];}
				this.filepath = Path.Combine(splitfilepath);
			}
			this.totalpath = Path.Combine(filepath, filename);
		}
		public bool Process () {
			if (filename != "" && File.Exists(totalpath)) {
				foreach (KeyValuePair<string, string> KVP in this.filetypes) {
					if (filename.EndsWith("."+KVP.Key)) {
						this.HTTP.writeSuccess(KVP.Value);
						this.HTTP.outputStream.WriteLine(Encoding.UTF8.GetString(File.ReadAllBytes(totalpath)));
						return true;
					}
				}
			}
			return false;
		}
	}
}

