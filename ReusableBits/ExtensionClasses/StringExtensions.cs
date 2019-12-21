using System.Text;

namespace ReusableBits.ExtensionClasses {
    public static class StringExtensions {
        public static string RemoveSpecialCharacters( this string str ) {
            StringBuilder sb = new StringBuilder();

            foreach( char c in str ) {
                if(( c >= '0' && c <= '9' ) ||
                   ( c >= 'A' && c <= 'Z' ) || 
                   ( c >= 'a' && c <= 'z' )) {
                    sb.Append( c );
                }
            }
            return sb.ToString();
        }
    }
}
