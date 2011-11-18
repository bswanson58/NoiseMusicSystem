using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class FavoriteListResult : BaseResult {
		[DataMember]
		public RoFavorite[]	Favorites { get; set; }

		public FavoriteListResult() {
			Favorites = new RoFavorite[0];
		}
	}
}
