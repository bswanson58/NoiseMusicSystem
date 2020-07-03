using System.Collections.Generic;

namespace TuneRenamer.Interfaces {
    public interface ITextHelpers {
        IEnumerable<string> Lines( string text );
        int                 LineCount( string text );

        string              BasicCleanText( string text, int defaultIndex );
        string              ExtendedCleanText( string text, int defaultIndex );

        string              DeleteText( string source, string textToDelete );
        string              DeleteText( string source, char startCharacter, char endCharacter );
        string              RemoveTrailingDigits( string source );

        string              SetExtension( string fileName, string proposedName );
        string              RenumberIndex( string fileName, int newIndex );
        IEnumerable<string> GetCommonSubstring( string text, int returnCount );
    }
}
