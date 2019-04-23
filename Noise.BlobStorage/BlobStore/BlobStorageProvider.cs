using System;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.BlobStorage.BlobStore {
    class BlobStorageProvider : IBlobStorageProvider, IHandle<Events.LibraryChanged> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly INoiseLog      	    mLog;
        private readonly ILibraryConfiguration  mLibraryConfiguration;
        private readonly IBlobStorageResolver   mStorageResolver;
        private IBlobStorageManager             mBlobStorageManager;
        private IInPlaceStorageManager          mInPlaceStorageManager;

        private readonly Func<IBlobStorageManager>      mBlobStorageFactory;
        private readonly Func<IInPlaceStorageManager>   mInPlaceStorageFactory;

        public  IBlobStorage                    BlobStorage => BlobStorageManager?.GetStorage();

        public BlobStorageProvider( ILibraryConfiguration libraryConfiguration, IEventAggregator eventAggregator, INoiseLog log,
                                    Func<IInPlaceStorageManager> inPlaceStorageManager, Func<IBlobStorageManager> blobStorageManager, IBlobStorageResolver storageResolver ) {
            mEventAggregator = eventAggregator;
            mLog = log;
            mLibraryConfiguration = libraryConfiguration;
            mInPlaceStorageFactory = inPlaceStorageManager;
            mBlobStorageFactory = blobStorageManager;
            mStorageResolver = storageResolver;

            mEventAggregator.Subscribe( this );
        }

        public void Handle( Events.LibraryChanged args ) {
            if( mBlobStorageManager != null ) {
                if( mBlobStorageManager.IsOpen ) {
                    mBlobStorageManager.CloseStorage();
                }
            }

            if( mInPlaceStorageManager != null ) {
                if( mInPlaceStorageManager.IsOpen ) {
                    mInPlaceStorageManager.CloseStorage();
                }
            }

            if( mLibraryConfiguration.Current != null ) {
                BlobStorageManager.Initialize( mLibraryConfiguration );

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

        private IBlobStorageManager GetBlobStorageManager() {
            if( mBlobStorageManager == null ) {
                mBlobStorageManager = mBlobStorageFactory();
                mBlobStorageManager.SetResolver( mStorageResolver );
            }

            return mBlobStorageManager;
        }

        private IBlobStorageManager GetInPlaceStorageManager() {
            if( mInPlaceStorageManager == null ) {
                mInPlaceStorageManager = mInPlaceStorageFactory();
                mInPlaceStorageManager.SetResolver( mStorageResolver );
            }

            return mInPlaceStorageManager;
        }

        public IBlobStorageManager BlobStorageManager {
            get {
                var retValue = default( IBlobStorageManager );

                if( mLibraryConfiguration.Current != null ) {
                    retValue = mLibraryConfiguration.Current.IsMetadataInPlace ? GetInPlaceStorageManager() : GetBlobStorageManager();
                }

                return retValue;
            }
        }
    }
}
