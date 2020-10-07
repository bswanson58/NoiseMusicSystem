using System.IO;
using CefSharp;
using CefSharp.Wpf;
using Noise.Infrastructure.Interfaces;

namespace Noise.Guide.Browser {
    public class CefInitialization {
        private readonly INoiseEnvironment  mEnvironment;

        public CefInitialization( INoiseEnvironment environment ) {
            mEnvironment = environment;
        }

        public void Initialize() {
            var settings = new CefSettings {
                LogSeverity = LogSeverity.Warning, 
                LogFile = Path.Combine( mEnvironment.LogFileDirectory(), "Guide.log" )
            };

            settings.RegisterScheme( new CefCustomScheme {
                SchemeName = SchemeHandlerFactory.cSchemeName,
                SchemeHandlerFactory = new SchemeHandlerFactory()
            });

            Cef.Initialize( settings );
            Cef.EnableHighDPISupport();
        }
    }
}
