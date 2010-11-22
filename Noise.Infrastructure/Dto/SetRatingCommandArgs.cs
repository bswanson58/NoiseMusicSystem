using System;

namespace Noise.Infrastructure.Dto {
	public class SetRatingCommandArgs : BaseCommandArgs {
		public	Int16	Value { get; private set; }

		public SetRatingCommandArgs( long itemId, Int16 newValue ) :
			base( itemId ) {
			Value = newValue;
		}
	}
}
