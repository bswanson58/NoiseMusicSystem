using System.Collections.Generic;
using System.Linq;

// from: https://stackoverflow.com/questions/21797599/how-can-i-find-lcs-length-between-two-large-strings/21797687

namespace TuneRenamer.Platform {
    static class StringArrayEx {
        public static IEnumerable<string> GetLongestCommonSubstring( this string[] strings, int returnCount ) {
            var commonSubstrings = new HashSet<string>( strings[0].GetSubstrings());

            foreach( string str in strings.Skip( 1 )) {
                commonSubstrings.IntersectWith( str.GetSubstrings());

                if( commonSubstrings.Count == 0 ) {
                    return new List<string>();
                }
            }

            return commonSubstrings.OrderByDescending( s => s.Length ).DefaultIfEmpty( string.Empty ).Take( returnCount );
        }

        private static IEnumerable<string> GetSubstrings( this string str ) {
            for( int c = 0; c < str.Length - 1; c++ ) {
                for( int cc = 1; c + cc <= str.Length; cc++ ) {
                    yield return str.Substring( c, cc );
                }
            }
        }
    }
}
