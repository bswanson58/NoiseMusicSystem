﻿using System.Windows;

namespace MilkBottle.Events {
    public class ApplicationClosing { }

    public class StatusEvent {
        public	string			Message { get; }
        public	bool			ExtendDisplay { get; set; }

        public StatusEvent( string message ) {
            Message = message;
        }
    }

    public class InitializationComplete { }
    public class MilkConfigurationUpdated { }
    public class PresetLibraryUpdated { }

    public class PresetLibrarySwitched {
        public  string  LibraryName { get; }

        public PresetLibrarySwitched( string toLibrary ) {
            LibraryName = toLibrary;
        }
    }

    public class LaunchRequest {
        public string	Target { get; }

        public LaunchRequest( string target ) {
            Target = target;
        }
    }

    public class WindowStateChanged {
        public  WindowState     CurrentState { get; }

        public WindowStateChanged( WindowState toState ) {
            CurrentState = toState;
        }
    }
}
