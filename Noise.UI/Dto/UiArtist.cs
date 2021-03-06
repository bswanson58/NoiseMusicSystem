﻿using System;
using System.Diagnostics;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Artist = {" + nameof(Name) + "}")]
	public class UiArtist : UiBase, IPlayingItem {
        private readonly Action<UiArtist>   mOnGenreClicked;

		public	string						SortName { get; set; }
		public	long						CalculatedGenre { get; set; }
		public	long						ExternalGenre { get; set; }
		public	long						UserGenre { get; set; }

		public	DelegateCommand				GenreClicked { get; }

        public UiArtist( DbArtist artist ) {
			GenreClicked = new DelegateCommand( OnGenreClicked );

			UpdateFromSource( artist );
        }

        public UiArtist( DbArtist artist, Action<UiArtist> onGenreClicked ) : 
            this( artist ) {
            mOnGenreClicked = onGenreClicked;
        }

		public void UpdateFromSource( DbArtist artist ) {
            if( artist != null ) {
				DbId = artist.DbId;

                Name = artist.Name;
				DisplayName = artist.Name;
                SortName = artist.Name;
				AlbumCount = artist.AlbumCount;
				IsFavorite = artist.IsFavorite;
				HasFavorites = artist.HasFavorites;
				CalculatedRating = artist.CalculatedRating;
				UserRating = artist.UserRating;
				Rating = artist.Rating;
                UiIsFavorite = artist.IsFavorite;
                UiRating = artist.Rating;
            }
        }

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
		public string Genre => ( DisplayGenre != null ? DisplayGenre.Name : "" );

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
			set => UiIsFavorite = !UiIsFavorite;
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
			get => ( IsUserRating ? UserRating : CalculatedRating );
            set {
				UserRating = value;
				
				RaisePropertyChanged( () => IsUserRating );
				RaisePropertyChanged( () => UseAlternateRating );
			}
		}

		[DependsUpon("UserRating")]
		public bool IsUserRating => ( UserRating != 0 );

	    [DependsUpon("UserRating")]
		public bool UseAlternateRating => (!IsUserRating );

	    // used to sort albums by rating in the album list.
        public int SortRating => ( IsFavorite ? 6 : 0 ) + UserRating;

        public bool IsPlaying {
            get { return( Get( () => IsPlaying )); }
            set {  Set( () => IsPlaying, value ); }
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

        public void SetPlayingStatus( PlayingItem item ) {
            IsPlaying = DbId.Equals( item.Artist );
        }

        private void OnGenreClicked() {
            mOnGenreClicked?.Invoke( this );
        }
    }
}
