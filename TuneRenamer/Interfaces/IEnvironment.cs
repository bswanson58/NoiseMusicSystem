﻿namespace TuneRenamer.Interfaces {
    public interface IEnvironment {
        string		ApplicationName();

        string		ApplicationDirectory();
        string		LogFileDirectory();
        string		PreferencesDirectory();
    }
}
