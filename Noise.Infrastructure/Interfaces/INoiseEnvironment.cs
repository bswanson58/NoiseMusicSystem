namespace Noise.Infrastructure.Interfaces {
	public interface INoiseEnvironment {
		string		ApplicationDirectory();
		string		BackupDirectory();
		string		ConfigurationDirectory();
		string		LibraryDirectory();
		string		PreferencesDirectory();
	}
}
