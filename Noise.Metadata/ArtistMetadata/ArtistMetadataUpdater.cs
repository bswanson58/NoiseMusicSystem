using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Raven.Client;
using ReusableBits.Threading;

namespace Noise.Metadata.ArtistMetadata {
	internal class ArtistMetadataUpdater : IMetadataUpdater {
		private const string									cBackgroundTaskName = "Artist Metadata Updateer";

		private readonly IEventAggregator						mEventAggregator;
		private readonly IDatabaseInfo							mDatabaseInfo;
		private readonly IArtistProvider						mArtistProvider;
		private readonly IRecurringTaskScheduler				mTaskScheduler;
		private readonly IEnumerable<IArtistMetadataProvider>	mProviders; 
		private readonly Stack<string>							mPriorityUpdateList;  
		private IDocumentStore									mDocumentStore;
		private IEnumerable<string>								mUpdateList;
		private IEnumerator<string>								mUpdateEnumerator;

		public ArtistMetadataUpdater( IEventAggregator eventAggregator, IDatabaseInfo databaseInfo, IArtistProvider artistProvider,
									  IRecurringTaskScheduler taskScheduler, IEnumerable<IArtistMetadataProvider> providers  ) {
			mEventAggregator = eventAggregator;
			mDatabaseInfo = databaseInfo;
			mArtistProvider = artistProvider;
			mTaskScheduler = taskScheduler;
			mProviders = providers;

			mPriorityUpdateList = new Stack<string>();
		}

		public void Initialize( IDocumentStore documentStore ) {
			Condition.Requires( documentStore ).IsNotNull();

			mDocumentStore = documentStore;

			try {
				foreach( var provider in mProviders ) {
					provider.Initialize( mDocumentStore );
				}

				var updateTask = new RecurringTask( UpdateNextArtist, cBackgroundTaskName );

				updateTask.TaskSchedule.StartAt( RecurringInterval.FromSeconds( 15 ))
									   .Delay( RecurringInterval.FromSeconds( 10 ));
				mTaskScheduler.AddRecurringTask( updateTask );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "ArtistMetadataUpdater:Initialize", ex );
			}
		}

		public void Shutdown() {
			mUpdateEnumerator = null;
			mUpdateList = null;
			mTaskScheduler.RemoveTask( cBackgroundTaskName );

			foreach( var provider in mProviders ) {
				provider.Shutdown();
			}

			mDocumentStore = null;
		}

		public void QueueArtistUpdate( string forArtist ) {
			if(( mDocumentStore != null ) &&
			   (!mPriorityUpdateList.Contains( forArtist ))) {
				DbArtistStatus	artistStatus = GetArtistStatus( forArtist );

				if( artistStatus != null ) {
					bool	needsUpdate = false;

					foreach( var provider in mProviders ) {
						var status = artistStatus.GetProviderStatus( provider.ProviderKey );

						needsUpdate |= !(( status != null ) &&
										 ( status.LastUpdate + status.Lifetime > DateTime.Now ));
						if( needsUpdate ) {
							break;
						}
					}

					if( needsUpdate ) {
						lock( mPriorityUpdateList ) {
							mPriorityUpdateList.Push( forArtist );
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

		private void UpdateNextArtist( RecurringTask task ) {
			try {
				string	artistName = string.Empty;

				if( mPriorityUpdateList.Any()) {
					lock( mPriorityUpdateList ) {
						artistName = mPriorityUpdateList.Pop();
					}
				}
				else {
					if(( mUpdateEnumerator != null ) &&
					   ( mUpdateEnumerator.MoveNext())) {
						artistName = mUpdateEnumerator.Current;
					}
					else {
						FillUpdateList();
					}
				}

				if(!string.IsNullOrWhiteSpace( artistName )) {
					DbArtistStatus	artistStatus = GetArtistStatus( artistName );

					if( artistStatus != null ) {
						UpdateArtist( artistStatus );
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "ArtistMetadataUpdater:UpdateNextArtist", ex );
			}
		}

		private DbArtistStatus GetArtistStatus( string forArtist ) {
			var retValue = default( DbArtistStatus );

			if(( mDocumentStore != null ) &&
			   (!mDocumentStore.WasDisposed )) {
				using( var session = mDocumentStore.OpenSession()) {
					retValue = session.Load<DbArtistStatus>( DbArtistStatus.FormatStatusKey( forArtist ));

					if( retValue == null ) {
						retValue = new DbArtistStatus { ArtistName = forArtist };

						session.Store( retValue );
						session.SaveChanges();
					}
				}
			}

			return( retValue );
		}


		private void UpdateArtist( DbArtistStatus artistStatus ) {
			bool	updated = false;

			foreach( var provider in mProviders ) {
				var status = artistStatus.GetProviderStatus( provider.ProviderKey );
				var	shouldUpdate = !(( status != null ) &&
									 ( status.LastUpdate + status.Lifetime > DateTime.Now ));

				if( shouldUpdate ) {
					provider.UpdateArtist( artistStatus.ArtistName );

					using( var session = mDocumentStore.OpenSession()) {
						artistStatus.SetLastUpdate( provider.ProviderKey );

						session.Store( artistStatus );
						session.SaveChanges();
					}

					updated = true;
				}
			}

			if( updated ) {
					mEventAggregator.Publish( new Events.ArtistMetadataUpdated( artistStatus.ArtistName ));
			}
		}
	}
}
