using Noise.Core.Database;

namespace Noise.Core.DataBuilders {
	public interface ISummaryBuilder {
		void BuildSummaryData( IDatabaseManager database );
	}
}
