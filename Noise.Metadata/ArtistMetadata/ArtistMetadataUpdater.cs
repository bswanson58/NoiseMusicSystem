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
using Raven.Client.Linq;
using ReusableBits.Threading;

namespace Noise.Metadata.ArtistMetadata {
	internal class ArtistMetadataUpdater : IMetadataUpdater {
		private const string									cBackgroundTaskName = "Artist Metadata Updateer";

		private readonly IEventAggregator						mEventAggregator;
		private readonly ILicenseManager						mLicenseManager;
		private readonly IRecurringTaskScheduler				mTaskScheduler;
		private readonly IEnumerable<IArtistMetadataProvider>	mProviders; 
		private readonly Stack<DbArtistStatus>					mPriorityUpdateList;  
		private IDocumentStore									mDocumentStore;
		private IEnumerable<DbArtistStatus>						mUpdateList;
		private IEnumerator<DbArtistStatus>						mUpdateEnumerator;

		public ArtistMetadataUpdater( IEventAggregator eventAggregator, ILicenseManager licenseManager,
									  IRecurringTaskScheduler taskScheduler, IEnumerable<IArtistMetadataProvider> providers  ) {
			mEventAggregator = eventAggregator;
			mLicenseManager = licenseManager;
			mTaskScheduler = taskScheduler;
			mProviders = providers;

			mPriorityUpdateList = new Stack<DbArtistStatus>();
		}

		public void Initialize( IDocumentStore documentStore ) {
			Condition.Requires( documentStore ).IsNotNull();

			mDocumentStore = documentStore;

			try {
				foreach( var provider in mProviders ) {
					provider.Initialize( mDocumentStore, mLicenseManager );
				}

				FillUpdateList();

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
			if( mDocumentStore != null ) {
				using( var session = mDocumentStore.OpenSession()) {
					var status = session.Load<DbArtistStatus>( DbArtistStatus.FormatStatusKey( forArtist ));

					if( status != null ) {
						lock( mPriorityUpdateList ) {
							mPriorityUpdateList.Push( status );
						}
					}
				}
			}
		}

		private void FillUpdateList() {
			using( var session = mDocumentStore.OpenSession()) {
				mUpdateList = ( from status in session.Query<DbArtistStatus>() select status ).ToArray();
			}

			mUpdateEnumerator = mUpdateList.GetEnumerator();
		}

		private void UpdateNextArtist( RecurringTask task ) {
			try {
				if( mPriorityUpdateList.Any()) {
					DbArtistStatus artistStatus;

					lock( mPriorityUpdateList ) {
						artistStatus = mPriorityUpdateList.Pop();
					}

					UpdateArtist( artistStatus );
				}
				else {
					if( mUpdateEnumerator != null ) {
						if( mUpdateEnumerator.MoveNext()) {
							UpdateArtist( mUpdateEnumerator.Current );
						}
						else {
							FillUpdateList();
						}
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "ArtistMetadataUpdater:UpdateNextArtist", ex );
			}
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
