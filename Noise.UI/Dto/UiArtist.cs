using System;
using System.Diagnostics;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Artist = {Name}")]
	public class UiArtist : UiBase {
		public string			SortName { get; set; }
		public long				CalculatedGenre { get; set; }
		public long				ExternalGenre { get; set; }
		public long				UserGenre { get; set; }

		public string ActiveYears {
			get{ return( Get( () => ActiveYears )); }
			set{ Set( () => ActiveYears, value ); }
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

		public bool	IsFavorite {
			get { return( Get(() => IsFavorite )); }
			set {  Set( () => IsFavorite, value ); }
		}

		public bool	HasFavorites {
			get { return( Get( () => HasFavorites )); }
			set { Set( () => HasFavorites, value ); }
		}

		[DependsUpon("IsFavorite")]
		[DependsUpon("HasFavorites")]
		[DependsUpon("UiIsFavorite")]
		public bool? FavoriteValue {
			get {
				bool? retValue = UiIsFavorite;

				if(!retValue.Value ) {
					if( HasFavorites ) {
						retValue = null;
					}
				}

				return( retValue );
			}
			set { UiIsFavorite = !UiIsFavorite; }
		}

		public Int16 CalculatedRating {
			get { return( Get( () => CalculatedRating )); }
			set {  Set( () => CalculatedRating, value ); }
		}

		public Int16 UserRating {
			get { return( Get( () => UserRating )); }
			set { Set( () => UserRating, value ); }
		}

		[DependsUpon("CalculatedRating")]
		public Int16 Rating {
			get{ return( IsUserRating ? UserRating : CalculatedRating ); }
			set {
				UserRating = value;
				
				RaisePropertyChanged( () => IsUserRating );
				RaisePropertyChanged( () => UseAlternateRating );
			}
		}

		[DependsUpon("UserRating")]
		public bool IsUserRating {
			get{ return( UserRating != 0 ); }
		}

		[DependsUpon("UserRating")]
		public bool UseAlternateRating {
			get { return(!IsUserRating ); }
		}

		public string Name {
			get{ return( Get( () => Name )); }
			set{ Set( () => Name, value ); }
		}

		public string DisplayName {
			get{ return( Get( () => DisplayName )); }
			set{ Set( () => DisplayName, value ); }
		}

		public string Website {
			get{ return( Get( () => Website )); }
			set{ Set( () => Website, value ); }
		}

		public override string ToString() {
			return( Name );
		}
	}
}
