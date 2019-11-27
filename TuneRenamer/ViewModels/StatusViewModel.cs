using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using ReusableBits.Mvvm.VersionSpinner;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;
using ReusableBits.Ui.Controls;
using TuneRenamer.Dto;

namespace TuneRenamer.ViewModels {
    class StatusViewModel : AutomaticCommandBase, IHandle<Events.StatusEvent>, IDisposable {
		private IEventAggregator		        mEventAggregator;
        private IVersionFormatter               mVersionFormatter;
		private readonly Queue<StatusMessage>	mHoldingQueue;
		private bool							mViewAttached;

		public	string							VersionString => $"Noise Music System v{mVersionFormatter.VersionString}";

		public StatusViewModel( IEventAggregator eventAggregator, IVersionFormatter versionFormatter ) {
			mEventAggregator = eventAggregator;
            mVersionFormatter = versionFormatter;

			mHoldingQueue = new Queue<StatusMessage>();

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

        public void Dispose() {
            mEventAggregator?.Unsubscribe( this );
            mEventAggregator = null;

            mVersionFormatter?.Dispose();
            mVersionFormatter = null;
        }
    }
}
