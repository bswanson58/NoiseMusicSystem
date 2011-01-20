using System;
using System.Diagnostics;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Album = {Name}")]
	public class UiAlbum : UiBase {
		public long				Artist { get; set; }
		public Int16			UserRating { get; set; }
		public Int16			CalculatedRating { get; set; }
		public Int16			MaxChildRating { get; set; }
		public Int16			TrackCount { get; set; }
		public long				CalculatedGenre { get; set; }
		public long				ExternalGenre { get; set; }
		public long				UserGenre { get; set; }
		public bool				IsFavorite { get; set; }
		public bool				HasFavorites { get; set; }
		public DbGenre			DisplayGenre { get ; set; }

		private readonly Action<long>	mSelectAlbumAction;
		private readonly Action<long>	mPlayAlbumAction;

		public UiAlbum( Action<long> onAlbumSelected, Action<long> onPlayAlbum ) {
			mSelectAlbumAction = onAlbumSelected;
			mPlayAlbumAction = onPlayAlbum;
		}

		public UiAlbum( string name ) {
			Name = name;
		}

		public string Name {
			get{ return( Get( () => Name )); }
			set{ Set( () => Name, value ); }
		}

		public string Genre {
			get{ return( DisplayGenre != null ? DisplayGenre.Name : "" ); }
		}

		public bool IsSelected {
			get { return( Get( () => IsSelected )); }
			set {
				Set( () => IsSelected, value  );

				if(( value ) &&
				   ( mSelectAlbumAction != null )) {
					mSelectAlbumAction( DbId );
				}
			}
		}

		public UInt32 PublishedYear {
			get{ return( Get( () => PublishedYear )); }
			set{ Set( () => PublishedYear, value ); }
		}

		public bool IsExpanded {
			get{ return( Get( () => IsExpanded )); }
			set{ Set( () => IsExpanded, value ); }
		}

		public void Execute_PlayAlbum() {
			if( mPlayAlbumAction != null ) {
				mPlayAlbumAction( DbId );
			}
		}

		public bool CanExecute_PlayAlbum() {
			return( mPlayAlbumAction != null );
		}
	}
}
