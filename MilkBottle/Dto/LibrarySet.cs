using System;
using System.Collections.Generic;

namespace MilkBottle.Dto {
    class LibrarySet {
        public  string                  LibraryName { get; set; }
        public  int                     PresetCount => Presets.Count;
        public  List<MilkDropPreset>    Presets { get; set; }

        public LibrarySet() :
            this( String.Empty ) {
        }

        public LibrarySet( string libraryName ) {
            LibraryName = libraryName;

            Presets = new List<MilkDropPreset>();
        }
    }
}
