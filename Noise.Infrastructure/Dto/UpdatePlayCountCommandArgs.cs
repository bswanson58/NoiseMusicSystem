namespace Noise.Infrastructure.Dto {
	public class UpdatePlayCountCommandArgs {
		public	long	ItemId { get; private set; }

		public UpdatePlayCountCommandArgs( long itemId ) {
			ItemId = itemId;
		}
	}
}
