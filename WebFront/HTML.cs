using System;
using System.Collections.Generic;

namespace WebFront
{
	//ELEMENTS
	public interface IElement {  }
	public struct TextElement : IElement {
		public string val;
		public TextElement (string val) {this.val = val;}
		public override string ToString() {return val;}
		public static implicit operator TextElement(string s) {return new TextElement(s);}
	}
	public class GenericElement : IElement {
		public readonly string type;
		public Dictionary<string, string> attributes = new Dictionary<string, string>();
		public List<IElement> contents = new List<IElement>();
		public GenericElement (string type, params IElement[] cIn) {
			this.type = type;
			foreach (IElement e in cIn) {
				this.contents.Add(e);
			}
		}
		public GenericElement Class (string val) {return Attribute("class", val);}
		public GenericElement ID (string val) {return Attribute("id", val);}
		public GenericElement Attribute (string name, string val) {
			if (!attributes.ContainsKey(name)) {
				attributes.Add(name, val);
			} else {
				attributes[name] = val;
			}
			return this;
		}
		public override string ToString (){
			string c = "";
			string a = "";
			foreach (IElement e in contents) {
				c += e.ToString();
			}
			foreach (KeyValuePair<string, string> KVP in attributes) {
				a += String.Format(" {0}=\"{1}\"", KVP.Key, KVP.Value);
			}
			return String.Format("<{0}{2}>{1}</{0}>", type, c, a);
		}
	}
	//HELPER CLASS
	public static class HTML {
		public static GenericElement Body (params IElement[] contents) {
			return new GenericElement("body", contents);
		}
		public static GenericElement Head (params IElement[] contents) {
			return new GenericElement("head", contents);
		}
		public static GenericElement Span (TextElement contents) {
			return new GenericElement("span", contents);
		}
		public static GenericElement Title (TextElement contents) {
			return new GenericElement("h1", contents);
		}
	}
}

