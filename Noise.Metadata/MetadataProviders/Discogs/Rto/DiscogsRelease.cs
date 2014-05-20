using System.Web.UI.WebControls;

namespace Noise.Metadata.MetadataProviders.Discogs.Rto {
	public class DiscogsRelease {
		public	int			Id { get; set; }
		public	string		Artist { get; set; }
		public	string		Title	{ get; set; }
		public	string		Format { get; set; }
		public	string		Label { get; set; }
		public	string		Role {get; set; }
		public	string		Type { get; set; }
		public	string		Thumb { get; set; }
		public	string		Status { get; set; }
		public	string		Resource_Url { get; set; }
		public	int			Main_Release {  get; set; }
		public	int			Year {  get; set; }
	}
}
