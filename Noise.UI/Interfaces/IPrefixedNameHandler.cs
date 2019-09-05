namespace Noise.UI.Interfaces {
    public interface IPrefixedNameHandler {
        bool    ArePrefixesEnabled { get; }

        string  FormatPrefixedName( string name );
        string  FormatSortingName( string name );
    }
}
