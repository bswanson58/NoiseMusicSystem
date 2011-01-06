using Noise.Infrastructure.Dto;

namespace Noise.Core.DataBuilders {
	public interface ISummaryBuilder {
		void BuildSummaryData( DatabaseChangeSummary summary );

		void Stop();
	}
}
