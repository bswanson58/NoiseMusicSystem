namespace Noise.Infrastructure.Interfaces {
	public interface INoiseEnvironment {
		string		ApplicationDirectory();
		string		LibraryDirectory();
		string		BackupDirectory();
	}
}
