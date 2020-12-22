using System.Diagnostics;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;

namespace Noise.RemoteClient.Dto {
    [DebuggerDisplay("Track = {" + nameof(ArtistName) + "}")]
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

        public  string              DisplayName { get; private set; }
        public  string              SortName { get; private set; }
        public  int                 SortRating => ( IsFavorite ? 6 : 0 ) + Rating;

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

        public void SetDisplayName( string displayName ) {
            DisplayName = displayName;

            RaisePropertyChanged( nameof( DisplayName ));
        }

        public void SetSortName( string sortName ) {
            SortName = sortName;

            RaisePropertyChanged( nameof( SortName ));
        }
    }
}
