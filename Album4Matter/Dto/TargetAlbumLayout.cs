using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Album4Matter.Dto {
    [DebuggerDisplay("Volume = {" + nameof( VolumeName ) + "}")]
    public class TargetVolume {
        public  string              VolumeName;
        public  List<SourceItem>    VolumeContents;

        public TargetVolume() {
            VolumeName = String.Empty;

            VolumeContents = new List<SourceItem>();
        }

        public TargetVolume( string name, IEnumerable<SourceItem> list ) :
            this() {
            VolumeName = name;
            VolumeContents.AddRange( list );
        }
    }

    public class TargetAlbumLayout {
        public  string              ArtistName;
        public  string              AlbumName;
        public  TargetVolume        AlbumList;
        public  List<TargetVolume>  VolumeList;

        public TargetAlbumLayout() {
            ArtistName = String.Empty;
            AlbumName = String.Empty;

            AlbumList = new TargetVolume();
            VolumeList = new List<TargetVolume>();
        }

        public TargetAlbumLayout( string artist, string album ) :
            this() {
            ArtistName = artist;
            AlbumName = album;
        }

        public TargetAlbumLayout( string artist, string album, IEnumerable<SourceItem> albumContents ) :
            this( artist, album ) {
            AlbumList.VolumeContents.AddRange( albumContents );
        }
    }
}
