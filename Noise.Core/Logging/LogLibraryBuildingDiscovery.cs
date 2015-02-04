using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Logging {
	public class LogLibraryBuildingDiscovery : BaseLogger, ILogLibraryBuildingDiscovery {
		private readonly IStorageFolderSupport	mStorageSupport;
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Library Building";
		private const string	cPhaseName = "Discovery";

		public LogLibraryBuildingDiscovery( IStorageFolderSupport storageFolderSupport, IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mStorageSupport = storageFolderSupport;

			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogDiscoveryMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void LogDiscoveryStarted( RootFolder folder ) {
			LogOnCondition( mPreferences.LogAnyBuildingDiscovery, () => LogDiscoveryMessage( "Starting discovery: {0} \"{1}\"", folder, mStorageSupport.GetPath( folder )));
		}

		public void LogDiscoveryCompleted( RootFolder folder ) {
			LogOnCondition( mPreferences.LogAnyBuildingDiscovery, () => LogDiscoveryMessage( "Completed discovery: {0}", folder ));
		}

		public void LogLibraryNotFound( RootFolder folder ) {
			LogOnCondition( mPreferences.LogAnyBuildingDiscovery, () => LogDiscoveryMessage( "Library does not exist: {0} \"{1}\"", folder, mStorageSupport.GetPath( folder )));
		}

		public void LogFolderFound( StorageFolder folder ) {
			LogOnCondition( mPreferences.BuildingDiscoverFolders, () => LogDiscoveryMessage( "Item found: {0} \"{1}\"", folder, mStorageSupport.GetPath( folder )));
		}

		public void LogFolderNotFound( StorageFolder folder ) {
			LogOnCondition( mPreferences.BuildingDiscoverFolders, () => LogDiscoveryMessage( "Item not found: {0} \"{1}\"", folder, mStorageSupport.GetPath( folder )));
		}

		public void LogFileFound( StorageFile file ) {
			LogOnCondition( mPreferences.BuildingDiscoverFiles, () => LogDiscoveryMessage( "Item found: {0} \"{1}\"", file, mStorageSupport.GetPath( file )));
		}

		public void LogFileNotFound( StorageFile file ) {
			LogOnCondition( mPreferences.BuildingDiscoverFiles, () => LogDiscoveryMessage( "Item not found: {0} \"{1}\"", file, mStorageSupport.GetPath( file )));
		}

		public void LogFileUpdated( StorageFile file ) {
			LogOnCondition( mPreferences.BuildingDiscoverFiles, () => LogDiscoveryMessage( "Item updated: {0} \"{1}\"", file, mStorageSupport.GetPath( file )));
		}

		public void LogDiscoveryException( RootFolder rootFolder, Exception exception, [CallerMemberName] string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", rootFolder ));
		}

		public void LogDiscoveryException( string message, Exception exception, [CallerMemberName] string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
