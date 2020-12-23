using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Noise.RemoteClient.Droid.Support;
using Prism;
using Prism.Ioc;
using Xamarin.Forms;

namespace Noise.RemoteClient.Droid {
    [Activity(Theme = "@style/MainTheme",
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
        protected override void OnCreate(Bundle savedInstanceState) {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException; 

            Forms.SetFlags("SwipeView_Experimental");
            Forms.Init( this, savedInstanceState );

            Rg.Plugins.Popup.Popup.Init( this );

            LoadApplication(new App(new AndroidInitializer()));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs) {
            new Exception( "TaskSchedulerOnUnobservedTaskException", unobservedTaskExceptionEventArgs.Exception ).LogUnhandledException();
        }  
 
        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs) {
            new Exception( "CurrentDomainOnUnhandledException", unhandledExceptionEventArgs.ExceptionObject as Exception ).LogUnhandledException();
        }  
    } 
    
    public class AndroidInitializer : IPlatformInitializer {
        public void RegisterTypes( IContainerRegistry containerRegistry ) { }
    }
}
