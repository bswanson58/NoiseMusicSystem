﻿using System;
using System.Diagnostics;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Artist = {Name}")]
	public class UiArtist : UiBase {
		public string			Name { get; set; }
		public string			SortName { get; set; }
		public string			DisplayName { get; set; }
		public string			Website { get; set; }
		public long				CalculatedGenre { get; set; }
		public long				ExternalGenre { get; set; }
		public long				UserGenre { get; set; }
		public Int16			UserRating { get; set; }
		public Int16			CalculatedRating { get; set; }
		public Int16			MaxChildRating { get; set; }
		public bool				IsFavorite { get; set; }
		public bool				HasFavorites { get; set; }

		private readonly Action<long>	mSelectArtistAction;

		public UiArtist( Action<long> onSelectAction ) {
			mSelectArtistAction = onSelectAction;
		}

		public Int16 AlbumCount {
			get{ return( Get( () => AlbumCount )); }
			set{ Set( () => AlbumCount, value ); }
		}

		public DbGenre DisplayGenre {
			get{ return( Get( () => DisplayGenre )); }
			set{ Set( () => DisplayGenre, value ); }
		}

		[DependsUpon("DisplayGenre")]
		public string Genre {
			get{ return( DisplayGenre != null ? DisplayGenre.Name : "" ); }
		}

		private bool IsUserRating {
			get{ return( UserRating != 0 ); }
		}

		public Int16 Rating {
			get{ return( IsUserRating ? UserRating : CalculatedRating ); }
			set{ UserRating = value; }
		}

		public bool IsSelected {
			get { return( Get( () => IsSelected )); }
			set {
				Set( () => IsSelected, value  );

				if(( value ) &&
				   ( mSelectArtistAction != null )) {
					mSelectArtistAction( DbId );
				}
			}
		}
	}
}
