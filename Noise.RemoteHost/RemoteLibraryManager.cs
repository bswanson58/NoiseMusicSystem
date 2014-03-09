using System;
using System.Linq;
using System.ServiceModel;
using AutoMapper;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteLibraryManager : INoiseRemoteLibrary {
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly ILibraryBuilder		mLibraryBuilder;

		public RemoteLibraryManager( ILibraryConfiguration libraryConfiguration, ILibraryBuilder libraryBuilder ) {
			mLibraryConfiguration = libraryConfiguration;
			mLibraryBuilder = libraryBuilder;
		}

		public BaseResult SelectLibrary( long libraryId ) {
			var retValue = new BaseResult();
			var	library = mLibraryConfiguration.Libraries.FirstOrDefault( l => l.LibraryId == libraryId );

			if( library != null ) {
				mLibraryConfiguration.Open( library );

				retValue.Success = true;
			}
			else {
				retValue.ErrorMessage = string.Format( "The specified library id ({0}) does not exist", libraryId );
			}

			return( retValue );
		}

		public BaseResult SyncLibrary() {
			var retValue = new BaseResult();

			if( mLibraryConfiguration.Current != null ) {
				if(!mLibraryBuilder.LibraryUpdateInProgress ) {
					mLibraryBuilder.StartLibraryUpdate();

					retValue.Success = true;
				}
				else {
					retValue.ErrorMessage = "A library sync is already in progress.";
				}
			}
			else {
				retValue.ErrorMessage = "There is no currently open library to sync.";
			}

			return( retValue );
		}

		private RoLibrary TransformLibrary( LibraryConfiguration library ) {
			var retValue = new RoLibrary();

			Mapper.DynamicMap( library, retValue );

			return( retValue );
		}

		public LibraryListResult GetLibraryList() {
			var retValue = new LibraryListResult();
			var libraries = mLibraryConfiguration.Libraries.Select( TransformLibrary );

			retValue.Libraries = libraries.ToArray();
			retValue.Success = true;

			return( retValue );
		}

		public BaseResult UpdateLibrary( RoLibrary library ) {
			var retValue = new BaseResult();

			if( library != null ) {
				try {
					var existingLibrary = mLibraryConfiguration.Libraries.FirstOrDefault( lib => lib.LibraryId == library.LibraryId );

					if( existingLibrary != null ) {
						existingLibrary.LibraryName = library.LibraryName;
						existingLibrary.DatabaseName = library.DatabaseName;
						existingLibrary.IsDefaultLibrary = library.IsDefaultLibrary;

						mLibraryConfiguration.UpdateLibrary( existingLibrary );

						retValue.Success = true;
					}
					else {
						retValue.ErrorMessage = "The library to be updated could not be located.";
					}
				}
				catch( Exception ex ) {
					retValue.ErrorMessage = ex.Message;
				}
			}
			else {
				retValue.ErrorMessage = "No library was received to update.";
			}

			return( retValue );
		}

		public LibraryListResult CreateLibrary( RoLibrary library ) {
			var retValue = new LibraryListResult();

			if( library != null ) {
				try {
					var newLibrary = new LibraryConfiguration { LibraryName = library.LibraryName,
																DatabaseName = library.DatabaseName,
																IsDefaultLibrary = library.IsDefaultLibrary };
					var mediaLocation = new MediaLocation{ PreferFolderStrategy = true };

					mediaLocation.FolderStrategy[0] = eFolderStrategy.Artist;
					mediaLocation.FolderStrategy[1] = eFolderStrategy.Album;
					mediaLocation.FolderStrategy[2] = eFolderStrategy.Volume;
					mediaLocation.Path = library.MediaLocation;

					newLibrary.MediaLocations.Add( mediaLocation );

					mLibraryConfiguration.AddLibrary( newLibrary );

					library.LibraryId = newLibrary.LibraryId;
					retValue.Libraries = new[] { library };
					retValue.Success = true;
				}
				catch( Exception ex ) {
					retValue.ErrorMessage = ex.Message;
				}
			}
			else {
				retValue.ErrorMessage = "No library was received to update.";
			}
			return( retValue );
		}
	}
}
