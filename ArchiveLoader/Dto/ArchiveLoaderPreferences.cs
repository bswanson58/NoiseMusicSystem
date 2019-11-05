using System;

namespace ArchiveLoader.Dto {
    class ArchiveLoaderPreferences {
        public  string      SourceDrive { get; set; }
        public  string      SourceDirectory { get; set; }
        public  string      TargetDirectory { get; set; }

        public ArchiveLoaderPreferences() {
            SourceDrive = String.Empty;
            SourceDirectory = String.Empty;
            TargetDirectory = String.Empty;
        }
    }
}
