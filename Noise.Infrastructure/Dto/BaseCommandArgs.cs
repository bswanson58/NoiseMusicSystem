namespace Noise.Infrastructure.Dto {
	public class BaseCommandArgs {
		public	long	ItemId { get; private set; }

		protected BaseCommandArgs( long itemId ) {
			ItemId = itemId;
		}
	}
}
