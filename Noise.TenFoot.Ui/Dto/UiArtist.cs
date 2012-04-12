using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Noise.Infrastructure.Dto;

namespace Noise.TenFoot.Ui.Dto {
	[DebuggerDisplay("UiArtist = {Name}")]
	public class UiArtist : UiBase {
		private Artwork						mArtistImage;
		private readonly DelegateCommand	mOnSelected;
		private readonly Action<UiArtist>	mSelectAction; 

		public	string		Name { get; set; }
		public	string		DisplayName { get; set; }
		public	string		SortName { get; set; }
		public	int			AlbumCount { get; set; }

		protected UiArtist() {
			mOnSelected = new DelegateCommand( SelectArtist );
		}

		public UiArtist( Action<UiArtist> onSelect ) :
			this() {
			mSelectAction = onSelect;
		}

		public Artwork ArtistImage {
			get{ return( mArtistImage ); }
			set{
				mArtistImage = value;
				RaisePropertyChanged( () => ArtistImage );
			}
		}

		public ICommand OnSelect {
			get{ return( mOnSelected ); }
		}

		private void SelectArtist() {
			if( mSelectAction != null ) {
				mSelectAction( this );
			}
		}
	}
}
