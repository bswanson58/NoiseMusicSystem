﻿using System.Data.Entity;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class RootFolderProvider : BaseProvider<RootFolder>, IRootFolderProvider {
		private	long	mFirstScanCompleted;

		public RootFolderProvider( IContextProvider contextProvider, ILogDatabase log ) :
			base( contextProvider, log ) { }

		public void AddRootFolder( RootFolder folder ) {
			AddItem( folder );
		}

		public RootFolder GetRootFolder( long folderId ) {
			RootFolder	retValue;

			using( var context = CreateContext()) {
				retValue = Set( context ).Include( entity => entity.FolderStrategy ).FirstOrDefault( entity => entity.DbId == folderId );
			}

			return( retValue );
		}

		public void DeleteRootFolder( RootFolder folder ) {
			Condition.Requires( folder ).IsNotNull();

			RemoveItem( folder );
		}

		public IDataProviderList<RootFolder> GetRootFolderList() {
			var	context = CreateContext();

			return( new EfProviderList<RootFolder>( context, Set( context ).Include( entity => entity.FolderStrategy )));
		}

		public IDataUpdateShell<RootFolder> GetFolderForUpdate( long folderId ) {
			var	context = CreateContext();

			return( new EfUpdateShell<RootFolder>( context, Set( context ).Include( entity => entity.FolderStrategy )
																		  .FirstOrDefault( entity => entity.DbId == folderId )));
		}

		public long FirstScanCompleted() {
			var retValue = mFirstScanCompleted;

			if( retValue == 0 ) {
				using( var folderList = GetRootFolderList()) {
					if( folderList.List.Any()) {
						mFirstScanCompleted = folderList.List.Max( folder => folder.InitialScanCompleted );
					}
				}

				retValue = mFirstScanCompleted;
			}

			return( retValue );
		}
	}
}
