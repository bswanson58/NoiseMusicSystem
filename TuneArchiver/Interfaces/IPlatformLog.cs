using System;

namespace TuneArchiver.Interfaces {
    public interface IPlatformLog {
        void	LogException( string message, Exception exception );
        void	LogMessage( string message );
    }
}
