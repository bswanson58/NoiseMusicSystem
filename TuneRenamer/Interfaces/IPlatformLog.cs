using System;
using Serilog.Core;
using Serilog.Events;

namespace TuneRenamer.Interfaces {
    public interface IPlatformLog {
        void	LogException( string message, Exception exception );
        void	LogMessage( string message );

        void    AddLoggingSink( ILogEventSink sink, LogEventLevel forMinimumEventLevel );
    }
}
