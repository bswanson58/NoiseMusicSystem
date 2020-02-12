namespace MilkBottle.Interfaces {
    public interface IEnvironment {
        string		ApplicationName();

        string		ApplicationDirectory();
        string		LogFileDirectory();
        string		PreferencesDirectory();
        string      MilkConfigurationFile();
    }
}
