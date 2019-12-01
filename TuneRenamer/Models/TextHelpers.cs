using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TuneRenamer.Interfaces;
using TuneRenamer.Platform;

namespace TuneRenamer.Models {
    class TextHelpers : ITextHelpers {
        private readonly string[]       mNewLines = { "\r\n", "\r", "\n" };
        private readonly IFileTypes     mFileTypes;

        public TextHelpers( IFileTypes fileTypes ) {
            mFileTypes = fileTypes;
        }

        public IEnumerable<string> GetCommonSubstring( string text,int returnCount ) {
            var strings = Lines( text ).ToArray();

            return strings.GetLongestCommonSubstring( returnCount );
        }

        public IEnumerable<string> Lines( string text ) {
            var split = text.Split( mNewLines, StringSplitOptions.None );

            return from l in split where !String.IsNullOrWhiteSpace( l ) select l;
        }

        public int LineCount( string text ) {
            return Lines( text ).Count();
        }

        public string CleanText( string text, int defaultIndex ) {
            var extension = DetermineExtension( ref text ).ToLower( CultureInfo.CurrentCulture );
            var index = DetermineTrackIndex( ref text, defaultIndex );
            var name = ReplaceCharSets( text );

            name= CapitalizeText( name );
            name = PathSanitizer.SanitizeFilename( name, ' ' ).Trim();

            return $"{index:D2} - {name}{extension}";
        }

        public string DeleteText( string sourceText, string textToDelete ) {
            var retValue = sourceText;

            while( retValue.Contains( textToDelete )) {
                retValue = retValue.Replace( textToDelete, String.Empty );
            }

            return retValue;
        }

        public string DeleteText( string source, char startCharacter, char endCharacter ) {
            var     retValue = source;
            bool    textDeleted;

            do {
                var startPosition = retValue.IndexOf( startCharacter );
                var endPosition = retValue.IndexOf( endCharacter );

                if(( startPosition >= 0 ) &&
                   ( endPosition > startPosition )) {
                    retValue = retValue.Remove( startPosition, endPosition - startPosition + 1 );

                    textDeleted = true;
                }
                else {
                    textDeleted = false;
                }
            } while( textDeleted );

            return retValue;
        }

        public string RemoveTrailingDigits( string source ) {
            var fileName = source;
            var extension = DetermineExtension( ref fileName );

            fileName = fileName.TrimEnd( '0', '1', '2', '3', '4', '4', '5', '6', '7', '8', '9', ' ', ':' );

            return $"{fileName}{extension}";
        }

        public string SetExtension( string fileName, string proposedName ) {
            var ext = DetermineExtension( ref fileName );

            DetermineExtension( ref proposedName );

            return  proposedName + ext;
        }

        private string DetermineExtension( ref string text ) {
            var retValue = String.Empty;

            if((!String.IsNullOrWhiteSpace( text )) &&
               ( mFileTypes.ItemIsMusicFile( PathSanitizer.SanitizeFilename( text, ' ' )))) {
                var lastPeriod = text.LastIndexOf( ".", StringComparison.CurrentCulture );

                if(( lastPeriod > 0 ) &&
                  (( text.Length - lastPeriod ) < 6 )) {
                    retValue = text.Substring( lastPeriod, text.Length - lastPeriod );
                    text  = text.Substring( 0, lastPeriod );
                }
            }

            return retValue;
        }

        private int DetermineTrackIndex( ref string text, int defaultIndex ) {
            var retValue = defaultIndex;
            var pattern = new Regex( @"[ ([]*(?<index>[0-9]+)" );
            var match = pattern.Match( text );

            if( match.Success ) {
                var indexStr = match.Groups["index"];

                if( Int16.TryParse( indexStr.Value, out var index )) {
                    retValue = index;

                    // clean up the returned string.
                    var startIndex = text.IndexOf( indexStr.Value, StringComparison.CurrentCulture );
                    text = text.Remove( 0, startIndex + indexStr.Length );
                    text = text.TrimStart( ' ', '"', '-', '.', ')', ']' );
                }
            }

            return retValue;
        }

        private string ReplaceCharSets( string text ) {
            var retValue = text;

            retValue = retValue.Replace( "_",   " " );
            retValue = retValue.Replace( " > ", " - " );
            retValue = retValue.Replace( " >",  " - " );
            retValue = retValue.Replace( "> ",  " - " );
            retValue = retValue.Replace( ">",   " - " );
            retValue = retValue.Replace( "-",   " - " );
            retValue = retValue.Replace( " -",  " - " );
            retValue = retValue.Replace( "- ",  " - " );

            while( retValue.IndexOf( "  ", StringComparison.CurrentCulture ) >= 0 ) {
                retValue = retValue.Replace( "  ",  " " );
            }

            retValue = retValue.Trim( ' ', '-' );

            return retValue;
        }

        private string CapitalizeText( string text ) {
            var	toUpper = true;
            var whiteSpace = new [] { ' ', '-', '_', '(', ')', '[', ']', '>', '.' };
            var	newString = new StringBuilder( text.Length );
			
            for( int index = 0; index < text.Length; index++ ) {
                if( toUpper ) {
                    newString.Append( Char.ToUpper( text[index]));
					
                    toUpper = false;
                }
                else {
                    newString.Append( Char.ToLower( text[index]));
                }
				
                if( text.IndexOfAny( whiteSpace, index, 1 ) == index ) {
                    toUpper = true;
                }
            }				
		
            return newString.ToString();
        }
    }

    // from: https://stackoverflow.com/questions/21797599/how-can-i-find-lcs-length-between-two-large-strings/21797687
    static class StringArrayEx {
        public static IEnumerable<string> GetLongestCommonSubstring( this string[] strings, int returnCount ) {
            var commonSubstrings = new HashSet<string>(strings[0].GetSubstrings());

            foreach ( string str in strings.Skip( 1 ) ) {
                commonSubstrings.IntersectWith( str.GetSubstrings());

                if ( commonSubstrings.Count == 0 ) {
                    return new List<string>();
                }
            }

            return commonSubstrings.OrderByDescending( s => s.Length ).DefaultIfEmpty( string.Empty ).Take( returnCount );
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
