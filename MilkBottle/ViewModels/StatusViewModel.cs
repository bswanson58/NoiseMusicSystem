using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using ReusableBits.Mvvm.VersionSpinner;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;
using ReusableBits.Ui.Controls;

namespace MilkBottle.ViewModels {
    class StatusViewModel : AutomaticCommandBase, IHandle<Events.StatusEvent>, IDisposable {
		private IEventAggregator		        mEventAggregator;
		private readonly IEnvironment			mEnvironment;
        private IVersionFormatter               mVersionFormatter;
		private readonly Queue<StatusMessage>	mHoldingQueue;
        private readonly string					mCompileDate;
		private bool							mViewAttached;

		public	string							VersionString => $"Noise Music System v{mVersionFormatter.VersionString}{mCompileDate}";

		public StatusViewModel( IEventAggregator eventAggregator, IEnvironment environment, IVersionFormatter versionFormatter, IPreferences preferences ) {
			mEventAggregator = eventAggregator;
			mEnvironment = environment;
            mVersionFormatter = versionFormatter;

			mHoldingQueue = new Queue<StatusMessage>();

            var pref = preferences.Load<MilkPreferences>();
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

		public void Execute_ViewAttached() {
			StatusMessage = new StatusMessage( string.Empty ); // delay a few seconds before initial message.

			StatusMessage = new StatusMessage( VersionInformation.Description );
			StatusMessage = new StatusMessage( VersionInformation.CopyrightHolder );

			lock( mHoldingQueue ) {
				while( mHoldingQueue.Any()) {
					StatusMessage = mHoldingQueue.Dequeue();
				}
			}

            mVersionFormatter.StartFormatting();
			mViewAttached = true;
		}

		public void Handle( Events.StatusEvent status ) {
			var message = new StatusMessage( status.Message ) { ExtendActiveDisplay = status.ExtendDisplay };

			if( mViewAttached ) {
				StatusMessage = message;
			}
			else {
				lock( mHoldingQueue ) {
					mHoldingQueue.Enqueue( message );
				}
			}
		}

        public void Execute_OpenDataFolder() {
            mEventAggregator.PublishOnUIThread( new Events.LaunchRequest( mEnvironment.ApplicationDirectory()));
        }

        public void Dispose() {
            mEventAggregator?.Unsubscribe( this );
            mEventAggregator = null;

            mVersionFormatter?.Dispose();
            mVersionFormatter = null;
        }
    }
}
