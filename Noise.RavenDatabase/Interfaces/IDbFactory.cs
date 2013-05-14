using System;
using Noise.Infrastructure.Interfaces;
using Raven.Client;

namespace Noise.RavenDatabase.Interfaces {
	public interface IDbFactory {
		IDocumentStore		GetLibraryDatabase();
		IBlobStorage		GetBlobStorage();

		IObservable<bool>	DatabaseClosed { get; }
	}
}
