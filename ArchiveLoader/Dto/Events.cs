namespace ArchiveLoader.Dto {
    public class Events {
        public class JobTargets {
            public string   SourceVolumeName { get; }
            public string   SourceDirectory { get; }
            public string   TargetDirectory { get; }

            public JobTargets( string sourceVolumeName, string source, string target ) {
                SourceVolumeName = sourceVolumeName;
                SourceDirectory = source;
                TargetDirectory = target;
            }
        }

        public class ProcessItemEvent {
            public CopyProcessEventReason   Reason { get; }
            public ProcessItem              Item { get; }

            public ProcessItemEvent( ProcessItem item, CopyProcessEventReason reason ) {
                Reason = reason;
                Item = item;
            }
        }

        public class StatusEvent {
            public	string			Message { get; }
            public	bool			ExtendDisplay { get; set; }

            public StatusEvent( string message ) {
                Message = message;
            }
        }

        public class VolumeStarted {
            public  string  VolumeName { get; }
            public  long    VolumeSize { get; }

            public VolumeStarted( string name, long size ) {
                VolumeName = name;
                VolumeSize = size;
            }
        }

        public class FileCopied {
            public  string  FileName { get; }
            public  long    FileSize { get; }

            public FileCopied( string fileName, long fileSize ) {
                FileName = fileName;
                FileSize = fileSize;
            }
        }

        public class VolumeCompleted {
            public  string  VolumeName { get; }

            public VolumeCompleted( string volumeName ) {
                VolumeName = volumeName;
            }
        }
    }
}
