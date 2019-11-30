using System.Collections.Generic;

namespace TuneRenamer.Interfaces {
    public interface ITextHelpers {
        IEnumerable<string> Lines( string text );
        int                 LineCount( string text );

        string              CleanText( string text, int defaultIndex );
        string              GetCommonSubstring( string text );
    }
}
