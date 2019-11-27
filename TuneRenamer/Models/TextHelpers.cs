using System;
using System.Collections.Generic;
using System.Linq;
using TuneRenamer.Interfaces;

namespace TuneRenamer.Models {
    class TextHelpers : ITextHelpers {
        public string GetCommonSubstring( string text ) {
            var strings = text.Split( new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None );

            return strings.GetLongestCommonSubstring();
        }
    }

    // from: https://stackoverflow.com/questions/21797599/how-can-i-find-lcs-length-between-two-large-strings/21797687
    static class StringArrayEx {
        public static string GetLongestCommonSubstring( this string[] strings ) {
            var commonSubstrings = new HashSet<string>(strings[0].GetSubstrings());

            foreach ( string str in strings.Skip( 1 ) ) {
                commonSubstrings.IntersectWith( str.GetSubstrings());
                if ( commonSubstrings.Count == 0 )
                    return string.Empty;
            }

            return commonSubstrings.OrderByDescending( s => s.Length ).DefaultIfEmpty( string.Empty ).First();
        }

        private static IEnumerable<string> GetSubstrings( this string str ) {
            for ( int c = 0; c < str.Length - 1; c++ ) {
                for ( int cc = 1; c + cc <= str.Length; cc++ ) {
                    yield return str.Substring( c, cc );
                }
            }
        }
    }
}
