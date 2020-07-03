using System;

namespace ArchiveLoader.Dto {
    class ArchiveLoaderPreferences {
        public  string      SourceDrive { get; set; }
        public  string      SourceDirectory { get; set; }
        public  string      TargetDirectory { get; set; }
        public  string      CatalogDirectory { get; set; }
        public  string      ReportDirectory { get; set; }

        public ArchiveLoaderPreferences() {
            SourceDrive = string.Empty;
            SourceDirectory = string.Empty;
            TargetDirectory = string.Empty;
            CatalogDirectory = string.Empty;
            ReportDirectory = string.Empty;
        }
    }
}
