using System;
using Noise.Infrastructure.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
	public class UiTimeExplorerAlbum : AutomaticCommandBase {
		private readonly Action<UiTimeExplorerAlbum>	mOnPlay;

		public	DbArtist	Artist { get; private set; }
		public	DbAlbum		Album { get; private set; }

		public UiTimeExplorerAlbum( DbArtist artist, DbAlbum album, Action<UiTimeExplorerAlbum> onPlay ) {
			Artist = artist;
			Album = album;

			mOnPlay = onPlay;
		}

		public void Execute_Play() {
			if( mOnPlay != null ) {
				mOnPlay( this );
			}
		}
	}
}
