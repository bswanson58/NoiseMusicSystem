﻿using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;
using ReusableBits.Ui.Controls;

namespace Noise.UI.ViewModels {
	public class StatusViewModel : AutomaticCommandBase, IHandle<Events.StatusEvent> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly Queue<StatusMessage>	mHoldingQueue;
		private bool							mViewAttached;

		public	string							Version { get; private set; }

		public StatusViewModel( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;
			mHoldingQueue = new Queue<StatusMessage>();

			Version = string.Format( "{0} v{1}", VersionInformation.ProductName, VersionInformation.Version );

			mEventAggregator.Subscribe( this );
		}

		public StatusMessage StatusMessage {
			get{ return( Get( () => StatusMessage )); }
			set{ Set( () => StatusMessage, value ); }
		}

		public void Execute_ViewAttached() {
			StatusMessage = new StatusMessage( string.Empty ); // delay a few seconds before initial message.

			StatusMessage = new StatusMessage( VersionInformation.Description );
			StatusMessage = new StatusMessage( VersionInformation.CopyrightHolder );

			while( mHoldingQueue.Any()) {
				lock( mHoldingQueue ) {
					StatusMessage = mHoldingQueue.Dequeue();
				}
			}

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
	}
}