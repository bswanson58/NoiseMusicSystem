using System;

namespace ForRent.Interfaces {
    public interface IPlatformLog {
        void	LogException( string message, Exception exception );
        void	LogMessage( string message );
    }
}
