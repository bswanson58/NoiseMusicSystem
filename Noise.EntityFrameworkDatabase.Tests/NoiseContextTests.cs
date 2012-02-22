using NUnit.Framework;
using Noise.Infrastructure.Dto;

namespace Noise.EntityFrameworkDatabase.Tests {
	[TestFixture]
	public class NoiseContextTests {

		[Test]
		public void CanCreateDatabase() {
			var artist = new DbArtist { };

			using( var context = new NoiseContext()) {
				context.Artists.Add( artist );
				context.SaveChanges();
			}
		}
	}
}
