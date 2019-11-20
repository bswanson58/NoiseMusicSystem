namespace Album4Matter.Dto {
    public class TargetVolume {
        public  string      VolumeName;
        public  string[]    VolumeContents;
    }

    public class TargetAlbumLayout {
        public  string          ArtistName;
        public  string          AlbumName;
        public  TargetVolume    AlbumList;
        public  TargetVolume[]  VolumeList;
    }
}
