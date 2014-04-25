using NUnit.Framework;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.Infrastructure.Dto;

namespace Noise.EntityFrameworkDatabase.Tests {
	[TestFixture]
	public class NoiseContextTests {
		private const string	cDatabaseName = @"D:\Noise Testing\Noise Context Test Database";

		[Test]
		public void CanCreateDatabase() {
			var context = new NoiseContext( cDatabaseName );

			Assert.IsNotNull( context );
		}

		[Test]
		public void CanAddArtist() {
			var artist = new DbArtist { Name = "The Rolling Stones" };

			using( var context = new NoiseContext( cDatabaseName )) {
				context.Artists.Add( artist );

				context.SaveChanges();
			}
		}
	}
}
