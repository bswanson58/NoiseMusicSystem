﻿using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;
using ReusableBits.ExtensionClasses;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Album = {" + nameof(Name) + "}")]
	public class UiAlbum : UiBase, IPlayingItem {
		public	long					Artist { get; set; }
		public	Int16					TrackCount { get; set; }
		public	long					CalculatedGenre { get; set; }
		public	long					ExternalGenre { get; set; }
		public	long					UserGenre { get; set; }
		public	DbGenre					DisplayGenre { get ; set; }
	    public	string                  SortName { get; set; }
        private string                  mDisplayName;
		private readonly Action<long>	mOnPlay;

		public	DelegateCommand			PlayAlbum { get; }

		public UiAlbum( DbAlbum album ) {
			UpdateFromSource( album );

			PlayAlbum = new DelegateCommand( OnPlayAlbum );
		}

		public UiAlbum( DbAlbum album, Action<long> onPlay ) :
            this ( album ) {
			mOnPlay = onPlay;
		}

		public void UpdateFromSource( DbAlbum album ) {
			if( album != null ) {
				DbId = album.DbId;

				Name = album.Name;
				Artist = album.Artist;
				IsFavorite = album.IsFavorite;
				HasFavorites = album.HasFavorites;
				CalculatedRating = album.CalculatedRating;
				PublishedYear = album.PublishedYear;
				Rating = album.Rating;
				TrackCount = album.TrackCount;
				CalculatedGenre = album.CalculatedGenre;
				ExternalGenre = album.ExternalGenre;
				UserGenre = album.UserGenre;
				UserRating = album.UserRating;

                UiIsFavorite = album.IsFavorite;
                UiRating = album.Rating;
                SortName = album.Name;
            }
        }

		public string Name {
			get{ return( Get( () => Name )); }
			set {
                Set( () => Name, value ); 

				mDisplayName = CreateDisplayName( Name );
				RaisePropertyChanged( () => DisplayName );
            }
		}

	    public string DisplayName {
	        get {
                if( String.IsNullOrWhiteSpace( mDisplayName )) {
                    mDisplayName = CreateDisplayName( Name );
                }

                return mDisplayName;
	        }
            set => mDisplayName = CreateDisplayName( value );
	    }

		public string Genre => ( DisplayGenre != null ? DisplayGenre.Name : String.Empty );

	    public Int32 PublishedYear {
			get{ return( Get( () => PublishedYear )); }
			set{ Set( () => PublishedYear, value ); }
		}

		public bool	IsFavorite {
			get { return( Get(() => IsFavorite )); }
			set {  Set( () => IsFavorite, value ); }
		}

        public bool IsPlaying {
            get { return( Get(() => IsPlaying )); }
            set {  Set( () => IsPlaying, value ); }
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

		[DependsUpon("UserRating")]
		public bool IsUserRating => ( UserRating != 0 );

	    [DependsUpon("CalculatedRating")]
		public Int16 Rating {
			get => ( IsUserRating ? UserRating : CalculatedRating );
	        set => UserRating = value;
	    }

        // used to sort albums by rating in the album list.
	    public int SortRating => IsFavorite ? 20 : ( Rating * 2 ) + ( HasFavorites ? 1 : 0 );

	    [DependsUpon("UserRating")]
		public bool UseAlternateRating => (!IsUserRating );

	    public override string ToString() {
			return( Name );
		}

		private void OnPlayAlbum() {
            // trigger the track queue animation
            RaisePropertyChanged( "AnimateQueueTrack" );

		    mOnPlay?.Invoke( DbId );
		}

	    private static string CreateDisplayName( string fullAlbumName ) {
            // Strip any published year from the end.
            var regex = new Regex( @".+(?<publishedYear>-\s*\d{4})\Z" );

            return fullAlbumName.Replace( regex, "publishedYear", String.Empty ).Trim();
        }

        public void SetPlayingStatus( PlayingItem item ) {
            IsPlaying = DbId.Equals( item.Album );
        }
    }
}
