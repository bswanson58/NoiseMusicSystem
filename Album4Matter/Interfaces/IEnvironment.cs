namespace Album4Matter.Interfaces {
    public interface IEnvironment {
        string		ApplicationName();

        string		ApplicationDirectory();
        string		LogFileDirectory();
        string		PreferencesDirectory();
    }
}
