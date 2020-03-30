using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using Caliburn.Micro;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Properties;
using ReusableBits.Platform;

namespace MilkBottle.Models {
    class IpcManager : IIpcManager {
        private const int                           cHeartbeatSeconds = 30;

        private readonly IIpcHandler                mIpcHandler;
        private readonly IPlatformLog               mLog;
        private readonly DispatcherTimer            mIpcTimer;
        private readonly JavaScriptSerializer       mSerializer;
        private readonly string                     mIpcIcon;
        private readonly List<ActiveCompanionApp>   mCompanionApplications;

        private readonly BehaviorSubject<IEnumerable<ActiveCompanionApp>>   mCompanionAppSubject;
        public  IObservable<IEnumerable<ActiveCompanionApp>>                CompanionAppsUpdate => mCompanionAppSubject.AsObservable();

        private readonly BehaviorSubject<PlaybackEvent>                     mPlaybackEventSubject;
        public  IObservable<PlaybackEvent>                                  OnPlaybackEvent => mPlaybackEventSubject.AsObservable();

        private readonly Subject<bool>                                      mActivationSubject;
        public  IObservable<bool>                                           OnActivationRequest => mActivationSubject.AsObservable();
        
        public IpcManager( IIpcHandler ipcHandler, IPlatformLog log ) {
            mIpcHandler = ipcHandler;
            mLog = log;

            mCompanionApplications = new List<ActiveCompanionApp>();
            mCompanionAppSubject = new BehaviorSubject<IEnumerable<ActiveCompanionApp>>( new List<ActiveCompanionApp>());

            mActivationSubject = new Subject<bool>();

            mPlaybackEventSubject = new BehaviorSubject<PlaybackEvent>( new PlaybackEvent());

            var iconStream = Application.GetResourceStream( new Uri( "pack://application:,,,/MilkBottle;component/Resources/ApplicationIcon.xaml" ));
            if( iconStream != null ) {
                var reader = new StreamReader( iconStream.Stream );

                mIpcIcon = reader.ReadToEnd();
            }

            mIpcHandler.Initialize( ApplicationConstants.ApplicationName, ApplicationConstants.EcosystemName, OnIpcMessage );
            mSerializer = new JavaScriptSerializer();

            mIpcTimer = new DispatcherTimer( DispatcherPriority.Background ) { Interval = TimeSpan.FromSeconds( cHeartbeatSeconds )};
            mIpcTimer.Tick += OnIpcTimer;
            mIpcTimer.Start();
        }

        private void OnIpcMessage( BaseIpcMessage message ) {
            Execute.OnUIThread( () => {
                switch( message.Subject ) {
                    case NoiseIpcSubject.cCompanionApplication:
                        AddCompanionApp( message );
                        break;

                    case NoiseIpcSubject.cActivateApplication:
                        mActivationSubject.OnNext( true );
                        break;

                    case NoiseIpcSubject.cPlaybackEvent:
                        PublishPlaybackEvent( message );
                        break;
                }
            });
        }

        private void AddCompanionApp( BaseIpcMessage message ) {
            try {
                var companionApp = mSerializer.Deserialize<CompanionApplication>( message.Body );

                if( companionApp != null ) {
                    var existingApp = mCompanionApplications.FirstOrDefault( a => a.ApplicationName.Equals( companionApp.Name ));

                    if( existingApp == null ) {
                        using( var stream = new MemoryStream( Encoding.UTF8.GetBytes( companionApp.Icon ))) {
                            var icon = XamlReader.Load( stream ) as FrameworkElement;

                            mCompanionApplications.Add( new ActiveCompanionApp( companionApp.Name, icon )); 
                            mCompanionAppSubject.OnNext( mCompanionApplications );
                        }
                    }
                    else {
                        existingApp.UpdateHeartbeat();
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "ShellViewModel.AddCompanionApp", ex );
            }
        }

        public void ActivateApplication( string applicationName ) {
            try {
                var message = new ActivateApplication();
                var json = mSerializer.Serialize( message );

                mIpcHandler.SendMessage( applicationName, NoiseIpcSubject.cActivateApplication, json );
            }
            catch( Exception ex ) {
                mLog.LogException( "ShellViewModel.OnCompanionAppRequest", ex );
            }
        }

        private void PublishPlaybackEvent( BaseIpcMessage message ) {
            try {
                var playbackEvent = mSerializer.Deserialize<PlaybackEvent>( message.Body );

                if( playbackEvent != null ) {
                    mPlaybackEventSubject.OnNext( playbackEvent );
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "ShellViewModel.PublishPlaybackEvent", ex );
            }
        }

        private void OnIpcTimer( object sender, EventArgs args ) {
            try {
                var message = new CompanionApplication( ApplicationConstants.ApplicationName, mIpcIcon );
                var json = mSerializer.Serialize( message );

                mIpcHandler.BroadcastMessage( NoiseIpcSubject.cCompanionApplication, json );

                var appList = new List<ActiveCompanionApp>( mCompanionApplications );

                appList.ForEach( a => {
                    var expiration = DateTime.Now - TimeSpan.FromSeconds( cHeartbeatSeconds * 3 );

                    if( a.LastHeartbeat < expiration ) {
                        mCompanionApplications.Remove( a );
                    }
                });

                mCompanionAppSubject.OnNext( mCompanionApplications );
            }
            catch( Exception ex ) {
                mLog.LogException( "IpcManager.OnIpcTimer", ex );
            }
        }

        public void Dispose() {
            mActivationSubject.OnCompleted();
            mCompanionAppSubject.OnCompleted();
            mPlaybackEventSubject.OnCompleted();

            mActivationSubject.Dispose();
            mCompanionAppSubject.Dispose();
            mPlaybackEventSubject.Dispose();

            mIpcTimer.Stop();
        }
    }
}
