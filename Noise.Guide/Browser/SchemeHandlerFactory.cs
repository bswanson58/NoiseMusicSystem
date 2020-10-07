using System;
using System.Collections.Generic;
using System.IO;
using CefSharp;

namespace Noise.Guide.Browser {
    class SchemeHandlerFactory : ISchemeHandlerFactory {
        public const string         cSchemeName = "noise";

        private readonly Dictionary<string, IDictionary<string, string>>    mGuideResources;

        public SchemeHandlerFactory() {
            mGuideResources = new Dictionary<string, IDictionary<string, string>>();

            // app resources should be based upon the running application
            var appResources = new Dictionary<string, string> {
                { "/home.html", Resources.Desktop.DesktopResources.home },
                { "/credits.html", Resources.Desktop.DesktopResources.credits }
            };

            var desktopResources = new Dictionary<string, string> {
                { "/credits.html", Resources.Desktop.DesktopResources.credits }
            };

            var commonResources = new Dictionary<string, string> {
                { "/overview.html", Resources.Common.CommonResources.overview }
            };

            mGuideResources.Add( "app", appResources );
            mGuideResources.Add( "desktop", desktopResources );
            mGuideResources.Add( "common", commonResources );
        }

        public IResourceHandler Create( IBrowser browser, IFrame frame, string schemeName, IRequest request ) {
            var retValue = default( IResourceHandler );
            var uri = new Uri( request.Url );

            if( mGuideResources.ContainsKey( uri.Host )) {
                var resources = mGuideResources[uri.Host];
                var resourceName = uri.AbsolutePath;

                if( resources.ContainsKey( resourceName )) {
                    var resource = resources[resourceName];
                    var fileExtension = Path.GetExtension( resourceName );

                    retValue = ResourceHandler.FromString( resource, System.Text.Encoding.UTF8, mimeType: Cef.GetMimeType( fileExtension ));
                }
            }

            return retValue;
        }
    }
}
