using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Noise.Infrastructure.Dto;

namespace Noise.EntityFrameworkDatabase {
	public class NoiseContext : DbContext {
		static NoiseContext() {
			Database.SetInitializer( new DropCreateDatabaseIfModelChanges<NoiseContext>());	
		}

		public DbSet<DbArtist>	Artists { get; set; }

		protected override void OnModelCreating( DbModelBuilder modelBuilder ) {
			modelBuilder.Ignore<Artwork>();
			modelBuilder.Ignore<DbAssociatedItem>();
			modelBuilder.Ignore<DbAssociatedItemList>();
			modelBuilder.Ignore<DbArtwork>();
			modelBuilder.Ignore<DbDecadeTag>();
			modelBuilder.Ignore<DbDiscographyRelease>();
			modelBuilder.Ignore<DbGenre>();
			modelBuilder.Ignore<DbInternetStream>();
			modelBuilder.Ignore<DbLyric>();
			modelBuilder.Ignore<DbPlayHistory>();
			modelBuilder.Ignore<DbPlayList>();
			modelBuilder.Ignore<DbTag>();
			modelBuilder.Ignore<DbTagAssociation>();
			modelBuilder.Ignore<DbTextInfo>();
			modelBuilder.Ignore<DbTimestamp>();
			modelBuilder.Ignore<DbVersion>();
			modelBuilder.Ignore<StorageFile>();
			modelBuilder.Ignore<StorageFolder>();
			modelBuilder.Ignore<TextInfo>();

			modelBuilder.Entity<DbBase>().HasKey( p => p.DbId );
			modelBuilder.Entity<DbBase>().Property( p => p.DbId ).HasDatabaseGeneratedOption( DatabaseGeneratedOption.None );

			modelBuilder.Entity<DbArtist>()
				.Map( m => {
						m.MapInheritedProperties();
						m.ToTable( "Artists" );
					})
				.HasKey( p => p.DbId );

			modelBuilder.Entity<DbAlbum>()
				.Map( m => {
						m.MapInheritedProperties();
						m.ToTable( "Albums" );
					});

			modelBuilder.Entity<DbTrack>()
				.HasKey( p => p.DbId )
				.Map( m => {
						m.MapInheritedProperties();
						m.ToTable( "Tracks" );
					});
		}
	}
}
