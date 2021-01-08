using Android.OS;
using Noise.RemoteClient.Droid.Support;
using Noise.RemoteClient.Interfaces;
using Plugin.CurrentActivity;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformSupportAndroid))]
namespace Noise.RemoteClient.Droid.Support {
    class PlatformSupportAndroid : IPlatformSupport {

        public void SetStatusBarColor( System.Drawing.Color color ) {
            if( Build.VERSION.SdkInt < BuildVersionCodes.Lollipop )
                return;

            var activity = CrossCurrentActivity.Current.Activity;
            var window = activity?.Window;

            if( window != null ) {
                window.AddFlags( Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds );
                window.ClearFlags( Android.Views.WindowManagerFlags.TranslucentStatus );
                window.SetStatusBarColor( color.ToPlatformColor() );
            }
        }
    }
}