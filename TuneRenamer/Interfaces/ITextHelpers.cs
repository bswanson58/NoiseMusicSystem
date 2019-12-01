using System.Collections.Generic;

namespace TuneRenamer.Interfaces {
    public interface ITextHelpers {
        IEnumerable<string> Lines( string text );
        int                 LineCount( string text );

        string              CleanText( string text, int defaultIndex );
        string              DeleteText( string source, string textToDelete );
        string              DeleteText( string source, char startCharacter, char endCharacter );

        string              SetExtension( string fileName, string proposedName );
        IEnumerable<string> GetCommonSubstring( string text, int returnCount );
    }
}
