using System;

namespace ArchiveLoader.Dto {
    class ArchiveLoaderPreferences {
        public  string      TargetDirectory { get; set; }

        public ArchiveLoaderPreferences() {
            TargetDirectory = String.Empty;;
        }
    }
}
