using Raven.Client;

namespace Noise.RavenDatabase.Interfaces {
	public interface IDbFactory {
		IDocumentStore	GetLibraryDatabase();
	}
}
