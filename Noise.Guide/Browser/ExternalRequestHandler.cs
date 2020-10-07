using CefSharp;
using CefSharp.Handler;

namespace Noise.Guide.Browser {
    public class ExternalRequestHandler : RequestHandler {
        protected override bool OnBeforeBrowse( IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect ) {
            // Open in Default browser
            if( request.Url.StartsWith( "http" )) {
                System.Diagnostics.Process.Start( request.Url );

                return true;
            }

            return false;
        }
    }
}
