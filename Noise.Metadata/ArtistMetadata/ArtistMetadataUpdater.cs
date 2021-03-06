﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using ReusableBits.Interfaces;
using ReusableBits.Threading;

namespace Noise.Metadata.ArtistMetadata {
	class ArtistMetadataUpdater : IMetadataUpdater {
		private const string									cBackgroundTaskName = "Artist Metadata Updateer";

		private readonly IEventAggregator						mEventAggregator;
		private readonly INoiseLog								mLog;
		private readonly IDatabaseInfo							mDatabaseInfo;
		private readonly IArtistProvider						mArtistProvider;
		private readonly IArtistStatusProvider					mStatusProvider;
		private readonly IRecurringTaskScheduler				mTaskScheduler;
		private readonly IEnumerable<IArtistMetadataProvider>	mProviders; 
		private readonly Stack<string>							mPriorityUpdateList;  
		private IEnumerable<string>								mUpdateList;
		private IEnumerator<string>								mUpdateEnumerator;

		public ArtistMetadataUpdater( IEventAggregator eventAggregator, IDatabaseInfo databaseInfo, IArtistProvider artistProvider, IArtistStatusProvider statusProvider,
									  IRecurringTaskScheduler taskScheduler, IEnumerable<IArtistMetadataProvider> providers, INoiseLog log ) {
			mEventAggregator = eventAggregator;
			mDatabaseInfo = databaseInfo;
			mArtistProvider = artistProvider;
			mStatusProvider = statusProvider;
			mTaskScheduler = taskScheduler;
			mProviders = providers;
			mLog = log;

			mPriorityUpdateList = new Stack<string>();
		}

		public void Initialize() {
			try {
				var updateTask = new RecurringTask( UpdateNextArtist, cBackgroundTaskName );

				updateTask.TaskSchedule.StartAt( RecurringInterval.FromSeconds( 15 ))
									   .Delay( RecurringInterval.FromSeconds( 15 ));
				mTaskScheduler.AddRecurringTask( updateTask );
			}
			catch( Exception ex ) {
				mLog.LogException( "Failed to initialize", ex );
			}
		}

		public void Shutdown() {
			mUpdateEnumerator = null;
			mUpdateList = null;
			mTaskScheduler.RemoveTask( cBackgroundTaskName );
		}

		public void QueueArtistUpdate( string forArtist ) {
			lock( mPriorityUpdateList ) {
				if(!mPriorityUpdateList.Contains( forArtist )) {
					var	artistStatus = GetArtistStatus( forArtist );

					if( artistStatus != null ) {
						bool	needsUpdate = false;

						foreach( var provider in mProviders ) {
							var status = artistStatus.GetProviderStatus( provider.ProviderKey );

							needsUpdate |= !(( status != null ) &&
											 ( status.LastUpdate + status.Lifetime > DateTime.Now ));
							if( needsUpdate ) {
								mPriorityUpdateList.Push( forArtist );

								break;
							}
						}
					}
				}
			}
		}

		private void FillUpdateList() {
			if( mDatabaseInfo.IsOpen ) {
				mUpdateList = ( from artist in mArtistProvider.GetArtistList().List select artist.Name ).ToArray();

				mUpdateEnumerator = mUpdateList.GetEnumerator();
			}
		}

		private string CheckForPriorityArtist() {
			var retValue = string.Empty;

			lock( mPriorityUpdateList ) {
				if( mPriorityUpdateList.Any()) {
					retValue = mPriorityUpdateList.Pop();
				}
			}

			return( retValue );
		}
		private void UpdateNextArtist( RecurringTask task ) {
			try {
				string	artistName = CheckForPriorityArtist();

				if( string.IsNullOrWhiteSpace( artistName )) {
					if(( mUpdateEnumerator != null ) &&
					   ( mUpdateEnumerator.MoveNext())) {
						artistName = mUpdateEnumerator.Current;
					}
					else {
						FillUpdateList();
					}
				}

				if(!string.IsNullOrWhiteSpace( artistName )) {
					var	artistStatus = GetArtistStatus( artistName );

					if( artistStatus != null ) {
						UpdateArtist( artistStatus );
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "UpdateNextArtist", ex );
			}
		}

		private DbArtistStatus GetArtistStatus( string forArtist ) {
			var retValue = mStatusProvider.GetStatus( forArtist );

			if( retValue == null ) {
				retValue =  new DbArtistStatus { ArtistName = forArtist };

				mStatusProvider.Insert( retValue );
            }

			return( retValue );
		}

		private async void UpdateArtist( DbArtistStatus artistStatus ) {
			bool	updated = false;

			foreach( var provider in mProviders ) {
				var status = artistStatus.GetProviderStatus( provider.ProviderKey );
				var	shouldUpdate = !(( status != null ) &&
									 ( status.LastUpdate + status.Lifetime > DateTime.Now ));

				if( shouldUpdate ) {
					updated = await provider.UpdateArtist( artistStatus.ArtistName );

					// update the time for the next update, regardless of whether this one was successful.
                    artistStatus.SetLastUpdate( provider.ProviderKey );
                    mStatusProvider.Update( artistStatus );
				}
			}

			if( updated ) {
				mEventAggregator.PublishOnUIThread( new Events.ArtistMetadataUpdated( artistStatus.ArtistName ));
			}
		}
	}
}
