using System.Collections.Generic;
using System.Threading.Tasks;
using MilkBottle.Dto;

namespace MilkBottle.Interfaces {
    interface IPresetLibrarian {
        Task<bool>                  Initialize();
        bool                        IsInitialized { get; }
        bool                        ContainsLibrary( string libraryName );

        IEnumerable<string>         AvailableLibraries { get; }
        IEnumerable<LibrarySet>     PresetLibraries { get; }

        LibrarySet                  GetLibrary( string libraryName );
    }
}
