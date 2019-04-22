using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Interfaces {
	public interface IContextProvider {
		IBlobStorage    	BlobStorage { get; }

		IDbContext			CreateContext();
	}
}
