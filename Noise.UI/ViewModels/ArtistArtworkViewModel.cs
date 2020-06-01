using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Logging;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    class ArtistArtworkViewModel : PropertyChangeBase, IDialogAware {
        public  const string                        cArtistParameter = "artist";
        public  const string                        cArtworkParameter = "artwork";

        private readonly IUiLog                     mLog;
        private readonly IMetadataManager           mMetadataManager;
        private string                              mInitialArtworkName;
        private UiArtist                            mArtist;
        private TaskHandler<IEnumerable<Artwork>>   mArtworkTask;
        private Artwork                             mCurrentArtwork;

        public  BindableCollection<Artwork>         Portfolio { get; }
        public  string                              Title { get; }
        public  DelegateCommand                     Ok { get; }
        public  event Action<IDialogResult>         RequestClose;

        public ArtistArtworkViewModel( IMetadataManager metadataManager, IUiLog log ) {
            mMetadataManager = metadataManager;
            mLog = log;

            Portfolio = new BindableCollection<Artwork>();

            Ok = new DelegateCommand( OnOk );

            Title = "Artist Portfolio";
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            mInitialArtworkName = parameters.GetValue<string>( cArtworkParameter );
            mArtist = parameters.GetValue<UiArtist>( cArtistParameter );

            if( mArtist != null ) {
                LoadArtwork();
            }
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
            Portfolio.AddRange( from artwork in artworkList orderby artwork.Name select artwork );

            if(!string.IsNullOrWhiteSpace( mInitialArtworkName )) {
                CurrentArtwork = Portfolio.FirstOrDefault( a => a.Name.Equals( mInitialArtworkName ));
            }

            if( CurrentArtwork == null ) {
                CurrentArtwork = Portfolio.FirstOrDefault();
            }
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnOk() {
            RaiseRequestClose( new DialogResult( ButtonResult.OK ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
