using System.Text.RegularExpressions;

// from: https://stackoverflow.com/questions/6275980/string-replace-ignoring-case

namespace TuneRenamer.Platform {
    public static class StringEx {
        public static string ReplaceWord( this string str, string from, string to, RegexOptions options = RegexOptions.None ) {
            // 'from' must have word boundaries.
            return Regex.Replace( str, $"\\b{Regex.Escape( from )}\\b", to.Replace( "$", "$$" ), options );
        }        
    }
}
