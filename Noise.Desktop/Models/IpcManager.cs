﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Platform;

namespace Noise.Desktop.Models {
    class IpcManager : IIpcManager, IRequireInitialization {
        private const int                       cHeartbeatSeconds = 30;

        private readonly IIpcHandler            mIpcHandler;
        private readonly IPlaybackPublisher     mPlaybackPublisher;
        private readonly IPreferences           mPreferences;
        private readonly INoiseWindowManager    mWindowManager;
        private readonly DispatcherTimer        mIpcTimer;
        private readonly JavaScriptSerializer   mSerializer;
        private IDisposable                     mPlaybackPublisherSubscription;
        private string                          mIpcIcon;

        public  ObservableCollection<UiCompanionApp>    CompanionApplications { get; }

        public IpcManager( ILifecycleManager lifecycleManager, INoiseWindowManager windowManager, IPreferences preferences,
                           IPlaybackPublisher playbackPublisher, IIpcHandler ipcHandler ) {
            mWindowManager = windowManager;
            mPreferences = preferences;
            mIpcHandler = ipcHandler;
            mPlaybackPublisher = playbackPublisher;

            CompanionApplications = new ObservableCollection<UiCompanionApp>();
            mSerializer = new JavaScriptSerializer();

            mIpcTimer = new DispatcherTimer( DispatcherPriority.Background ) { Interval = TimeSpan.FromSeconds( cHeartbeatSeconds )};
            mIpcTimer.Tick += OnIpcTimer;

            lifecycleManager.RegisterForInitialize( this );
            lifecycleManager.RegisterForShutdown( this );
        }

        public void Initialize() {
            var iconStream = Application.GetResourceStream( new Uri( "pack://application:,,,/Noise.Desktop;component/Resources/ApplicationIcon.xaml" ));
            if( iconStream != null ) {
                var reader = new StreamReader( iconStream.Stream );

                mIpcIcon = reader.ReadToEnd();
            }

            mIpcHandler.Initialize( Constants.ApplicationName, Constants.EcosystemName, OnIpcMessage );

            mPlaybackPublisher.PlaybackEvents.Subscribe( OnPlaybackEvent );

            mIpcTimer.Start();
        }

        public void Shutdown() {
            mIpcTimer.Stop();

            mPlaybackPublisherSubscription?.Dispose();
            mPlaybackPublisherSubscription = null;
        }

        private void OnPlaybackEvent( PlaybackEvent args ) {
            var json = mSerializer.Serialize( args );

            mIpcHandler.BroadcastMessage( NoiseIpcSubject.cPlaybackEvent, json );
        }

        private void OnIpcMessage( BaseIpcMessage message ) {
			Execute.OnUIThread( () => {
                switch( message.Subject ) {
                    case NoiseIpcSubject.cCompanionApplication:
                        AddCompanionApp( message );
                        break;

                    case NoiseIpcSubject.cActivateApplication:
                        mWindowManager.ActivateShell();
                        break;
                }
            });
        }

        private void AddCompanionApp( BaseIpcMessage message ) {
            var companionApp = mSerializer.Deserialize<CompanionApplication>( message.Body );

            if( companionApp != null ) {
                var existingApp = CompanionApplications.FirstOrDefault( a => a.ApplicationName.Equals( companionApp.Name ));

                if( existingApp == null ) {
                    using( var stream = new MemoryStream( Encoding.UTF8.GetBytes( companionApp.Icon ))) {
                        var icon = XamlReader.Load( stream ) as FrameworkElement;

                        CompanionApplications.Add( new UiCompanionApp( companionApp.Name, icon, $"Switch to {companionApp.Name}", OnCompanionAppRequest ));
                    }
                }
                else {
                    existingApp.UpdateHeartbeat();
                }
            }
        }

        private void OnCompanionAppRequest( UiCompanionApp app ) {
            var message = new ActivateApplication();
            var json = mSerializer.Serialize( message );

            mIpcHandler.SendMessage( app.ApplicationName, NoiseIpcSubject.cActivateApplication, json );

            var preferences = mPreferences.Load<UserInterfacePreferences>();

            if( preferences.MinimizeOnSwitchToCompanionApp ) {
                mWindowManager.DeactivateShell();
            }
        }

        private void OnIpcTimer( object sender, EventArgs args ) {
            var message = new CompanionApplication( Constants.ApplicationName, mIpcIcon );
            var json = mSerializer.Serialize( message );

            mIpcHandler.BroadcastMessage( NoiseIpcSubject.cCompanionApplication, json );

            var appList = new List<UiCompanionApp>( CompanionApplications );

            appList.ForEach( a => {
                var expiration = DateTime.Now - TimeSpan.FromSeconds( cHeartbeatSeconds * 3 );

                if( a.LastHeartbeat < expiration ) {
                    CompanionApplications.Remove( a );
                }
            });
        }
    }
}
