using System;

namespace Album4Matter.Interfaces {
    public interface IPlatformLog {
        void	LogException( string message, Exception exception );
        void	LogMessage( string message );
    }
}
