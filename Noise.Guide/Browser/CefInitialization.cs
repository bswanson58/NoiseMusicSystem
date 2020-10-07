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
                CachePath = Path.Combine( mEnvironment.LogFileDirectory(), "GuideCache" ),
                LogSeverity = LogSeverity.Warning, 
                LogFile = Path.Combine( mEnvironment.LogFileDirectory(), "Guide.log" )
            };

            Cef.Initialize( settings );
            Cef.EnableHighDPISupport();
        }
    }
}
