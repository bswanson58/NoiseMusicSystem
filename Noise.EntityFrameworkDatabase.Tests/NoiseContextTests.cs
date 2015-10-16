using System.IO;
using NUnit.Framework;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.Infrastructure.Dto;

namespace Noise.EntityFrameworkDatabase.Tests {
	[TestFixture]
	public class NoiseContextTests {
		private const string	cDatabasePath = @"D:\Noise Testing";
		private const string	cDatabaseName = "Noise Context Test Database";
		private	const string	cConnectionString = @"Data Source=(localdb)\v11.0;Integrated Security=true;MultipleActiveResultSets=True;AttachDbFileName={0}";


		[SetUp]
		public void Setup() {
			if( !Directory.Exists( cDatabasePath )) {
				Directory.CreateDirectory( cDatabasePath );
			}
		}

		[Test]
		private NoiseContext CreateSut() {
			var	databasePath = Path.Combine( cDatabasePath, cDatabasePath );
			var connectionString = string.Format( cConnectionString, databasePath );

			return( new NoiseContext( cDatabaseName, connectionString ));
		}

		[Test]
		public void CanCreateDatabase() {
			var context = CreateSut();

			Assert.IsNotNull( context );
		}

		[Test]
		public void CanAddArtist() {
			var artist = new DbArtist { Name = "The Rolling Stones" };

			using( var context = CreateSut()) {
				context.Artists.Add( artist );

				context.SaveChanges();
			}
		}
	}
}
