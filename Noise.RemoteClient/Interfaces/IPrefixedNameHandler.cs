namespace Noise.RemoteClient.Interfaces {
    interface IPrefixedNameHandler {
        bool    ArePrefixesEnabled { get; }

        string  FormatPrefixedName( string name );
        string  FormatSortingName( string name );
    }
}
