namespace Noise.Infrastructure.Interfaces {
	public interface INoiseEnvironment {
		string		ApplicationName();
		string		ApplicationDirectory();
		string		BackupDirectory();
		string		ConfigurationDirectory();
		string		LibraryDirectory();
		string		LogFileDirectory();
		string		PreferencesDirectory();
	}
}
