using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Authentication;
using System.Text;
using System.Threading ;

using WebFront;

namespace WebBack
{
	public interface IServer {void Start(); void HandleGET(HTTPProcessor sp); void HandlePOST(HTTPProcessor sp, StreamReader sr);}

	public static class StringRandom {
        public const string Num = "1234567890";
		public const string NumLet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
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

    public static class URLOperations {
        public static string Decode(string URL) {
            URL = URL.Replace("%20", " ");
            return URL;
        }
        public static string Encode(string URL) {
            URL = URL.Replace(" ", "%20");
            return URL;
        }
    }

	public class HTTPProcessor {
		public TcpClient socket;        
		public IServer srv;
		
		private Stream inputStream;
		public StreamWriter outputStream;
		public BinaryWriter outputBinary;
		public BufferedStream outputCore;
		public SslStream outputSecure;
		
		public String http_method;
		public String http_url;
		public String http_protocol_versionstring;
		public string http_host;
		public IPAddress clientip;
		public SCookie clientcookie;
		public Hashtable httpHeaders = new Hashtable();

		public bool secure;
		
		
		private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB
		
		public HTTPProcessor(TcpClient s, IServer srv, bool secure = false) {
			this.clientip = ((IPEndPoint)s.Client.RemoteEndPoint).Address;
			this.socket = s;
			this.srv = srv;
			this.secure = secure;
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
            try {
                inputStream = new BufferedStream(socket.GetStream());
                outputCore = new BufferedStream(socket.GetStream());
                outputStream = new StreamWriter(outputCore);
                outputBinary = new BinaryWriter(outputCore);
				if (this.secure) {
					Console.WriteLine("Secure requested.");
					outputSecure = new SslStream(socket.GetStream(), false);
					Console.WriteLine("Attempting to authenticate.");
					outputSecure.AuthenticateAsServer(System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromCertFile("snsys.us.cert"), false, SslProtocols.Tls, false);
					Console.WriteLine("Authenticated.");
				}
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
				outputBinary.Flush();
				outputStream.Flush();
				outputCore.Flush();
				inputStream = null; outputBinary = null; outputStream = null; outputCore = null;      
                socket.Close();
            } catch (Exception e){
                Console.WriteLine("-----CORE CONNECTION ERROR BEGINS-----");
				Console.WriteLine(e);
				Console.WriteLine("-----CORE CONNECTION ERROR ENDS-----");
            }
		}
		
		public void parseRequest() {
            try {
				string request = this.secure?streamReadLine(outputSecure):streamReadLine(inputStream);
                string[] tokens = request.Split(' ');
                if (tokens.Length != 3) {
                    throw new Exception("invalid http request line");
                }
                http_method = tokens[0].ToUpper();
                http_url = URLOperations.Decode(tokens[1]);
                http_protocol_versionstring = tokens[2];
            } catch (Exception e){
                Console.WriteLine ("A parsing error occured:");
				Console.WriteLine (e);
            }
		}
		
		public void readHeaders() {
            try {
                string line;
				while ((line = this.secure?streamReadLine(outputSecure):streamReadLine(inputStream)) != null) {
					Console.WriteLine(line);
                    if (line.Equals("")) {
                        return;
                    }
                    if (line.StartsWith("Host:")) {
                        http_host = line.Substring(6);
                    }
                    if (line.StartsWith("Cookie:")) {
                        string[] t = line.Substring(8).Split('=');
                        clientcookie = new SCookie(t[0], t[1]);
                    }
                    int separator = line.IndexOf(':');
                    if (separator == -1) {
                        throw new Exception("invalid http header line: " + line);
                    }
                    string name = line.Substring(0, separator);
                    int pos = separator + 1;
                    while ((pos < line.Length) && (line[pos] == ' ')) {
                        pos++; // strip any spaces
                    }

                    string value = line.Substring(pos, line.Length - pos);
                    httpHeaders[name] = value;
                }
            } catch (Exception e){
                Console.WriteLine("A header reading error occured:");
				Console.WriteLine (e);
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
					
					int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
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
			srv.HandlePOST(this, new StreamReader(ms));
			
		}

		public void write200() {
			WriteToClient("HTTP/1.0 200 OK"); 
		}
		public void writeType(string content_type="text/html") {
			WriteToClient("Content-Type: " + content_type);
		}
		public void writeCookie(SCookie c) {
			WriteToClient(String.Format("Set-Cookie: {0}", c.ToString())); 
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

		public static Dictionary<string,Dictionary<string,string>> ProcessPOST (string POSTstring) {
			Dictionary<string,Dictionary<string,string>> typeDictionary = new Dictionary<string,Dictionary<string,string>>();
			string curtype = "UNSET";
			foreach (string i in POSTstring.Split('&')) {
				string[] P = i.Split('=');
				if (P[0] == "POSTType") {
					curtype = P[1];
					continue;
				}
				if (!typeDictionary.ContainsKey(curtype)){typeDictionary.Add(curtype, new Dictionary<string, string>());}
				typeDictionary[curtype].Add(P[0],P[1]);
			}
			return typeDictionary;
		}
	}

	public class FileProcessor {
		//extension //MIME
		public static Dictionary<string, string> defaultfiletypes = new Dictionary<string, string> {
			//{"", ""},
			{".css", "text/css"},
			{".gif", "image/gif"},
			{".htm", "text/html"},
			{".html", "text/html"},
			{".ico", "image/x-icon"},
			{".jpeg", "image/jpeg"},
			{".jpg", "image/jpeg"},
			{".js", "application/x-javascript"},
			{".png", "image/png"},
			{".txt", "text/plain"},
			{".zip", "application/zip"},
		};
		public Dictionary<string, string> filetypes;
		private readonly HTTPProcessor HTTP;
		public readonly string filename;
		public readonly string filepath;
		public readonly string totalpath;
		public List<string> processMethodStrings;
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
		public bool Process (METHOD M, bool caseSensitive = true, bool sendunknowntypes = false) {
			if (M != METHOD.GREY && processMethodStrings == null) {throw new NullReferenceException();}
			if (filename != "" && File.Exists(totalpath) && ListCheck(M, caseSensitive)) {
				foreach (KeyValuePair<string, string> KVP in this.filetypes) {
					if (filename.ToLower().EndsWith(KVP.Key)) {
						HTTP.writeSuccess(KVP.Value);
						byte[] filedata = File.ReadAllBytes(totalpath);
						HTTP.WriteToClient(filedata);
						return true;
					}
				}
				if (sendunknowntypes) {
					HTTP.writeSuccessOmitMIME();
					HTTP.WriteToClient(File.ReadAllBytes(totalpath));
					return true;
				}
			}
			return false;
		}
		public enum METHOD {WHITELIST_CONTAINS, WHITELIST_IS, WHITELIST_BEGINS, WHITELIST_ENDS, BLACKLIST_CONTAINS, BLACKLIST_IS, BLACKLIST_BEGINS, BLACKLIST_ENDS, GREY}
		private bool ListCheck (METHOD M, bool caseSensitive = true) {
			if (M == METHOD.BLACKLIST_BEGINS || M == METHOD.BLACKLIST_ENDS ||M == METHOD.BLACKLIST_CONTAINS ||M == METHOD.BLACKLIST_IS) {
				switch (M) {
				case METHOD.BLACKLIST_BEGINS:
                    foreach (string s in processMethodStrings) { if (caseSensitive ? filename.StartsWith(s) : filename.ToLower().StartsWith(s.ToLower())) { return false; } continue; }
					break;
				case METHOD.BLACKLIST_ENDS:
                    foreach (string s in processMethodStrings) { if (caseSensitive ? filename.EndsWith(s) : filename.ToLower().EndsWith(s.ToLower())) { return false; } continue; }
					break;
				case METHOD.BLACKLIST_CONTAINS:
                    foreach (string s in processMethodStrings) { if (caseSensitive ? filename.Contains(s) : filename.ToLower().Contains(s.ToLower())) { return false; } continue; }
					break;
				case METHOD.BLACKLIST_IS:
                    foreach (string s in processMethodStrings) { if (caseSensitive ? filename == s : filename.ToLower() == s.ToLower()) { return false; } continue; }
					break;
				}
				return true;
			} else if (M == METHOD.WHITELIST_BEGINS || M == METHOD.WHITELIST_ENDS ||M == METHOD.WHITELIST_CONTAINS ||M == METHOD.WHITELIST_IS) { 
				switch (M) {
				case METHOD.WHITELIST_BEGINS:
                        foreach (string s in processMethodStrings) { if (caseSensitive ? filename.StartsWith(s) : filename.ToLower().StartsWith(s.ToLower())) { return true; } continue; }
					break;
				case METHOD.WHITELIST_ENDS:
                    foreach (string s in processMethodStrings) { if (caseSensitive ? filename.EndsWith(s) : filename.ToLower().EndsWith(s.ToLower())) { return true; } continue; }
					break;
				case METHOD.WHITELIST_CONTAINS:
                    foreach (string s in processMethodStrings) { if (caseSensitive ? filename.Contains(s) : filename.ToLower().Contains(s.ToLower())) { return true; } continue; }
					break;
				case METHOD.WHITELIST_IS:
                    foreach (string s in processMethodStrings) { if (caseSensitive ? filename == s : filename.ToLower() == s.ToLower()) { return true; } continue; }
					break;
				}
				return false;
			} else {
				return true;
			}
		}
	}
}

