using System.Globalization;
using System.IO;
using System.Windows;
using MilkBottle.Properties;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace MilkBottle.Logging {
    class MessageBoxSink : ILogEventSink {
        readonly ITextFormatter     mTextFormatter = new MessageTemplateTextFormatter("{Level} - {Message}{NewLine}{NewLine}{Exception}", CultureInfo.CurrentUICulture );

        public void Emit( LogEvent logEvent ) {
            var renderSpace = new StringWriter();

            mTextFormatter.Format( logEvent, renderSpace );

            MessageBox.Show( renderSpace.ToString(), $"{ApplicationConstants.ApplicationName} Error" );
        }
    }
}
