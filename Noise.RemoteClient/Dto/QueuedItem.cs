using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Dto {
    public class QueuedItem {
        public  AlbumInfo   Album { get; }
        public  TrackInfo   Track { get; }

        public  bool        IsAlbum => Album != null;

        public QueuedItem( AlbumInfo album ) {
            Album = album;
        }

        public QueuedItem( TrackInfo track ) {
            Track = track;
        }

        public string QueuedItemName {
            get {
                var retValue = string.Empty;

                if( Album != null ) {
                    retValue = Album.AlbumName;
                }

                if( Track != null ) {
                    retValue = Track.TrackName;
                }

                return retValue;
            }
        }
    }
}
