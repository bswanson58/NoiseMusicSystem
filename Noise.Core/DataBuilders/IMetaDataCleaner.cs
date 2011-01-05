using Noise.Infrastructure.Dto;

namespace Noise.Core.DataBuilders {
	public interface IMetaDataCleaner {
		void	CleanDatabase( DatabaseChangeSummary changes );

		void	Stop();
	}
}
