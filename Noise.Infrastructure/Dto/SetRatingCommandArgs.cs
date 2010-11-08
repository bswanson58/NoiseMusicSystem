using System;

namespace Noise.Infrastructure.Dto {
	public class SetRatingCommandArgs {
		public	long	ItemId { get; private set; }
		public	Int16	Value { get; private set; }

		public SetRatingCommandArgs( long itemId, Int16 newValue ) {
			ItemId = itemId;
			Value = newValue;
		}
	}
}
