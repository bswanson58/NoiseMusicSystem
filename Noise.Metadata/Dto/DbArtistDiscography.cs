using System.Collections.Generic;
using System.Diagnostics;
using LiteDB;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Metadata.Dto {
	[DebuggerDisplay("Artist = {" + nameof(ArtistName) + "}")]
	internal class DbArtistDiscography : EntityBase, IArtistDiscography {
		public	string					ArtistName { get; set; }
		public	List<DbDiscographyRelease>	Discography { get; set; }

		public DbArtistDiscography() {
			ArtistName = string.Empty;
			Discography = new List<DbDiscographyRelease>();
		}

		[BsonCtor]
		public DbArtistDiscography( ObjectId id, string artistName ) :
			base( id ) {
			ArtistName = artistName;

			Discography = new List<DbDiscographyRelease>();
        }
	}
}
