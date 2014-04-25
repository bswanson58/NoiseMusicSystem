using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class StorageFileProvider : BaseDataProvider<StorageFile>, IStorageFileProvider {
		public StorageFileProvider( IEloqueraManager databaseManager ) :
			base( databaseManager ) { }

		public void AddFile( StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			InsertItem( file );
		}

		public void Add( IEnumerable<StorageFile> list ) {
			foreach( var item in list ) {
				AddFile( item );
			}
		}

		public void DeleteFile( StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			DeleteItem( file );
		}

		public StorageFile GetPhysicalFile( DbTrack forTrack ) {
			Condition.Requires( forTrack ).IsNotNull();

			return( TryGetItem( "SELECT StorageFile Where MetaDataPointer = @trackId", new Dictionary<string, object>{{ "trackId", forTrack.DbId }}, "GetPhysicalFile" ));
		}

		public IDataProviderList<StorageFile> GetAllFiles() {
			return( TryGetList( "SELECT StorageFile", "GetAllFiles" ));
		}

		public IDataProviderList<StorageFile> GetDeletedFilesList() {
			return( TryGetList( "SELECT StorageFile WHERE IsDeleted", "GetDeletedFilesList" ));
		}

		public IDataProviderList<StorageFile> GetFilesInFolder( long parentFolder ) {
			return( TryGetList( "SELECT StorageFile Where ParentFolder = @parentId", new Dictionary<string, object> {{ "parentId", parentFolder }}, "GetFilesInFolder" ));
		}

		public IDataProviderList<StorageFile> GetFilesRequiringProcessing() {
			return ( TryGetList( "SELECT StorageFile Where FileType = @fileType OR WasUpdated = True", 
									new Dictionary<string, object> { { "fileType", eFileType.Undetermined } }, "GetFilesRequiringProcessing" ) );
		}

		public IDataUpdateShell<StorageFile> GetFileForUpdate( long fileId ) {
			return( GetUpdateShell( "SELECT StorageFile Where DbId = @fileId", new Dictionary<string, object> {{ "fileId", fileId }}));
		}

		public long GetItemCount() {
			return( GetItemCount( "SELECT StorageFile" ));
		}
	}
}
