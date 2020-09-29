using System;
using MilkBottle.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;

namespace MilkBottle.ViewModels {
    class InfoOverlayViewModel : PropertyChangeBase, IDisposable {
        private IDisposable                 mPlaybackSubscription;

        public  string                      TrackInformationTrigger { get; private set; }

        public  string                      ArtistName { get; private set; }
        public  string                      AlbumName { get; private set; }
        public  string                      TrackName { get; private set; }

        public  DelegateCommand     Animate { get; }

        public InfoOverlayViewModel( IIpcManager ipcManager ) {
            Animate = new DelegateCommand( OnAnimate );

            ArtistName = "The Rolling Stones"; //String.Empty;
            AlbumName = "Sticky Fingers"; // String.Empty;
            TrackName = "You Can't Always Get What You Want"; // String.Empty;

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
            TrackInformationTrigger = DateTime.Now.Millisecond.ToString();

            RaisePropertyChanged( () => TrackInformationTrigger );
        }

        public void Dispose() {
            mPlaybackSubscription?.Dispose();
            mPlaybackSubscription = null;
        }
    }
}
