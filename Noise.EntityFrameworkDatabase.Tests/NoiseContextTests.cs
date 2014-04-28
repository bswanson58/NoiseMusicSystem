using System.IO;
using NUnit.Framework;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.Infrastructure.Dto;

namespace Noise.EntityFrameworkDatabase.Tests {
	[TestFixture]
	public class NoiseContextTests {
		private const string	cDatabasePath = @"D:\Noise Testing";
		private const string	cDatabaseName = "Noise Context Test Database";

		[SetUp]
		public void Setup() {
			if( !Directory.Exists( cDatabasePath )) {
				Directory.CreateDirectory( cDatabasePath );
			}
		}

		[Test]
		public void CanCreateDatabase() {
			var context = new NoiseContext( Path.Combine( cDatabasePath, cDatabaseName ));

			Assert.IsNotNull( context );
		}

		[Test]
		public void CanAddArtist() {
			var artist = new DbArtist { Name = "The Rolling Stones" };

			using( var context = new NoiseContext( Path.Combine( cDatabasePath, cDatabaseName ))) {
				context.Artists.Add( artist );

				context.SaveChanges();
			}
		}
	}
}
