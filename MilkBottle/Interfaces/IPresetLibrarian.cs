using System;
using System.Collections.Generic;
using MilkBottle.Dto;

namespace MilkBottle.Interfaces {
    interface IPresetLibrarian {
        bool                        IsInitialized { get; }

        IEnumerable<string>         AvailableLibraries { get; }
        LibrarySet                  GetLibrary( string libraryName );
    }
}
