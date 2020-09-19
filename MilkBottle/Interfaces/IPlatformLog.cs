using MilkBottle.Infrastructure.Interfaces;
using Serilog.Core;
using Serilog.Events;

namespace MilkBottle.Interfaces {
    public interface IPlatformLog : IBasicLog {
        void    AddLoggingSink( ILogEventSink sink, LogEventLevel forMinimumEventLevel );
    }
}
