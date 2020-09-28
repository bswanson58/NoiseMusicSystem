using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MilkBottle.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;

namespace MilkBottle.ViewModels {
    class InfoOverlayViewModel : PropertyChangeBase, IDisposable {
        private const string                cArtistElement = "_artistName";
        private const string                cAlbumElement = "_albumName";
        private const string                cTrackElement = "_trackName";

        private readonly Subject<string>    mArtistSubject;
        private readonly Subject<string>    mAlbumSubject;
        private readonly Subject<string>    mTrackSubject;
        private IDisposable                 mPlaybackSubscription;
        private IDisposable                 mArtistSubscription;
        private IDisposable                 mAlbumSubscription;
        private IDisposable                 mTrackSubscription;

        public  string                      AnimationTrigger { get; private set; }
        public  string                      TargetElement { get; private set; }

        public  string                      ArtistName { get; private set; }
        public  string                      AlbumName { get; private set; }
        public  string                      TrackName { get; private set; }

        public  DelegateCommand     Animate { get; }

        public InfoOverlayViewModel( IIpcManager ipcManager ) {
            Animate = new DelegateCommand( OnAnimate );

            mArtistSubject = new Subject<string>();
            mArtistSubscription = mArtistSubject.Delay( TimeSpan.FromMilliseconds( 500 )).ObserveOnDispatcher().Subscribe( OnArtist );
            mAlbumSubject = new Subject<string>();
            mAlbumSubscription = mAlbumSubject.Delay( TimeSpan.FromMilliseconds( 7000 )).ObserveOnDispatcher().Subscribe( OnAlbum );
            mTrackSubject = new Subject<string>();
            mTrackSubscription = mTrackSubject.Delay( TimeSpan.FromMilliseconds( 12000 )).ObserveOnDispatcher().Subscribe( OnTrack );

            ArtistName = String.Empty;
            AlbumName = String.Empty;
            TrackName = String.Empty;
            AnimationTrigger = String.Empty;

            mPlaybackSubscription = ipcManager.OnPlaybackEvent.Subscribe( OnPlaybackEvent );
        }

        private void OnAnimate() {
            mArtistSubject.OnNext( "The Rolling Stones" );
            mAlbumSubject.OnNext( "Sticky Fingers" );
            mTrackSubject.OnNext( "Bitch" );
        }

        private void OnPlaybackEvent( PlaybackEvent args ) {
            mArtistSubject.OnNext( args.ArtistName );
            mAlbumSubject.OnNext( args.AlbumName );
            mTrackSubject.OnNext( args.TrackName );
        }

        private void OnArtist( string artist ) {
            ArtistName = artist;

            TargetElement = cArtistElement;
            AnimationTrigger = artist;

            RaisePropertyChanged( () => ArtistName );
            RaisePropertyChanged( () => TargetElement );
            RaisePropertyChanged( () => AnimationTrigger );
        }

        private void OnAlbum( string album ) {
            AlbumName = album;

            TargetElement = cAlbumElement;
            AnimationTrigger = album;

            RaisePropertyChanged( () => AlbumName );
            RaisePropertyChanged( () => TargetElement );
            RaisePropertyChanged( () => AnimationTrigger );
        }

        private void OnTrack( string track ) {
            TrackName = track;

            TargetElement = cTrackElement;
            AnimationTrigger = track;

            RaisePropertyChanged( () => TrackName );
            RaisePropertyChanged( () => TargetElement );
            RaisePropertyChanged( () => AnimationTrigger );
        }

        public void Dispose() {
            mPlaybackSubscription?.Dispose();
            mPlaybackSubscription = null;

            mArtistSubscription?.Dispose();
            mArtistSubscription = null;

            mAlbumSubscription?.Dispose();
            mAlbumSubscription = null;

            mTrackSubscription?.Dispose();
            mTrackSubscription = null;
        }
    }
}
