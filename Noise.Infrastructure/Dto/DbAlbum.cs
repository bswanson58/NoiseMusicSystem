﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("Album = {" + nameof( Name ) + "}")]
	public class DbAlbum : DbBase, IUserSettings, IVersionable {
		public string			Name { get; set; }
		public long				Artist { get; set; }
		public Int16			UserRating { get; set; }
		public Int16			CalculatedRating { get; set; }
		public Int16			TrackCount { get; set; }
		[Required]
		[RegularExpression( "^(?:0|1|\\d{4})$", ErrorMessage = "Only 0 (zero) or 1900-2100 are allowed.")]
		[Range( 0, 2100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
		public Int32			PublishedYear { get; set; }
		public long				CalculatedGenre { get; set; }
		public long				ExternalGenre { get; set; }
		public long				UserGenre { get; set; }
		public bool				IsFavorite { get; set; }
		public bool				HasFavorites { get; set; }
		public long				DateAddedTicks { get; private set; }
		public Int32			PlayCount { get; private set;}
		public long				LastPlayedTicks { get; private set; }
		public float			ReplayGainAlbumGain { get; set; }
		public float			ReplayGainAlbumPeak { get; set; }
		public long				Version { get; set; }

        public bool             IsUserRating => UserRating != 0;
        public DateTime         DateAdded => new DateTime( DateAddedTicks );

		public DbAlbum() {
			Name = string.Empty;
			CalculatedGenre = Constants.cDatabaseNullOid;
			ExternalGenre = Constants.cDatabaseNullOid;
			UserGenre = Constants.cDatabaseNullOid;
			DateAddedTicks = DateTime.Now.Ticks;
			PublishedYear = Constants.cUnknownYear;
		}

		public long Genre {
			get => ( UserGenre == Constants.cDatabaseNullOid ? ExternalGenre == Constants.cDatabaseNullOid ? CalculatedGenre : ExternalGenre : UserGenre );
            set => UserGenre = value;
        }

		public Int16 Rating {
			get => ( IsUserRating ? UserRating : CalculatedRating );
            set => UserRating = value;
        }

        public void UpdateLastPlayed() {
			PlayCount++;
			LastPlayedTicks = DateTime.Now.Ticks;
		}

		public void SetPublishedYear( int year ) {
			if(( year == Constants.cUnknownYear ) ||
			   ( year == Constants.cVariousYears ) ||
			  (( year >= 1900 ) &&
			   ( year <= 2100 ))) {
				PublishedYear = year;
			}
		}

		public void UpdateVersion() {
			Version++;
		}

		public void SetVersionPreUpdate( long version ) {
			Version = version - 1;
		}

		public override string ToString() {
			return( $"Album \"{Name}\", Id:{DbId}, Artist:{Artist}" );
		}
	}
}
