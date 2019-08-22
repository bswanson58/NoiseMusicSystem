namespace Noise.Infrastructure.Dto {
	public class PlayStrategyParameterDbId : BasePlayStrategyParameters {
		public long		DbItemId { get; set; }

		public PlayStrategyParameterDbId( eTrackPlayHandlers forStrategy ) :
			base( forStrategy, typeof( PlayStrategyParameterDbId ).Name ) {
			DbItemId = Constants.cDatabaseNullOid;
		}
	}
}
