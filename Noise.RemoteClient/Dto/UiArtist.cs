using Noise.RemoteServer.Protocol;
using Prism.Mvvm;

namespace Noise.RemoteClient.Dto {
    class UiArtist : BindableBase {
        private bool                mIsPlaying;

        public  ArtistInfo          Artist { get; }
        public  long                ArtistId => Artist.DbId;
        public  string              ArtistName => Artist.ArtistName;
        public  string              Genre => Artist.Genre;
        public  int                 AlbumCount => Artist.AlbumCount;
        public  bool                IsFavorite => Artist.IsFavorite;
        public  int                 Rating => Artist.Rating;
        public  bool                HasRating => Rating != 0;

        public UiArtist( ArtistInfo artist ) {
            Artist = artist;
        }

        public string RatingSource {
            get {
                var retValue = "0_Star";

                if(( Rating > 0 ) &&
                   ( Rating < 6 )) {
                    retValue = $"{Rating:D1}_Star";
                }

                return retValue;
            }
        }

        public bool IsPlaying {
            get => mIsPlaying;
            set => SetProperty( ref mIsPlaying, value );
        }

        public void SetIsPlaying( PlayingState state ) {
            IsPlaying = ArtistId.Equals( state?.ArtistId );
        }

    }
}
