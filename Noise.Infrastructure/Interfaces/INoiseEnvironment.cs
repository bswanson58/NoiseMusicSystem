namespace Noise.Infrastructure.Interfaces {
	public interface INoiseEnvironment {
		string		ApplicationDirectory();
		string		ConfigurationDirectory();
		string		LibraryDirectory();
		string		BackupDirectory();
	}
}
