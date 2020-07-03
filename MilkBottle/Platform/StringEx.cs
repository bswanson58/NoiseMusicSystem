using System.Text.RegularExpressions;

// from: https://stackoverflow.com/questions/6275980/string-replace-ignoring-case

namespace MilkBottle.Platform {
    public static class StringEx {
        public static string ReplaceWord( this string str, string from, string to, RegexOptions options = RegexOptions.IgnoreCase ) {
            return Regex.Replace( str, Regex.Escape( from ), to.Replace( "$","$$" ), options );
        }
    }
}
