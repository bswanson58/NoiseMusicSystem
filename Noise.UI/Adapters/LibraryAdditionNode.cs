using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class LibraryAdditionNode : ViewModelBase {
		public	DbArtist		Artist { get; private set; }
		public	DbAlbum			Album { get; private set; }
		public	double			RelativeAge { get; set; }
		private readonly Action<LibraryAdditionNode>	mAlbumPlayAction;

		public LibraryAdditionNode( DbArtist artist, DbAlbum album, Action<LibraryAdditionNode> albumPlayAction ) {
			Artist = artist;
			Album = album;

			mAlbumPlayAction = albumPlayAction;
		}

		public void Execute_Play( object sender ) {
			mAlbumPlayAction( this );
		}
	}
}
