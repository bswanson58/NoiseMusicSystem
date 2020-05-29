using System;
using Noise.Infrastructure.Dto;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
	public class UiTimeExplorerAlbum : PropertyChangeBase {
		private readonly Action<UiTimeExplorerAlbum>	mOnPlay;

		public	DbArtist	Artist { get; }
		public	DbAlbum		Album { get; }

		public	DelegateCommand	Play {  get; }

		public UiTimeExplorerAlbum( DbArtist artist, DbAlbum album, Action<UiTimeExplorerAlbum> onPlay ) {
			Artist = artist;
			Album = album;

			mOnPlay = onPlay;

			Play = new DelegateCommand( OnPlay );
		}

		private void OnPlay() {
            mOnPlay?.Invoke( this );
        }
	}
}
