using System;

namespace Noise.Core.MetaData {
	public class ExpiringContent {
		public	DateTime	HarvestDate { get; private set; }
		public	DateTime	ExpireDate	{ get; set; }

		public ExpiringContent() {
			HarvestDate = DateTime.Now.Date;
		}
	}
}
