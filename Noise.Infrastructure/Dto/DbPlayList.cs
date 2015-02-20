using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Noise.Infrastructure.Dto {
	public class DbPlayList : DbBase, IUserSettings {
		public string			Name { get; set; }
		public string			Description { get; set; }
		public Int16			Rating { get; set; }
		public long				Genre { get; set; }
		public bool				IsFavorite { get; set; }
		public string			PersistedTrackIds { get; protected set; }

		public DbPlayList() {
			Name = string.Empty;
			Description = string.Empty;
			PersistedTrackIds = string.Empty;
		}

		public DbPlayList( string name, string description, IEnumerable<long> trackIds ) :
			this() {
			Name = name;
			Description = description;

			TrackIds = trackIds;
		}

		public bool IsUserRating {
			get { return( true ); }
		}

		public IEnumerable<long> TrackIds {
			get {
				if(!string.IsNullOrWhiteSpace( PersistedTrackIds )) {
					return( Array.ConvertAll( PersistedTrackIds.Split( ';' ), long.Parse ).ToList());
				}

				return( new long[0]);
			}
			set {
				PersistedTrackIds = String.Join( ";", value.Select( p => p.ToString( CultureInfo.InvariantCulture )).ToArray());
			}
		}

		public override string ToString() {
			return( string.Format( "Playlist \"{0}\"", Name ));
		}
	}
}
