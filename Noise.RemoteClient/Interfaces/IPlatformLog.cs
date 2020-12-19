using System;

namespace Noise.RemoteClient.Interfaces {
    interface IPlatformLog {
        void	LogException( string message, Exception exception );
        void	LogMessage( string message );
    }
}
