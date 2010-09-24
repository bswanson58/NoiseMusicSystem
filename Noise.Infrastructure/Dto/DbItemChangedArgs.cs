namespace Noise.Infrastructure.Dto {
	public enum DbItemChanged {
		Insert,
		Update,
		Delete
	}

	public class DbItemChangedArgs {
		public	DbBase			Item { get; private set; }
		public	DbItemChanged	Change { get; private set; }

		public DbItemChangedArgs( DbBase item, DbItemChanged change ) {
			Item = item;
			Change = change;
		}
	}
}
