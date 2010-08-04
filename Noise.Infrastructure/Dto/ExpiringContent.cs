using System;

namespace Noise.Infrastructure.Dto {
	public class ExpiringContent {
		public	long		AssociatedItem { get; private set; }
		public	ContentType	ContentType { get; private set; }
		public	DateTime	HarvestDate { get; private set; }
		public	bool		IsContentAvailable { get; set; }

		public ExpiringContent( long associatedItem, ContentType contentType ) {
			AssociatedItem = associatedItem;
			ContentType = contentType;
			IsContentAvailable = false;

			UpdateExpiration();
		}

		public void UpdateExpiration() {
			HarvestDate = DateTime.Now.Date;
		}
	}
}
