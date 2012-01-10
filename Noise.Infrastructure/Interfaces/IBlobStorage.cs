using System.IO;

namespace Noise.Infrastructure.Interfaces {
	public interface IBlobStorage {
		void	Insert( long blobId, string fromFile );
		void	Insert( long blobId, Stream blobData );
		void	Update( long blobId, string fromFile );
		void	Update( long blobId, Stream blobData );

		void	Store( long blobId, string fromFile );
		void	Store( long blobId, Stream blobData );
		void	StoreText( long blobId, string text );

		void	Delete( long blobId );

		Stream	Retrieve( long blobId );
		byte[]	RetrieveBytes( long blobId );
		string	RetrieveText( long blobId );

		void	DeleteStorage();
	}

}
