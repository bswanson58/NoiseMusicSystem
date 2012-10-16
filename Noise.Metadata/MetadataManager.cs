using System;
using System.IO;
using Caliburn.Micro;
using Noise.BlobStorage.BlobStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Interfaces;
using Noise.Metadata.Support;

namespace Noise.Metadata {
	public class MetadataManager : IRequireInitialization,
								   IHandle<Events.ArtistAdded>, IHandle<Events.ArtistRemoved>,IHandle<Events.ArtistContentRequest> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IBlobStorageManager	mBlobStorageManager;
		private readonly IArtistMetadataManager	mArtistMetadataManager;
		private readonly IArtistProvider		mArtistProvider;

		public MetadataManager( ILifecycleManager lifecycleManager,  IEventAggregator eventAggregator,
								IBlobStorageManager blobStorageManager, IMetadataStorageResolver storageResolver,
								IArtistMetadataManager artistMetadataManager, IArtistProvider artistProvider ) {
			mEventAggregator = eventAggregator;
			mBlobStorageManager = blobStorageManager;
			mBlobStorageManager.SetResolver( storageResolver );
			mArtistMetadataManager = artistMetadataManager;
			mArtistProvider = artistProvider;

			lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );

			mEventAggregator.Subscribe( this );
		}

		public void Initialize() {
			var libraryPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
											Constants.CompanyName, 
											Constants.LibraryConfigurationDirectory,
											"Metadata" );

			mBlobStorageManager.Initialize( libraryPath );

			if(!mBlobStorageManager.IsOpen ) {
				if(!mBlobStorageManager.OpenStorage()) {
					mBlobStorageManager.CreateStorage();

					if(!mBlobStorageManager.OpenStorage()) {
						var ex = new ApplicationException( "MetadataManager:Blob storage could not be created." );

						NoiseLogger.Current.LogException( ex );
						throw( ex );
					}
				}
			}

			mArtistMetadataManager.Initialize( mBlobStorageManager );
		}

		public void Shutdown() {
			mArtistMetadataManager.Shutdown();
			mBlobStorageManager.CloseStorage();
		}

		public void Handle( Events.ArtistAdded args ) {
			var artist = mArtistProvider.GetArtist( args.ArtistId );

			if( artist != null ) {
				mArtistMetadataManager.ArtistMentioned( artist.Name );
			}
		}

		public void Handle( Events.ArtistRemoved args ) {
			var artist = mArtistProvider.GetArtist( args.ArtistId );

			if( artist != null ) {
				mArtistMetadataManager.ArtistForgotten( artist.Name );
			}
		}

		public void Handle( Events.ArtistContentRequest args ) {
			var artist = mArtistProvider.GetArtist( args.ArtistId );

			if( artist != null ) {
				mArtistMetadataManager.ArtistMetadataRequested( artist.Name );
			}
		}
	}
}
