using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class NoiseContext : DbContext, IDbContext {
		public NoiseContext() :
			base( "NoiseDatabase" ) { }

		public IDbSet<DbArtist>	Artists { get; set; }
		public IDbSet<DbAlbum>	Albums { get; set; }
 		public IDbSet<DbTrack>	Tracks { get; set; } 

		protected override void OnModelCreating( DbModelBuilder modelBuilder ) {
			modelBuilder.Configurations.Add( new ArtistConfiguration());
			modelBuilder.Configurations.Add( new AssociatedItemConfiguration());
			modelBuilder.Configurations.Add( new AssociatedItemListConfiguration());
			modelBuilder.Configurations.Add( new AlbumConfiguration());
			modelBuilder.Configurations.Add( new DiscographyConfiguration());
			modelBuilder.Configurations.Add( new GenreConfiguration());
			modelBuilder.Configurations.Add( new InternetStreamConfiguration());
			modelBuilder.Configurations.Add( new LyricConfiguration());
			modelBuilder.Configurations.Add( new PlayHistoryConfiguration());
			modelBuilder.Configurations.Add( new TrackConfiguration());
		}

		public new IDbSet<TEntity> Set<TEntity>() where TEntity : class {
            return base.Set<TEntity>();
        }
	}

	internal class ArtistConfiguration : EntityTypeConfiguration<DbArtist> {
		internal ArtistConfiguration() {
			Map( m => {
					m.ToTable( "Artists" );
			     	m.MapInheritedProperties();
			     });

			HasKey( p => p.DbId );
			Property( p => p.DbId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );

			Ignore( p => p.Genre );
			Ignore( p => p.Rating );
			Ignore( p => p.IsUserRating );
			Ignore( p => p.DateAdded );
		}
	}

	internal class AlbumConfiguration : EntityTypeConfiguration<DbAlbum> {
		internal AlbumConfiguration() {
			Map( m => {
					m.ToTable( "Albums" );
			     	m.MapInheritedProperties();
			     });

			HasKey( p => p.DbId );
			Property( p => p.DbId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );

			Ignore( p => p.DateAdded );
			Ignore( p => p.Genre );
			Ignore( p => p.Rating );
			Ignore( p => p.IsUserRating );
		}
	}

	internal class TrackConfiguration : EntityTypeConfiguration<DbTrack> {
		internal TrackConfiguration() {
			Map( m => {
					m.ToTable( "Tracks" );
			     	m.MapInheritedProperties();
			     });

			HasKey( p => p.DbId );
			Property( p => p.DbId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );

			Ignore( p => p.DateAdded );
			Ignore( p => p.Duration );
			Ignore( p => p.Genre );
			Ignore( p => p.Rating );
			Ignore( p => p.IsUserRating );
		}
	}

	internal class AssociatedItemConfiguration : EntityTypeConfiguration<DbAssociatedItem> {
		internal AssociatedItemConfiguration() {
			Map( m => {
			     	m.ToTable( "AssociatedItems" );
					m.MapInheritedProperties();
			     });

			HasKey( p => p.DbId );
			Property( p => p.DbId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );
		}
	}

	internal class AssociatedItemListConfiguration : EntityTypeConfiguration<DbAssociatedItemList> {
		internal AssociatedItemListConfiguration() {
			Map( m => {
			     	m.ToTable( "AssociatedItemLists" );
					m.MapInheritedProperties();
			     });

			HasKey( p => p.DbId );
			Property( p => p.DbId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );

			Ignore( p => p.ContentType );
		}
	}

	internal class DiscographyConfiguration : EntityTypeConfiguration<DbDiscographyRelease> {
		internal DiscographyConfiguration() {
			Map( m => {
			     	m.ToTable( "Discography" );
					m.MapInheritedProperties();
			     });

			HasKey( p => p.DbId );
			Property( p => p.DbId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );

			Ignore( p => p.ReleaseType );
		}
	}

	internal class GenreConfiguration : EntityTypeConfiguration<DbGenre> {
		internal GenreConfiguration() {
			Map( m => {
			     	m.ToTable( "Genres" );
					m.MapInheritedProperties();
			     });

			HasKey( p => p.DbId );
			Property( p => p.DbId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );
		}
	}

	internal class InternetStreamConfiguration : EntityTypeConfiguration<DbInternetStream> {
		internal InternetStreamConfiguration() {
			Map( m => {
			     	m.ToTable( "Streams" );
					m.MapInheritedProperties();
			     });

			HasKey( p => p.DbId );
			Property( p => p.DbId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );

			Ignore( p => p.Encoding );
		}
	}

	internal class LyricConfiguration : EntityTypeConfiguration<DbLyric> {
		internal LyricConfiguration() {
			Map( m => {
			     	m.ToTable( "Lyrics" );
					m.MapInheritedProperties();
			     });

			HasKey( p => p.DbId );
			Property( p => p.DbId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );
		}
	}

	internal class PlayHistoryConfiguration : EntityTypeConfiguration<DbPlayHistory> {
		internal PlayHistoryConfiguration() {
			Map( m => {
			     	m.ToTable( "PlayHistory" );
					m.MapInheritedProperties();
			     });

			HasKey( p => p.DbId );
			Property( p => p.DbId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );
		}
	}
}
