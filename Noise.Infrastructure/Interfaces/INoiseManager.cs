namespace Noise.Infrastructure.Interfaces {
	public interface INoiseManager {
		bool			Initialize();

		IDataProvider	DataProvider { get; }

		void			Explore();
	}
}
