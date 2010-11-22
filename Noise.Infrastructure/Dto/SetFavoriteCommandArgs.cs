namespace Noise.Infrastructure.Dto {
	public class SetFavoriteCommandArgs : BaseCommandArgs {
		public	bool	Value { get; private set; }

		public SetFavoriteCommandArgs( long itemId, bool newValue ) :
			base( itemId ) {
			Value = newValue;
		}
	}
}
