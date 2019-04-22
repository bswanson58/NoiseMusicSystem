using System;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;


namespace Noise.BlobStorage.BlobStore {
    class BlobStorageProvider : IBlobStorageProvider, IHandle<Events.LibraryChanged> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly INoiseLog      	    mLog;
        private readonly IBlobStorageManager	mBlobStorageManager;
        private readonly ILibraryConfiguration  mLibraryConfiguration;

        public BlobStorageProvider( ILibraryConfiguration libraryConfiguration, IEventAggregator eventAggregator,
                                    IBlobStorageManager blobStorageManager, IBlobStorageResolver storageResolver, INoiseLog log ) {
            mEventAggregator = eventAggregator;
            mLog = log;
            mLibraryConfiguration = libraryConfiguration;
            mBlobStorageManager = blobStorageManager;
            mBlobStorageManager.SetResolver( storageResolver );

            mEventAggregator.Subscribe( this );
        }

        public void Handle( Events.LibraryChanged args ) {
            if( mBlobStorageManager.IsOpen ) {
                mBlobStorageManager.CloseStorage();
            }

            if( mLibraryConfiguration.Current != null ) {
                BlobStorageManager.Initialize( mLibraryConfiguration.Current.BlobDatabasePath );

                if(!BlobStorageManager.IsOpen ) {
                    if(!BlobStorageManager.OpenStorage()) {
                        BlobStorageManager.CreateStorage();

                        if(!BlobStorageManager.OpenStorage()) {
                            var ex = new ApplicationException( "BlobStorageProvider:Blob storage could not be created." );

                            mLog.LogException( "Blob storage could not be created", ex );
                            throw( ex );
                        }
                    }
                }
            }
        }

        public IBlobStorageManager  BlobStorageManager {
            get {
                var retValue = default( IBlobStorageManager );

                if( mLibraryConfiguration.Current != null ) {
                    retValue = mLibraryConfiguration.Current.IsMetadataInPlace ? mBlobStorageManager : mBlobStorageManager;
                }

                return retValue;
            }
        }
    }
}
