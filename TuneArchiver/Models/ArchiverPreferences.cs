using System;

namespace TuneArchiver.Models {
    public class ArchiverPreferences {
        public  string      ArchiveRootPath { get; set; }
        public  string      StagingDirectory { get; set; }
        public  string      ArchiveLabelFormat { get; set; }
        public  string      ArchiveLabelIdentifier {  get; set; }
        public  string      ArchiveMediaType { get; set; }

        public ArchiverPreferences() {
            ArchiveRootPath = String.Empty;
            StagingDirectory = String.Empty;
            ArchiveLabelFormat = String.Empty;
            ArchiveLabelIdentifier = String.Empty;
            ArchiveMediaType = String.Empty;
        }
    }
}
