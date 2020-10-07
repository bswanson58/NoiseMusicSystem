using System;
using System.Collections.Generic;
using System.IO;
using CefSharp;

namespace Noise.Guide.Browser {
    public class DesktopGuideResourceHandler : CefSharp.Handler.ResourceRequestHandler {
        private readonly Dictionary<string, string> mGuideResources;

        public DesktopGuideResourceHandler() {
            mGuideResources = new Dictionary<string, string> {
                { "/desktop/guide.css", Resources.desktop.DesktopGuide.guide },
                { "/desktop/home.html", Resources.desktop.DesktopGuide.home },
                { "/desktop/credits.html", Resources.desktop.DesktopGuide.credits },
                { "/desktop/overview.html", Resources.desktop.DesktopGuide.overview },
            };
        }

        protected override IResourceHandler GetResourceHandler( IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request ) {
            var retValue = default( IResourceHandler );
            var uri = new Uri( request.Url );

            if( mGuideResources.ContainsKey( uri.AbsolutePath )) {
                var resource = mGuideResources[uri.AbsolutePath];
                var fileExtension = Path.GetExtension( uri.AbsolutePath );

                retValue = ResourceHandler.FromString( resource, System.Text.Encoding.UTF8, mimeType: Cef.GetMimeType( fileExtension ));
            }

            return retValue;
        }
    }
}
