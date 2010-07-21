﻿using Microsoft.Practices.Composite.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class TrackViewNode : ViewModelBase {
		private readonly IEventAggregator	mEventAggregator;
		private bool						mIsSelected;
		public	DbTrack						Track { get; private set; }

		public TrackViewNode( IEventAggregator eventAggregator, DbTrack track ) {
			mEventAggregator = eventAggregator;
			Track = track;
		}

		public bool IsSelected {
			get { return mIsSelected; }
			set {
				if( value != mIsSelected ) {
					mIsSelected = value;

					RaisePropertyChanged( () => IsSelected );

					if( mIsSelected ) {
						mEventAggregator.GetEvent<Events.TrackSelected>().Publish( Track );
					}
				}
			}
		}

		public void Execute_PlayTrack( object sender ) {
			var track = sender as DbTrack;

			if( track != null ) {
				mEventAggregator.GetEvent<Events.TrackPlayRequested>().Publish( track );
			}
		}
	}
}
