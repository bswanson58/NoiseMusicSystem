namespace Noise.Infrastructure.Dto {
	public class UpdatePlayCountCommandArgs : BaseCommandArgs {
		public UpdatePlayCountCommandArgs( long itemId ) :
			base( itemId ) {
		}
	}
}
