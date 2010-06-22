namespace Noise.Infrastructure {
	public interface INoiseManager {
		bool			Initialize();

		IDataProvider	DataProvider { get; }

		void			Explore();
	}
}
