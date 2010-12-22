using System;
using System.Collections.Generic;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;

namespace Noise.UI.Adapters {
	public class ArtistTreeNode : ViewModelBase {
		private readonly Func<ArtistTreeNode,IEnumerable<UiAlbum>>	mChildFillFunction;
		private	bool							mRequiresChildren;

		public	UiArtist						Artist { get; private set; }
		public	ObservableCollectionEx<UiAlbum>	Children { get; set; }

		public ArtistTreeNode( UiArtist artist, Func<ArtistTreeNode, IEnumerable<UiAlbum>> fillFunc ) {
			Artist = artist;
			mChildFillFunction = fillFunc;
			Children = new ObservableCollectionEx<UiAlbum> { new UiAlbum( "Albums are being loaded..." ) };
			mRequiresChildren = true;
		}

		private void SetChildren( IEnumerable<UiAlbum> children ) {
			Children.SuspendNotification();
			Children.Clear();
			Children.AddRange( children );
			Children.ResumeNotification();

			mRequiresChildren = false;
			RaisePropertyChanged( () => Children );
		}

		public bool IsSelected {
			get { return( Artist.IsSelected ); }
			set {
				Artist.IsSelected = value;
				RaisePropertyChanged( () => IsSelected );
			}
		}

		public bool IsExpanded {
			get { return( Get( () => IsExpanded )); }
			set {
				if( value ) {
 					IsSelected = true;
				}

				Set( () => IsExpanded, value  );

				if(( value ) &&
				   ( mChildFillFunction != null )) {
					if( mRequiresChildren ) {
						SetChildren( mChildFillFunction( this ));
					}
					else {
						if(( Children == null ) ||
						   ( Artist.AlbumCount != Children.Count )) {
							SetChildren( mChildFillFunction( this ));
						}
					}
				}
			}
		}

		public void Execute_Rename() {
		}

		public bool CanExecute_Rename() {
			return( false );
		}
	}
}
