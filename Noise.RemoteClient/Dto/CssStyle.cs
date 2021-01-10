using System;
using System.Collections.Generic;

namespace Noise.RemoteClient.Dto {
    class CssStyle {
        public  String  Name;
        public  String  Category;
        public  String  Css;
        public  String  Size;

        public CssStyle() {
            Name = String.Empty;
            Category = String.Empty;
            Css = String.Empty;
            Size = String.Empty;
        }
    }

    class CssStyleFile {
        public  List<CssStyle>  Styles;

        public CssStyleFile() {
            Styles = new List<CssStyle>();
        }
    }
}
