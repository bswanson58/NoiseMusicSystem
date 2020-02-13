using System.Collections.Generic;
using MilkBottle.Dto;

namespace MilkBottle.Interfaces {
    interface IPresetLibrarian {
        IEnumerable<string>         AvailableLibraries { get; }
        LibrarySet                  GetLibrary( string libraryName );
    }
}
