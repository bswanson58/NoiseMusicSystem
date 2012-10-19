using System;
using System.IO;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Interfaces;
using Raven.Client;
using Raven.Client.Embedded;

namespace Noise.Metadata {
	public class MetadataManager : IRequireInitialization,
								   IHandle<Events.ArtistAdded>, IHandle<Events.ArtistRemoved>,IHandle<Events.ArtistContentRequest> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistMetadataManager	mArtistMetadataManager;
		private readonly IArtistProvider		mArtistProvider;
		private IDocumentStore					mDocumentStore;

		public MetadataManager( ILifecycleManager lifecycleManager,  IEventAggregator eventAggregator,
								IArtistMetadataManager artistMetadataManager, IArtistProvider artistProvider ) {
			mEventAggregator = eventAggregator;
			mArtistMetadataManager = artistMetadataManager;
			mArtistProvider = artistProvider;

			lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );
		}

		public void Initialize() {
			try {
				var libraryPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
												Constants.CompanyName, 
												Constants.LibraryConfigurationDirectory,
												"Metadata" );
				mDocumentStore = new EmbeddableDocumentStore { DataDirectory = libraryPath };
				mDocumentStore.Initialize();
				mArtistMetadataManager.Initialize( mDocumentStore );

				mEventAggregator.Subscribe( this );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "MetadataManager:Initialize", ex );
			}
		}

		public void Shutdown() {
			mDocumentStore.Dispose();
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
