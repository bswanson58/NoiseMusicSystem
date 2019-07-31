
namespace Noise.Infrastructure.Interfaces {
	public interface IDataExchangeManager {
		bool	ExportFavorites( string fileName );
		bool	ExportStreams( string fileName );
        bool    ExportUserTags( string fileName );

		int		Import( string fileName, bool eliminateDuplicates );
	}
}
