using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArchiveLoader.Dto {
    public class CompletedProcessItem {
        public  string                      Name { get; }
        public  string                      Artist { get; }
        public  string                      Album { get; }
        public  string                      Subdirectory { get; }
        public  string                      FileName { get; }
        public  string                      VolumeName { get; }
        public  List<DisplayedStatusItem>   ProcessList { get; }
        public  string                      ProcessNames { get; }
        public  string                      Output { get; }
        public  ProcessState                FinalState { get; }
        public  int                         Errors { get; }

        public CompletedProcessItem( ProcessItem fromItem ) {
            Name = fromItem.Name;
            FileName = fromItem.FileName;
            VolumeName = fromItem.VolumeName;
            Artist = fromItem.ArtistFolder;
            Album = fromItem.AlbumFolder;
            Subdirectory = fromItem.Subdirectory;

            if( fromItem.ProcessList.Any()) {
                ProcessNames = fromItem.ProcessList.Count > 1 ? 
                                    String.Join( ", ", from handler in fromItem.ProcessList select handler.Handler.HandlerName ) : 
                                    fromItem.ProcessList[0].Handler.HandlerName;

                ProcessList = new List<DisplayedStatusItem>( from p in fromItem.ProcessList select new DisplayedStatusItem( p.Handler.HandlerName, p.InitialProcessState ));
                ProcessList.LastOrDefault()?.SetLastItem( true );
            }

            FinalState = fromItem.ProcessList.LastOrDefault()?.ProcessState ?? ProcessState.Completed;

            var output = new StringBuilder();

            foreach( var handler in fromItem.ProcessList ) {
                if(!String.IsNullOrWhiteSpace( handler.ProcessStdOut)) {
                    output.Append( $"{handler.Handler.HandlerName} - Std Out: " );
                    output.AppendLine( handler.ProcessStdOut );
                }
                if(!String.IsNullOrWhiteSpace( handler.ProcessErrOut )) {
                    output.Append( $"{handler.Handler.HandlerName} - Err Out: " );
                    output.AppendLine( handler.ProcessErrOut );

                    Errors++;
                }
            }

            Output = output.ToString();
        }
    }
}
