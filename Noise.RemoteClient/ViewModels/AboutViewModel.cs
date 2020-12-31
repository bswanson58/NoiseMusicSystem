using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Platform;
using Noise.RemoteClient.Support;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace Noise.RemoteClient.ViewModels {
    class AboutViewModel : BindableBase, IDisposable {
        private readonly IHostInformationProvider   mHostInformation;
        private readonly IPreferences               mPreferences;
        private IDisposable                         mHostStatusSubscription;
        private string                              mVersionNumber;
        private DateTime                            mBuildDate;
        private string                              mHostName;
        private string                              mLibraryName;
        private ThemeResource                       mFontResource;
        private ThemeResource                       mThemeResource;

        public string                               BuildDate => mBuildDate.ToShortDateString();

        public  ObservableCollection<ThemeResource> FontResources { get; }
        public  ObservableCollection<ThemeResource> ThemeResources { get; }

        public  DelegateCommand                     DisplayLogs { get; }

        public AboutViewModel( IHostInformationProvider hostInformation, IPreferences preferences ) {
            mPreferences = preferences;
            mHostInformation = hostInformation;

            FontResources = new ObservableCollection<ThemeResource>( ThemeCatalog.FontThemes );
            mFontResource = FontResources.FirstOrDefault( r => r.ResourceId.Equals( mPreferences.Get( PreferenceNames.ApplicationFont, ThemeCatalog.DefaultFont )));

            ThemeResources = new ObservableCollection<ThemeResource>( ThemeCatalog.ThemeResources );
            mThemeResource = ThemeResources.FirstOrDefault( r => r.ResourceId.Equals( mPreferences.Get( PreferenceNames.ApplicationTheme, ThemeCatalog.DefaultTheme )));

            DisplayLogs = new DelegateCommand( OnDisplayLogs );

            GatherVersionInfo();
            mHostStatusSubscription = mHostInformation.HostStatus.Subscribe( OnHostStatus );
        }

        public ThemeResource CurrentFont {
            get => mFontResource;
            set => SetProperty( ref mFontResource, value, OnFontResourceChanged );
        }

        private void OnFontResourceChanged() {
            if( mFontResource != null ) {
                ThemeManager.ChangeFontResource( mFontResource );

                mPreferences.Set( PreferenceNames.ApplicationFont, mFontResource.ResourceId );
            }
        }

        public ThemeResource CurrentTheme {
            get => mThemeResource;
            set => SetProperty( ref mThemeResource, value, OnThemeChanged );
        }

        private void OnThemeChanged() {
            if( mThemeResource != null ) {
                ThemeManager.ChangeThemeResource( mThemeResource );

                mPreferences.Set( PreferenceNames.ApplicationTheme, mThemeResource.ResourceId );
            }
        }

        private void GatherVersionInfo() {
            VersionNumber = VersionTracking.CurrentVersion;
            mBuildDate = BuildTimeStamp.GetTimestamp();

            RaisePropertyChanged( nameof( BuildDate ));
        }

        private async void OnHostStatus( HostStatusResponse status ) {
            if( status.LibraryOpen ) {
                LibraryName = status.LibraryName;

                var hostInfo = await mHostInformation.GetHostInformation();

                HostName = hostInfo.HostName;
            }
            else {
                LibraryName = String.Empty;
            }
        }

        public string VersionNumber {
            get => mVersionNumber;
            set => SetProperty( ref mVersionNumber, value );
        }

        public string HostName {
            get => mHostName;
            set => SetProperty( ref mHostName, value );
        }

        public string LibraryName {
            get => mLibraryName;
            set => SetProperty( ref mLibraryName, value );
        }

        private async void OnDisplayLogs() {
            // route to the shell content page, don't push it on the navigation stack.
            await Shell.Current.GoToAsync( $"///{RouteNames.LogFileDisplay}" );
        }

        public void Dispose() {
            mHostStatusSubscription?.Dispose();
            mHostStatusSubscription = null;
        }
    }
}
