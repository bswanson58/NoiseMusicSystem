using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class LibraryAdditionNode : ViewModelBase, IPlayingItem {
		public	DbArtist		Artist { get; }
		public	DbAlbum			Album { get; }
		public	double			RelativeAge { get; set; }
		private readonly Action<LibraryAdditionNode>	mAlbumPlayAction;

		public LibraryAdditionNode( DbArtist artist, DbAlbum album, Action<LibraryAdditionNode> albumPlayAction ) {
			Artist = artist;
			Album = album;

			mAlbumPlayAction = albumPlayAction;
		}

        public bool IsPlaying {
            get { return( Get( () => IsPlaying )); }
            set {  Set( () => IsPlaying, value ); }
        }

        public void SetPlayingStatus( PlayingItem item ) {
            IsPlaying = Album.DbId.Equals( item.Album );
        }

        public void Execute_Play( object sender ) {
            // trigger the track queue animation
            RaisePropertyChanged( "AnimateQueueTrack" );

            mAlbumPlayAction( this );
		}
	}
}
