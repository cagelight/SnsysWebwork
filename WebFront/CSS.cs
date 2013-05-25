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
		//Enums and Dictionaries
        public enum POSITION_X { LEFT, CENTER, RIGHT };
        public enum POSITION_Y { TOP, CENTER, BOTTOM };
        private static Dictionary<POSITION_X, string> POSITION_XD = new Dictionary<POSITION_X, string>() {{POSITION_X.LEFT, "left"}, {POSITION_X.CENTER, "center"}, {POSITION_X.RIGHT, "right"}};
        private static Dictionary<POSITION_Y, string> POSITION_YD = new Dictionary<POSITION_Y, string>() { { POSITION_Y.TOP, "top" }, { POSITION_Y.CENTER, "center" }, { POSITION_Y.BOTTOM, "bottom" } };
		public enum REPEAT {X, Y, XY, NONE, INHERIT};
		private static Dictionary<REPEAT, string> REPEATD = new Dictionary<REPEAT, string>() {{REPEAT.X,"repeat-x"},{REPEAT.Y,"repeat-y"},{REPEAT.XY,"repeat"},{REPEAT.NONE,"no-repeat"},{REPEAT.INHERIT,"repeat-inherit"},};
		
        //PRESETS

        //Background
		public static Style BackgroundColor (string hexcolor) { return new Style("background-color", hexcolor[0]=='#'?hexcolor:"#"+hexcolor); }
		public static Style BackgroundColor (HexColor hexcolor) { return new Style("background-color", hexcolor.ToString()); }
		public static Style BackgroundImage (string filename) { return new Style("background-image", String.Format("url(\'{0}\')", filename)); }
        public static Style BackgroundPosition(POSITION_X P) { return new Style("background-position", Style.POSITION_XD[P]); } 
        public static Style BackgroundPosition(POSITION_Y P) { return new Style("background-position", Style.POSITION_YD[P]); }
        public static Style BackgroundPosition(POSITION_X PX, POSITION_Y PY) { return new Style("background-position", String.Format("{0} {1}", Style.POSITION_XD[PX], Style.POSITION_YD[PY])); }
        /// <summary> This overload takes two floats, which will be treated as percentages. 0.1 = 10%, 0.75 = 75%, etc. Float values greater than 1.0 are not recommended. </summary>
        public static Style BackgroundPosition(float X, float Y) { return new Style("background-position", String.Format("{0}% {1}%", X, Y)); }
        /// <summary> This overload takes two integers, which will be treated as pixel coordinates. 2 = 2px, 43 = 43px, etc. </summary>
        public static Style BackgroundPosition(int X, int Y) { return new Style("background-position", String.Format("{0}px {1}px", X, Y)); }
        /// <summary> This is the default method, it will use "inherit". </summary>
        public static Style BackgroundPosition() { return new Style("background-position", "inherit"); }
		public static Style BackgroundRepeat (REPEAT R) {return new Style("background-repeat", Style.REPEATD[R]);}
	}

	public struct HexColor {
		byte R; byte G; byte B;
		public HexColor (byte R, byte G, byte B) {
			this.R = R; this.G = G; this.B = B; }
		public override string ToString () {
			return string.Format ("#{0}{1}{2}", this.R.ToString("X2"), this.G.ToString("X2"), this.B.ToString("X2")); }
		public static HexColor Red = new HexColor(255,0,0);
	}
}