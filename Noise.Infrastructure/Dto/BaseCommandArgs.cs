namespace Noise.Infrastructure.Dto {
	public class BaseCommandArgs {
		public	long	ItemId { get; }

		protected BaseCommandArgs( long itemId ) {
			ItemId = itemId;
		}
	}
}
