namespace MilkBottle.Infrastructure.Interfaces {
    public interface IEnvironment {
        string		ApplicationName();
        string      EnvironmentName();

        string		ApplicationDirectory();
        string      DatabaseDirectory();
        string		LogFileDirectory();
        string		PreferencesDirectory();
        string      MilkLibraryFolder();
        string      MilkTextureFolder();
    }
}
