using System;

namespace TuneArchiver.Models {
    public class ArchiverPreferences {
        public  string      ArchiveRootPath { get; set; }
        public  string      StagingDirectory { get; set; }

        public ArchiverPreferences() {
            ArchiveRootPath = String.Empty;
            StagingDirectory = String.Empty;
        }
    }
}
