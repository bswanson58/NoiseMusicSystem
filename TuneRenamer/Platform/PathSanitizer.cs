using System;
using System.Text;

// from: https://stackoverflow.com/questions/62771/how-do-i-check-if-a-given-string-is-a-legal-valid-file-name-under-windows/62888

namespace TuneRenamer.Platform {
    /// <summary>
    /// Cleans paths of invalid characters.
    /// </summary>
    public static class PathSanitizer {
        /// <summary>
        /// The set of invalid filename characters, kept sorted for fast binary search
        /// </summary>
        private static readonly char[] mInvalidFilenameChars;
        /// <summary>
        /// The set of invalid path characters, kept sorted for fast binary search
        /// </summary>
        private static readonly char[] mInvalidPathChars;

        static PathSanitizer() {
            // set up the two arrays -- sorted once for speed.
            mInvalidFilenameChars = System.IO.Path.GetInvalidFileNameChars();
            mInvalidPathChars = System.IO.Path.GetInvalidPathChars();
            Array.Sort(mInvalidFilenameChars);
            Array.Sort(mInvalidPathChars);

        }

        /// <summary>
        /// Cleans a filename of invalid characters
        /// </summary>
        /// <param name="input">the string to clean</param>
        /// <param name="errorChar">the character which replaces bad characters</param>
        /// <returns></returns>
        public static string SanitizeFilename(string input, char errorChar) {
            return Sanitize(input, mInvalidFilenameChars, errorChar);
        }

        /// <summary>
        /// Cleans a path of invalid characters
        /// </summary>
        /// <param name="input">the string to clean</param>
        /// <param name="errorChar">the character which replaces bad characters</param>
        /// <returns></returns>
        public static string SanitizePath(string input, char errorChar) {
            return Sanitize(input, mInvalidPathChars, errorChar);
        }

        /// <summary>
        /// Cleans a string of invalid characters.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="invalidChars"></param>
        /// <param name="errorChar"></param>
        /// <returns></returns>
        private static string Sanitize(string input, char[] invalidChars, char errorChar) {
            // null always sanitizes to null
            if (input == null) { return null; }
            StringBuilder result = new StringBuilder();
            foreach (var characterToTest in input) {
                // we binary search for the character in the invalid set. This should be lightning fast.
                if (Array.BinarySearch(invalidChars, characterToTest) >= 0) {
                    // we found the character in the array of 
                    result.Append(errorChar);
                }
                else {
                    // the character was not found in invalid, so it is valid.
                    result.Append(characterToTest);
                }
            }

            // we're done.
            return result.ToString();
        }
    }
}
