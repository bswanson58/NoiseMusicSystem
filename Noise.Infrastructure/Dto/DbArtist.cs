using System;
using System.Diagnostics;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("Artist = {" + nameof(Name) + "}")]
	public class DbArtist : DbBase, IUserSettings, IVersionable {
		public string			Name { get; set; }
		public long				CalculatedGenre { get; set; }
		public long				ExternalGenre { get; set; }
		public long				UserGenre { get; set; }
		public Int16			UserRating { get; set; }
		public Int16			CalculatedRating { get; set; }
		public Int16			AlbumCount { get; set; }
		public long				DateAddedTicks { get; protected set; }
		public bool				IsFavorite { get; set; }
		public bool				HasFavorites { get; set; }
		public long				LastChangeTicks { get; protected set; }
		public Int32			PlayCount { get; private set; }
		public long				LastPlayedTicks { get; private set; }
		public Int32			ViewCount { get; private set; }
		public long				LastViewedTicks { get; private set; }
		public long				Version { get; set; }

		public DbArtist() {
			Name = "";
			DateAddedTicks = DateTime.Now.Ticks;
			CalculatedGenre = Constants.cDatabaseNullOid;
			ExternalGenre = Constants.cDatabaseNullOid;
			UserGenre = Constants.cDatabaseNullOid;

			UpdateLastChange();
		}

		public long Genre {
			get => ( UserGenre == Constants.cDatabaseNullOid ? ( ExternalGenre == Constants.cDatabaseNullOid ? CalculatedGenre : ExternalGenre ) : UserGenre );
            set => UserGenre = value;
        }

		public Int16 Rating {
			get => ( IsUserRating ? UserRating : CalculatedRating );
            set => UserRating = value;
        }

		public bool IsUserRating => ( UserRating != 0 );

        public DateTime DateAdded => ( new DateTime( DateAddedTicks ));

        public void UpdateLastChange() {
			LastChangeTicks = DateTime.Now.Ticks;
		}

		public void UpdateLastPlayed() {
			PlayCount++;
			LastPlayedTicks = DateTime.Now.Ticks;
		}

		public void UpdateLastViewed() {
			ViewCount++;
			LastViewedTicks = DateTime.Now.Ticks;
		}

		public void UpdateVersion() {
			Version++;
		}

		public void SetVersionPreUpdate( long version ) {
			Version = version - 1;
		}

		public override string ToString() {
			return( $"Artist \"{Name}\", Id:{DbId}" );
		}
	}
}
