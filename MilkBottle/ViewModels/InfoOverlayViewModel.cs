using System;
using MilkBottle.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;

namespace MilkBottle.ViewModels {
    class InfoOverlayViewModel : PropertyChangeBase, IDisposable {
        private const string                cInactiveState = "_inactiveState";
        private const string                cDisplayTrackInformation = "_displayTrackInformation";
        private IDisposable                 mPlaybackSubscription;

        public  string                      VisualState { get; private set; }

        public  string                      ArtistName { get; private set; }
        public  string                      AlbumName { get; private set; }
        public  string                      TrackName { get; private set; }

        public  DelegateCommand     Animate { get; }

        public InfoOverlayViewModel( IIpcManager ipcManager ) {
            Animate = new DelegateCommand( OnAnimate );

            ArtistName = "The Rolling Stones"; //String.Empty;
            AlbumName = "Sticky Fingers"; // String.Empty;
            TrackName = "You Can't Always Get What You Want"; // String.Empty;
            VisualState = cInactiveState;

            mPlaybackSubscription = ipcManager.OnPlaybackEvent.Subscribe( OnPlaybackEvent );
        }

        private void OnAnimate() {
            TriggerDisplay();
        }

        private void OnPlaybackEvent( PlaybackEvent args ) {
            if(!String.IsNullOrWhiteSpace( args?.TrackName )) {
                ArtistName = args.ArtistName;
                AlbumName = args.AlbumName;
                TrackName = args.TrackName;

                RaisePropertyChanged( () => ArtistName );
                RaisePropertyChanged( () => AlbumName );
                RaisePropertyChanged( () => TrackName );

                TriggerDisplay();
            }
        }

        private void TriggerDisplay() {
            VisualState = cDisplayTrackInformation + '|' + DateTime.Now.Millisecond;

            RaisePropertyChanged( () => VisualState );
        }

        public void Dispose() {
            mPlaybackSubscription?.Dispose();
            mPlaybackSubscription = null;
        }
    }
}
