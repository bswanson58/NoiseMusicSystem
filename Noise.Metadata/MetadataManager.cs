using System.Collections.Generic;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Interfaces;

namespace Noise.Metadata {
	public class MetadataManager : IRequireInitialization, IMetadataManager,
								   IHandle<Events.ArtistAdded>, IHandle<Events.ArtistRemoved> {
		private readonly IEventAggregator				mEventAggregator;
		private readonly INoiseLog						mLog;
		private readonly IArtistMetadataManager			mArtistMetadataManager;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IEnumerable<IMetadataUpdater>	mUpdaters;
		private readonly IDatabaseProvider				mDatabaseProvider;

		public MetadataManager( ILifecycleManager lifecycleManager,  IEventAggregator eventAggregator, IDatabaseProvider databaseProvider,
								IMetadataUpdater[] updaters, IArtistMetadataManager artistMetadataManager, IArtistProvider artistProvider, INoiseLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mUpdaters = updaters;
			mArtistMetadataManager = artistMetadataManager;
			mArtistProvider = artistProvider;
			mDatabaseProvider = databaseProvider;

            lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );
		}

		public void Initialize() {
            foreach( var updater in mUpdaters ) {
                updater.Initialize();
            }
            
            mEventAggregator.Subscribe( this );
		}

		public void Shutdown() {
			foreach( var updater in mUpdaters ) {
				updater.Shutdown();
			}

			mDatabaseProvider.Shutdown();
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

        public bool ArtistPortfolioAvailable( string forArtist ) {
            return mArtistMetadataManager.ArtistPortfolioAvailable( forArtist );
        }

        public IArtistMetadata GetArtistMetadata( string forArtist ) {
			mArtistMetadataManager.ArtistMentioned( forArtist );
			mUpdaters.Apply( updater => updater.QueueArtistUpdate( forArtist ));

			return( mArtistMetadataManager.GetArtistBiography( forArtist ));
		}

		public IArtistDiscography GetArtistDiscography( string forArtist ) {
			mArtistMetadataManager.ArtistMentioned( forArtist );
			mUpdaters.Apply( updater => updater.QueueArtistUpdate( forArtist ));

			return( mArtistMetadataManager.GetArtistDiscography( forArtist ));
		}

		public Artwork GetArtistArtwork( string forArtist ) {
			mArtistMetadataManager.ArtistMentioned( forArtist );
			mUpdaters.Apply( updater => updater.QueueArtistUpdate( forArtist ));

			return( mArtistMetadataManager.GetArtistArtwork( forArtist ));
		}

        public IEnumerable<Artwork> GetArtistPortfolio( string forArtist ) {
            return mArtistMetadataManager.GetArtistPortfolio( forArtist );
        }

        public async void ExportMetadata( string exportPath ) {
			/*
			try {
				if( mDocumentStore is EmbeddableDocumentStore ) {
                    var options = new SmugglerOptions();

                    if( mDocumentStore is EmbeddableDocumentStore embeddedStore ) {
                        var exporter = new DataDumper( embeddedStore.DocumentDatabase, options );

                        using( var stream = File.OpenWrite( exportPath )) {

                            await exporter.ExportData( stream, options, false );
                        }
                    }
				}
			}
			catch( Exception ex ) {
				mLog.LogException( $"Exporting Metadata to \"{exportPath}\"", ex );
			}
			*/
		}

		public async void ImportMetadata( string importPath ) {
            /*
			try {
				if(( mDocumentStore is EmbeddableDocumentStore ) &&
				   ( File.Exists( importPath ))) {
                    var options = new SmugglerOptions();

                    if( mDocumentStore is EmbeddableDocumentStore embeddedStore ) {
					    var importer = new DataDumper( embeddedStore.DocumentDatabase, options );

					    using( var stream = File.OpenRead( importPath )) {
						    await importer.ImportData( stream, options );
					    }
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( $"Importing Metadata from \"{importPath}\"", ex );
			}
			*/
		}
	}
}
