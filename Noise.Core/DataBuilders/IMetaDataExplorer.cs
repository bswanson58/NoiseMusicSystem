using Noise.Infrastructure.Dto;

namespace Noise.Core.DataBuilders {
	public interface IMetaDataExplorer {
		void	BuildMetaData( DatabaseChangeSummary summary );
		void	Stop();
	}
}
