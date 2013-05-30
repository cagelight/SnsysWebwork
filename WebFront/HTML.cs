using System;
using System.Collections.Generic;

namespace WebFront {
    //ELEMENTS
    public interface IElement {
    }
    public struct TextElement : IElement {
        public string val;
        public TextElement(string val) { this.val = val; }
        public override string ToString() { return val; }
        public static implicit operator TextElement(string s) { return new TextElement(s); }
    }
    public class HTMLContent : IElement {
        public readonly string type;
        protected Dictionary<string, string> attributes = new Dictionary<string, string>();
        protected Dictionary<string, string> styles = new Dictionary<string, string>();
        protected List<IElement> contents = new List<IElement>();
        public HTMLContent(string type, params IElement[] cIn) {
            this.type = type;
            foreach (IElement e in cIn) {
                this.contents.Add(e);
            }
        }
        public static HTMLContent operator +(HTMLContent A, IElement B) { A.contents.Add(B); return A; }
        public HTMLContent Action(string val) { return Attribute("action", val); }
        public HTMLContent Class(string val) { return Attribute("class", val); }
        public HTMLContent Href(string val) { return Attribute("href", val); }
        public HTMLContent ID(string val) { return Attribute("id", val); }
        public HTMLContent Method(string val) { return Attribute("method", val); }
        public HTMLContent Name(string val) { return Attribute("name", val); }
        public HTMLContent Rel(string val) { return Attribute("rel", val); }
        public HTMLContent Src(string val) { return Attribute("src", val); }
        public HTMLContent Type(string val) { return Attribute("type", val); }
        public HTMLContent Value(string val) { return Attribute("value", val); }
        public HTMLContent Attribute(string name, string val) {
            if (!attributes.ContainsKey(name)) {
                attributes.Add(name, val);
            } else {
                attributes[name] = val;
            }
            return this;
        }
        public HTMLContent Style(params Style[] instyles) {
            foreach (Style S in instyles) {
                if (!styles.ContainsKey(S.key)) {
                    styles.Add(S.key, S.val);
                } else {
                    styles[S.key] = S.val;
                }
            }
            return this;
        }
        public override string ToString() {
            string c = "";
            string a = "";
            foreach (IElement e in contents) {
                if (e != null) { c += e.ToString(); }
            }
            foreach (KeyValuePair<string, string> KVP in attributes) {
                a += String.Format(" {0}=\"{1}\"", KVP.Key, KVP.Value);
            }
            if (styles.Count > 0) {
                string s = "";
                foreach (KeyValuePair<string, string> KVP in styles) {
                    s += String.Format("{0}:{1};", KVP.Key, KVP.Value);
                }
                a += String.Format(" style=\"{0}\"", s);
            }
            return String.Format("<{0}{2}>{1}</{0}>", type, c, a);
        }
    }
    public class HTMLEmpty : HTMLContent, IElement {
        public HTMLEmpty(string type) : base(type, new TextElement("")) { }
        public override string ToString() {
            string a = "";
            foreach (KeyValuePair<string, string> KVP in attributes) {
                a += String.Format(" {0}=\"{1}\"", KVP.Key, KVP.Value);
            }
            if (styles.Count > 0) {
                string s = "";
                foreach (KeyValuePair<string, string> KVP in styles) {
                    s += String.Format("{0}:{1};", KVP.Key, KVP.Value);
                }
                a += String.Format(" style=\"{0}\"", s);
            }
            return String.Format("<{0}{1}>", type, a);
        }
    }
    //HELPER CLASS
    public static class HTML {
        public struct Webpage {
            public HTMLContent Head;
            public HTMLContent Body;
            public Webpage(string title) {
                this.Head = HTML.Head();
                this.Body = HTML.Body();
                this.Head += HTML.Title(title);
            }
            public override string ToString() {
                return String.Format("<html>{0}{1}</html>", this.Head.ToString(), this.Body.ToString());
            }
        }
        public static HTMLContent Attribute(params IElement[] contents) { return new HTMLContent("a", contents); }
        public static HTMLContent Attribute(TextElement contents) { return new HTMLContent("a", contents); }
        public static HTMLContent Body(params IElement[] contents) { return new HTMLContent("body", contents); }
        public static HTMLEmpty Breakline { get { return new HTMLEmpty("br"); } }
        public static HTMLContent Div(params IElement[] contents) { return new HTMLContent("div", contents); }
        public static HTMLContent Form(params IElement[] contents) { return new HTMLContent("form", contents); }
        public static HTMLContent H1(TextElement contents) { return new HTMLContent("h1", contents); }
        public static HTMLContent H2(TextElement contents) { return new HTMLContent("h2", contents); }
        public static HTMLContent H3(TextElement contents) { return new HTMLContent("h3", contents); }
        public static HTMLContent Head(params IElement[] contents) { return new HTMLContent("head", contents); }
        public static HTMLEmpty Image() { return new HTMLEmpty("img"); }
        public static HTMLEmpty Input() { return new HTMLEmpty("input"); }
        public static HTMLEmpty Link() { return new HTMLEmpty("link"); }
        public static HTMLContent Script(TextElement contents) { return new HTMLContent("script", contents); }
        public static HTMLContent Span(TextElement contents) { return new HTMLContent("span", contents); }
        public static HTMLContent Table(params IElement[] contents) { return new HTMLContent("table", contents); }
        public static HTMLContent TBody(params IElement[] contents) { return new HTMLContent("tbody", contents); }
        public static HTMLContent TCell(params IElement[] contents) { return new HTMLContent("td", contents); }
        public static HTMLContent TRow(params IElement[] contents) { return new HTMLContent("tr", contents); }
        public static HTMLContent Title(TextElement contents) { return new HTMLContent("title", contents); }

        public static HTMLContent SimpleTable(ushort rowSize, params IElement[] contents) {
            HTMLContent Body = HTML.TBody();
            int rowNum = contents.Length / rowSize;
            int rowRemain = contents.Length % rowSize;
            for (int a = 0; a < rowNum; ++a) {
                HTMLContent Row = HTML.TRow();
                for (ushort r = 0; r < rowSize; ++r) {
                    int index = (a * rowSize) + r;
                    Row += HTML.TCell(contents[index]);
                }
                Body += Row;
            }
            if (rowRemain != 0) {
                HTMLContent Row = HTML.TRow();
                for (int r = 0; r < rowRemain; ++r) {
                    int index = (rowSize * rowNum) + r;
                    Row += HTML.TCell(contents[index]);
                }
                Body += Row;
            }
            return HTML.Table(Body);
        }
    }
}

