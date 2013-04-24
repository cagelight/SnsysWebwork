using System;
using System.Collections.Generic;

namespace WebFront
{
	public struct Style {
		public string key; public string val;
		public Style (string key, string val) {
			this.key = key; this.val = val; }
		public override string ToString (){
			return string.Format("{0}:{1};", this.key, this.val); }
	}
	public struct StyleSet {
		public Dictionary<string, string> styles;
		public StyleSet (params Style[] instyles) {
			styles = new Dictionary<string, string>();
			foreach (Style s in instyles) {
				styles.Add(s.key, s.val); } }
		public string this[string key] {
			get{if(styles.ContainsKey(key)){return styles[key];} else {return "";}}
			set{if(styles.ContainsKey(key)){styles[key] = value;} else {styles.Add(key, value);}}
		}
		public override string ToString (){
			string r = "";
			foreach (KeyValuePair<string, string> KVP in styles) {
				r += String.Format("{0}:{1};");
			}
			return r;
		}
	}
}