using System;

namespace TuneRenamer.Interfaces {
    public interface IPlatformLog {
        void	LogException( string message, Exception exception );
        void	LogMessage( string message );
    }
}
