using System;

namespace Noise.Infrastructure.Dto {
	public class ExpiringContent {
		public	DateTime	HarvestDate { get; private set; }
		public	DateTime	ExpireDate	{ get; set; }

		public ExpiringContent() {
			HarvestDate = DateTime.Now.Date;
			ExpireDate = Constants.cNoExpirationDate;
		}
	}
}
