using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Logging;
using Noise.UI.Support;
using ReusableBits;

namespace Noise.UI.ViewModels {
    class ArtistArtworkViewModel : DialogModelBase {
        private readonly IUiLog                     mLog;
        private readonly UiArtist                   mArtist;
        private readonly IMetadataManager           mMetadataManager;
        private readonly string                     mInitialArtworkName;
        private TaskHandler<IEnumerable<Artwork>>   mArtworkTask;
        private Artwork                             mCurrentArtwork;

        public  BindableCollection<Artwork>         Portfolio { get; }

        public ArtistArtworkViewModel( IMetadataManager metadataManager, UiArtist artist, IUiLog log, string currentArtworkName ) {
            mMetadataManager = metadataManager;
            mArtist = artist;
            mLog = log;
            mInitialArtworkName = currentArtworkName;

            Portfolio = new BindableCollection<Artwork>();
            LoadArtwork();
        }

        public Artwork CurrentArtwork {
            get => mCurrentArtwork;
            set {
                mCurrentArtwork = value;
                RaisePropertyChanged( () => CurrentArtwork );
            }
        }

        protected TaskHandler<IEnumerable<Artwork>> ArtworkTask {
            get {
                if( mArtworkTask == null ) {
                    Execute.OnUIThread( () => mArtworkTask = new TaskHandler<IEnumerable<Artwork>>());
                }

                return( mArtworkTask );
            }

            set => mArtworkTask = value;
        }

        private void LoadArtwork() {
            ArtworkTask.StartTask( RetrieveArtwork, SetArtwork, ex => { mLog.LogException( $"Loading artwork for artist: {mArtist.Name}", ex );});
        }

        private IEnumerable<Artwork> RetrieveArtwork() {
            return mMetadataManager.GetArtistPortfolio( mArtist.Name );
        }

        private void SetArtwork( IEnumerable<Artwork>  artworkList ) {
            Portfolio.Clear();
            Portfolio.AddRange( artworkList );

            if(!string.IsNullOrWhiteSpace( mInitialArtworkName )) {
                CurrentArtwork = Portfolio.FirstOrDefault( a => a.Name.Equals( mInitialArtworkName ));
            }

            if( CurrentArtwork == null ) {
                CurrentArtwork = Portfolio.FirstOrDefault();
            }
        }
    }
}
