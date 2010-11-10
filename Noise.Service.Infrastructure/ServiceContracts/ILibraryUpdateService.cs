using System.ServiceModel;

namespace Noise.Service.Infrastructure.ServiceContracts {
	[ServiceContract( Namespace= NoiseNamespace.LibraryService )]
	public interface ILibraryUpdateService {
		[OperationContract]
		void	StartLibraryUpdate();

		[OperationContract]
		void	StopLibraryUpdate();

		[OperationContract]
		bool	IsLibraryUpdateInProgress();
	}
}
