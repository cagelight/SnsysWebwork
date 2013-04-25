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
		public GenericElement Action (string val) {return Attribute("action", val);}
		public GenericElement Class (string val) {return Attribute("class", val);}
		public GenericElement ID (string val) {return Attribute("id", val);}
		public GenericElement Method (string val) {return Attribute("method", val);}
		public GenericElement Name (string val) {return Attribute("name", val);}
		public GenericElement Type (string val) {return Attribute("type", val);}
		public GenericElement Value (string val) {return Attribute("value", val);}
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
	public class GenericSingleElement : GenericElement, IElement {
		public GenericSingleElement(string type) : base(type, new TextElement("")) {}
		public override string ToString (){
			string a = "";
			foreach (KeyValuePair<string, string> KVP in attributes) {
				a += String.Format(" {0}=\"{1}\"", KVP.Key, KVP.Value);
			}
			return String.Format("<{0}{1}>", type, a);
		}
	}
	//HELPER CLASS
	public static class HTML {
		public static GenericElement Body (params IElement[] contents) {
			return new GenericElement("body", contents);
		}
		public static GenericSingleElement Breakline () {
			return new GenericSingleElement("br");
		}
		public static GenericElement Form (params IElement[] contents) {
			return new GenericElement("form", contents);
		}
		public static GenericElement Head (params IElement[] contents) {
			return new GenericElement("head", contents);
		}
		public static GenericSingleElement Input () {
			return new GenericSingleElement("input");
		}
		public static GenericElement Span (TextElement contents) {
			return new GenericElement("span", contents);
		}
		public static GenericElement Title (TextElement contents) {
			return new GenericElement("h1", contents);
		}
	}
}

