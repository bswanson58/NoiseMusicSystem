using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.VersionSpinner;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;
using ReusableBits.Ui.Controls;

namespace Noise.UI.ViewModels {
	public class StatusViewModel : AutomaticPropertyBase, IHandle<Events.StatusEvent>, IDisposable {
		private const string	cGeneralStatusTemplate = "GeneralStatusTemplate";
		private const string	cSpeechStatusTemplate  = "SpeechStatusTemplate";

		private readonly INoiseEnvironment		mNoiseEnvironment;
        private IEventAggregator		        mEventAggregator;
        private IVersionFormatter               mVersionFormatter;
		private readonly Queue<StatusMessage>	mHoldingQueue;
        private readonly string					mCompileDate;
		private bool							mViewAttached;
		public	string							VersionString => $"Noise Music System v{mVersionFormatter.VersionString}{mCompileDate}";

		public	DelegateCommand					OpenDataFolder { get; }
		public	DelegateCommand					ViewAttached { get; }

		public StatusViewModel( IEventAggregator eventAggregator, INoiseEnvironment noiseEnvironment, IVersionFormatter versionFormatter, IPreferences preferences ) {
			mEventAggregator = eventAggregator;
			mNoiseEnvironment = noiseEnvironment;
            mVersionFormatter = versionFormatter;

			mHoldingQueue = new Queue<StatusMessage>();

			OpenDataFolder = new DelegateCommand( OnOpenDataFolder );
			ViewAttached = new DelegateCommand( OnViewAttached );

			var pref = preferences.Load<UserInterfacePreferences>();
			if( pref.DisplayBuildDate ) {
                var compileDate = ApplicationInformation.CompileDate;

                mCompileDate = $" - {compileDate.Month:D2}/{compileDate.Day:D2}/{compileDate.Year % 100:D2}";
            }
			else {
				mCompileDate = String.Empty;
            }

            mVersionFormatter.SetVersion( VersionInformation.Version );
            mVersionFormatter.DisplayLevel = VersionLevel.Build;
            mVersionFormatter.PropertyChanged += VersionFormatterOnPropertyChanged;

			mEventAggregator.Subscribe( this );
		}

        private void VersionFormatterOnPropertyChanged( object sender, PropertyChangedEventArgs e ) {
            RaisePropertyChanged( e.PropertyName );
        }

        public StatusMessage StatusMessage {
			get{ return( Get( () => StatusMessage )); }
			set{ Set( () => StatusMessage, value ); }
		}

		private void OnViewAttached() {
			StatusMessage = new StatusMessage( string.Empty ); // delay a few seconds before initial message.

			StatusMessage = new StatusMessage( VersionInformation.Description, cGeneralStatusTemplate );
			StatusMessage = new StatusMessage( VersionInformation.CopyrightHolder, cGeneralStatusTemplate );

			lock( mHoldingQueue ) {
				while( mHoldingQueue.Any()) {
					StatusMessage = mHoldingQueue.Dequeue();
				}
			}

            mVersionFormatter.StartFormatting();
			mViewAttached = true;
		}

		public void Handle( Events.StatusEvent status ) {
			var message = new StatusMessage( status.Message, SelectTemplate( status )) { ExtendActiveDisplay = status.ExtendDisplay };

			if( mViewAttached ) {
				StatusMessage = message;
			}
			else {
				lock( mHoldingQueue ) {
					mHoldingQueue.Enqueue( message );
				}
			}
		}

		private string SelectTemplate( Events.StatusEvent status ) {
			var retValue = cGeneralStatusTemplate;

			if( status.StatusType == Events.StatusEventType.Speech ) {
				retValue = cSpeechStatusTemplate;
			}

			return( retValue );
		}

		private void OnOpenDataFolder() {
            mEventAggregator.PublishOnUIThread( new Events.LaunchRequest( mNoiseEnvironment.ApplicationDirectory()));
        }

        public void Dispose() {
            mEventAggregator?.Unsubscribe( this );
            mEventAggregator = null;

            mVersionFormatter?.Dispose();
            mVersionFormatter = null;
        }
    }
}
