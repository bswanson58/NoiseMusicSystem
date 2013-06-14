using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	public class PlayStrategyParameterDbId : BasePlayStrategyParameters {
		public long		DbItemId { get; set; }

		public PlayStrategyParameterDbId( ePlayExhaustedStrategy forStrategy ) :
			base( forStrategy, typeof( PlayStrategyParameterDbId ).Name ) {
			DbItemId = Constants.cDatabaseNullOid;
		}
	}
}
