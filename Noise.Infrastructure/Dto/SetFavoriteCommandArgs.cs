namespace Noise.Infrastructure.Dto {
	public class SetFavoriteCommandArgs {
		public	long	ItemId { get; private set; }
		public	bool	Value { get; private set; }

		public SetFavoriteCommandArgs( long itemId, bool newValue ) {
			ItemId = itemId;
			Value = newValue;
		}
	}
}
