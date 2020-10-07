using System;
using CefSharp;
using CefSharp.Handler;

namespace Noise.Guide.Browser {
    public class GuideRequestHandler : RequestHandler {
        private const string    cGuideHost = "noise";

        protected override bool OnBeforeBrowse( IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect ) {
            var retValue = false;
            var uri = new Uri( request.Url );

            if(!uri.Host.ToLower().Equals( cGuideHost )) {
                // Use external browser
                System.Diagnostics.Process.Start( request.Url );

                retValue = true;
            }

            return retValue;
        }

        protected override IResourceRequestHandler GetResourceRequestHandler( IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, 
                                                                              IRequest request, bool isNavigation, bool isDownload, string requestInitiator,
                                                                              ref bool disableDefaultHandling ) {
            var retValue = default( IResourceRequestHandler );
            var uri = new Uri( request.Url );

            // Only intercept guide urls
            if( uri.Host.ToLower().Equals( cGuideHost )) {
                retValue = new DesktopGuideResourceHandler();
            }

            // Default behaviour, url will be loaded normally.
            return retValue;
        }
    }
}
