using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Adapters {
	public class LibraryAdditionNode : AutomaticPropertyBase, IPlayingItem {
        private readonly Action<LibraryAdditionNode>	mAlbumPlayAction;

        public	DbArtist		Artist { get; }
		public	DbAlbum			Album { get; }
		public	double			RelativeAge { get; set; }
        public  DelegateCommand Play { get; }

		public LibraryAdditionNode( DbArtist artist, DbAlbum album, Action<LibraryAdditionNode> albumPlayAction ) {
			Artist = artist;
			Album = album;

			mAlbumPlayAction = albumPlayAction;

            Play = new DelegateCommand( OnPlay );
		}

        public bool IsPlaying {
            get { return( Get( () => IsPlaying )); }
            set {  Set( () => IsPlaying, value ); }
        }

        public void SetPlayingStatus( PlayingItem item ) {
            IsPlaying = Album.DbId.Equals( item.Album );
        }

        private void OnPlay() {
            // trigger the track queue animation
            RaisePropertyChanged( "AnimateQueueTrack" );

            mAlbumPlayAction( this );
		}
	}
}
