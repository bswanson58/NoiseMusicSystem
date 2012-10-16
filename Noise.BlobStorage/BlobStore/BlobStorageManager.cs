using System.Globalization;
using System.IO;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.BlobStorage.BlobStore {
	public class BlobStorageManager : IBlobStorageManager, IBlobStorage {
		private IBlobStorageResolver	mBlobResolver;
		private bool					mIsOpen;
		private string					mStoragePath;

		public BlobStorageManager() {
			mStoragePath = string.Empty;
			mStoragePath = string.Empty;
			mIsOpen = false;
		}

		public void SetResolver( IBlobStorageResolver resolver ) {
			Condition.Requires( resolver ).IsNotNull();

			mBlobResolver = resolver;
		}

		public bool Initialize( string rootStoragePath ) {
			Condition.Requires( mBlobResolver ).IsNotNull();

			mStoragePath = rootStoragePath;

			return( true );
		}

		public bool OpenStorage() {
			if(!string.IsNullOrWhiteSpace( mStoragePath )) {
				CloseStorage();
			}

			if( Directory.Exists( mStoragePath )) {
					mIsOpen = true;
			}

			return( IsOpen );
		}

		public bool IsOpen {
			get{ return( mIsOpen ); }
		}

		public bool CreateStorage() {
			if(!Directory.Exists( mStoragePath )) {
				Directory.CreateDirectory( mStoragePath );
			}

			return( Directory.Exists( mStoragePath ));
		}

		public void CloseStorage() {
			if( IsOpen ) {
				mIsOpen = false;
				mStoragePath = "";
			}
		}

		public void DeleteStorage() {
			if( IsOpen ) {
				CloseStorage();
			}

			if( Directory.Exists( mStoragePath )) {
				Directory.Delete( mStoragePath, true );
			}
		}

		public IBlobStorage GetStorage() {
			return( this );
		}

		public void Insert( long blobId, string fromFile ) {
			using( var fileStream = new FileStream( fromFile, FileMode.Open, FileAccess.Read )) {
				Insert( blobId, fileStream );
			}
		}

		public void Insert( long blobId, Stream blobData ) {
			string	blobPath = ResolveBlobId( blobId );

			if( File.Exists( blobPath )) {
				throw new BlobStorageException( blobId, blobPath, "Attempt to insert existing item." );
			}

			StoreBlob( blobData, blobPath );
		}

		public void StoreText( long blobId, string text ) {
			var	blobPath = ResolveBlobId( blobId );
			var storagePath = Path.GetDirectoryName( blobPath );

			if(!string.IsNullOrEmpty( storagePath )) {
				if(!Directory.Exists( storagePath )) {
					Directory.CreateDirectory( storagePath );
				}

				using( var blobStream = new FileStream( blobPath, FileMode.Create, FileAccess.Write )) {
					using( var writer = new StreamWriter( blobStream )) {
						writer.Write( text );

						writer.Close();
						blobStream.Close();
					}
				}
			}
		}

		public void Store( long blobId, string fromFile ) {
			using( var fileStream = new FileStream( fromFile, FileMode.Open, FileAccess.Read )) {
				StoreBlob( fileStream, ResolveBlobId( blobId ));
			}
		}

		public void Store( long blobId, Stream blobData ) {
			StoreBlob( blobData, ResolveBlobId( blobId ));
		}

		public void Delete( long blobId ) {
			string	blobPath = ResolveBlobId( blobId );

			if( File.Exists( blobPath )) {
				File.Delete( blobPath );
			}
			else {
				throw new BlobStorageException( blobId, blobPath, "Attempt to delete nonexistent item." );
			}
		}

		public string RetrieveText( long blobId ) {
			var		retValue = "";

			using( var stream = Retrieve( blobId )) {
				if( stream != null ) {
					using( var reader = new StreamReader( stream )) {
						retValue = reader.ReadToEnd();

						reader.Close();
					}

					stream.Close();
				}
			}

			return( retValue );
		}

		public byte[] RetrieveBytes( long blobId ) {
			byte[]	retValue = null;
			
			using( var stream = Retrieve( blobId )) {
				if( stream != null ) {
					retValue = new byte[stream.Length];

					stream.Read( retValue, 0, retValue.Length );
					stream.Close();
				}
			}

			return( retValue );
		}

		public Stream Retrieve( long blobId ) {
			Stream	retValue;
			string	blobPath = ResolveBlobId( blobId );

			if( File.Exists( blobPath )) {
				retValue = new FileStream( blobPath, FileMode.Open, FileAccess.Read );
			}
			else {
				NoiseLogger.Current.LogInfo( "Attempt to retieve nonexistent blob item" );

				retValue = new MemoryStream();
//				throw new BlobStorageException( blobId, blobPath, "Attempt to retrieve nonexistent item." );
			}

			return( retValue );
		}

		private static void StoreBlob( Stream blobData, string blobPath ) {
			var storagePath = Path.GetDirectoryName( blobPath );

			if(!string.IsNullOrEmpty( storagePath )) {
				if(!Directory.Exists( storagePath )) {
					Directory.CreateDirectory( storagePath );
				}

				using( Stream blobStream = new FileStream( blobPath, FileMode.Create, FileAccess.Write )) {
					blobData.Position = 0;
					blobData.CopyTo( blobStream );
					blobStream.Close();
				}
			}
		}

		private string ResolveBlobId( long blobId ) {
			var retValue = mStoragePath;

			for( uint level = 0; level < mBlobResolver.StorageLevels; level++ ) {
				retValue = Path.Combine( retValue, mBlobResolver.KeyForStorageLevel( blobId, level ));
			}

			retValue = Path.Combine( retValue, blobId.ToString( CultureInfo.InvariantCulture ));

			return( retValue );
		}
	}
}
