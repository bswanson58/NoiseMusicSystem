using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class NoiseContext : DbContext, IDbContext {
		public	IDbSet<DbArtist>	Artists { get; set; }
		public	IDbSet<DbAlbum>		Albums { get; set; }
		public	IDbSet<DbTrack>		Tracks { get; set; } 
		private readonly bool		mIsValidContext;

		public NoiseContext( string databaseName, string databaseConnectionString ) :
			base( databaseName ) {
			mIsValidContext = !string.IsNullOrWhiteSpace( databaseConnectionString );
	
			Database.Connection.ConnectionString = databaseConnectionString;
		}

		public bool IsValidContext {
			get { return( mIsValidContext ); }
		}

		protected override void OnModelCreating( DbModelBuilder modelBuilder ) {
			modelBuilder.Configurations.Add( new AlbumConfiguration());
			modelBuilder.Configurations.Add( new ArtistConfiguration());
			modelBuilder.Configurations.Add( new ArtworkConfiguration());
			modelBuilder.Configurations.Add( new GenreConfiguration());
			modelBuilder.Configurations.Add( new InternetStreamConfiguration());
			modelBuilder.Configurations.Add( new LyricConfiguration());
			modelBuilder.Configurations.Add( new PlayHistoryConfiguration());
			modelBuilder.Configurations.Add( new PlayListConfiguration());
			modelBuilder.Configurations.Add( new FolderStrategyConfiguration());
			modelBuilder.Configurations.Add( new RootFolderConfiguration());
			modelBuilder.Configurations.Add( new StorageFileConfiguration());
			modelBuilder.Configurations.Add( new StorageFolderConfiguration());
			modelBuilder.Configurations.Add( new StorageSidecarConfiguration());
			modelBuilder.Configurations.Add( new TagConfiguration());
			modelBuilder.Configurations.Add( new TagAssociationConfiguration());
			modelBuilder.Configurations.Add( new TextInfoConfiguration());
			modelBuilder.Configurations.Add( new TimestampConfiguration());
			modelBuilder.Configurations.Add( new TrackConfiguration());
			modelBuilder.Configurations.Add( new VersionConfiguration());

			modelBuilder.Ignore<Artwork>();
			modelBuilder.Ignore<TextInfo>();
		}

		public new IDbSet<TEntity> Set<TEntity>() where TEntity : class {
			return base.Set<TEntity>();
		}
	}

	internal abstract class BaseEntityConfiguration<TEntity> : EntityTypeConfiguration<TEntity> where TEntity : DbBase {
		internal BaseEntityConfiguration( string tableName ) {
			Map( m => {
					m.ToTable( tableName );
					m.MapInheritedProperties();
				 });


			HasKey( p => p.DbId );
			Property( p => p.DbId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );
		} 
	}

	internal class ArtistConfiguration : BaseEntityConfiguration<DbArtist> {
		internal ArtistConfiguration() :
			base( "Artists" ) {
			Ignore( p => p.Genre );
			Ignore( p => p.Rating );
			Ignore( p => p.IsUserRating );
			Ignore( p => p.DateAdded );
		}
	}

	internal class AlbumConfiguration : BaseEntityConfiguration<DbAlbum> {
		internal AlbumConfiguration() :
			base( "Albums" ) {
			Ignore( p => p.DateAdded );
			Ignore( p => p.Genre );
			Ignore( p => p.Rating );
			Ignore( p => p.IsUserRating );
		}
	}

	internal class TrackConfiguration : BaseEntityConfiguration<DbTrack> {
		internal TrackConfiguration() :
			base( "Tracks" ) {
			Ignore( p => p.DateAdded );
			Ignore( p => p.Duration );
			Ignore( p => p.Genre );
			Ignore( p => p.IsUserRating );
		}
	}

	internal class ArtworkConfiguration : BaseEntityConfiguration<DbArtwork> {
		internal ArtworkConfiguration() :
			base( "Artwork" ) { }
	}

	internal class GenreConfiguration : BaseEntityConfiguration<DbGenre> {
		internal GenreConfiguration() :
			base( "Genres" ) {
			Ignore( p => p.Genre );
			Ignore( p => p.IsUserRating );
		}
	}

	internal class InternetStreamConfiguration : BaseEntityConfiguration<DbInternetStream> {
		internal InternetStreamConfiguration() :
			base( "Streams" ) { }
	}

	internal class LyricConfiguration : BaseEntityConfiguration<DbLyric> {
		internal LyricConfiguration() :
			base( "Lyrics" ) { }
	}

	internal class PlayHistoryConfiguration : BaseEntityConfiguration<DbPlayHistory> {
		internal PlayHistoryConfiguration() :
			base( "PlayHistory" ) { }
	}

	internal class PlayListConfiguration : BaseEntityConfiguration<DbPlayList> {
		internal PlayListConfiguration() :
			base( "PlayLists" ) {
			Ignore( p => p.TrackIds );
			Ignore( p => p.IsUserRating );
		}
	}

	internal class TagConfiguration : BaseEntityConfiguration<DbTag> {
		internal TagConfiguration() :
			base( "Tags" ) { }
	}

	internal class TagAssociationConfiguration : BaseEntityConfiguration<DbTagAssociation> {
		internal TagAssociationConfiguration() :
			base( "TagAssociations" ) { }
	}

	internal class TextInfoConfiguration : BaseEntityConfiguration<DbTextInfo> {
		internal TextInfoConfiguration() :
			base( "TextInfo" ) { }
	}

	internal class TimestampConfiguration : EntityTypeConfiguration<DbTimestamp> {
		internal TimestampConfiguration() {
			ToTable( "Timestamps" );
			HasKey( p => p.ComponentId );
			Property( p => p.ComponentId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );
		}
	}

	internal class StorageFileConfiguration : BaseEntityConfiguration<StorageFile> {
		internal StorageFileConfiguration() :
			base( "Files" ) { }
	}

	internal class StorageFolderConfiguration : BaseEntityConfiguration<StorageFolder> {
		internal StorageFolderConfiguration() :
			base( "Folders" ) { }
	}

	internal class StorageSidecarConfiguration : BaseEntityConfiguration<StorageSidecar> {
		internal StorageSidecarConfiguration() :
			base( "Sidecars" ) { }
	}

	internal class FolderStrategyConfiguration : BaseEntityConfiguration<FolderStrategy> {
		internal FolderStrategyConfiguration() :
			base( "FolderStrategies" ) { }
	}

	internal class RootFolderConfiguration : BaseEntityConfiguration<RootFolder> {
		internal RootFolderConfiguration() :
			base( "Folders" ) { }
	}

	internal class VersionConfiguration : BaseEntityConfiguration<DbVersion> {
		internal VersionConfiguration() :
			base( "Version" ) {
			Ignore( p => p.DatabaseCreation );
		}
	}
}
