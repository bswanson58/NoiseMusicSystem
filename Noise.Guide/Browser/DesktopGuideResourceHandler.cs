using System;
using System.Collections.Generic;
using System.IO;
using CefSharp;

namespace Noise.Guide.Browser {
    public class DesktopGuideResourceHandler : CefSharp.Handler.ResourceRequestHandler {
        private readonly Dictionary<string, string> mStringResources;

        public DesktopGuideResourceHandler() {
            mStringResources = new Dictionary<string, string> {
                { "/guide.css", Resources.desktop.DesktopGuide.guide },
                { "/home.html", Resources.desktop.DesktopGuide.home },
                { "/credits.html", Resources.desktop.DesktopGuide.credits },
                { "/overview.html", Resources.desktop.DesktopGuide.overview },
            };
        }

        protected override IResourceHandler GetResourceHandler( IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request ) {
            var retValue = default( IResourceHandler );
            var uri = new Uri( request.Url );

            if( mStringResources.ContainsKey( uri.AbsolutePath )) {
                var resource = mStringResources[uri.AbsolutePath];
                var fileExtension = Path.GetExtension( uri.AbsolutePath );

                retValue = ResourceHandler.FromString( resource, System.Text.Encoding.UTF8, mimeType: Cef.GetMimeType( fileExtension ));
            }

            return retValue;
        }
    }
}
