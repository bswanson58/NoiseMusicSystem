using System;
using System.IO;
using System.Reflection;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Support;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Noise.RemoteClient.ViewModels {
    class AboutViewModel : BindableBase, IDisposable {
        private readonly IHostInformationProvider   mHostInformation;
        private IDisposable                         mHostStatusSubscription;
        private string                              mVersionNumber;
        private DateTime                            mBuildDate;
        private string                              mHostName;
        private string                              mLibraryName;

        public string                               BuildDate => mBuildDate.ToShortDateString();

        public  DelegateCommand                     DisplayLogs { get; }

        public AboutViewModel( IHostInformationProvider hostInformation ) {
            mHostInformation = hostInformation;

            DisplayLogs = new DelegateCommand( OnDisplayLogs );

            GatherVersionInfo();
            mHostStatusSubscription = mHostInformation.HostStatus.Subscribe( OnHostStatus );
        }

        private void GatherVersionInfo() {
            VersionNumber = VersionTracking.CurrentVersion;
            mBuildDate = new FileInfo( Assembly.GetExecutingAssembly().Location ).LastWriteTime;

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
