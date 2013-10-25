namespace Noise.UI.Dto {
	public class NameIdPair {
		public long		Id {  get; private set;}
		public string	Name {  get; private set;}

		public NameIdPair( long id, string name ) {
			Id = id;
			Name = name;
		}
	}
}
