using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CuttingEdge.Conditions;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using ReusableBits.Threading;
using TagLib;
using TagLib.Id3v2;
using File = TagLib.File;

namespace Noise.Core.FileStore {
	internal class FileUpdates : IFileUpdates, IRequireInitialization {
		internal const string						cBackgroundFileUpdater = "BackgroundFileUpdater";

		private readonly ITrackProvider				mTrackProvider;
		private readonly IDbBaseProvider			mDbBaseProvider;
		private readonly IStorageFileProvider		mStorageFileProvider;
		private readonly IStorageFolderSupport		mStorageFolderSupport;
		private readonly IRecurringTaskScheduler	mTaskScheduler;
		private readonly List<BaseCommandArgs>		mUnfinishedCommands;
		private bool								mClearReadOnly;

		private AsyncCommand<SetFavoriteCommandArgs>		mSetFavoriteCommand;
		private AsyncCommand<SetRatingCommandArgs>			mSetRatingCommand;
		private AsyncCommand<UpdatePlayCountCommandArgs>	mUpdatePlayCountCommand;
		private AsyncCommand<SetMp3TagCommandArgs>			mSetMp3TagsCommand;

		public FileUpdates( ILifecycleManager lifecycleManager, IRecurringTaskScheduler recurringTaskScheduler,
						    ITrackProvider trackProvider, IDbBaseProvider dbBaseProvider,
							IStorageFolderSupport storageFolderSupport, IStorageFileProvider storageFileProvider ) {
			mTrackProvider = trackProvider;
			mDbBaseProvider = dbBaseProvider;
			mStorageFileProvider = storageFileProvider;
			mStorageFolderSupport = storageFolderSupport;
			mTaskScheduler = recurringTaskScheduler;

			lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );

			mUnfinishedCommands = new List<BaseCommandArgs>();
		}

		public void Initialize() {
			mSetFavoriteCommand = new AsyncCommand<SetFavoriteCommandArgs>( OnSetFavorite );
			mSetFavoriteCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetFavorite.RegisterCommand( mSetFavoriteCommand );

			mSetRatingCommand = new AsyncCommand<SetRatingCommandArgs>( OnSetRating );
			mSetRatingCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetRating.RegisterCommand( mSetRatingCommand );

			mUpdatePlayCountCommand = new AsyncCommand<UpdatePlayCountCommandArgs>( OnUpdatePlayCount );
			mUpdatePlayCountCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.UpdatePlayCount.RegisterCommand( mUpdatePlayCountCommand );

			mSetMp3TagsCommand = new AsyncCommand<SetMp3TagCommandArgs>( OnSetMp3Tags );
			mSetMp3TagsCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetMp3Tags.RegisterCommand( mSetMp3TagsCommand );

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if( configuration != null ) {
				mClearReadOnly = configuration.EnableReadOnlyUpdates;
			}

			var backgroundJob = new RecurringTask( ProcessUnfinishedCommands, "Background File Update Task" );
			backgroundJob.TaskSchedule.StartAt( RecurringInterval.FromMinutes( 1 )).Interval( RecurringInterval.FromMinutes( 1 ));

			mTaskScheduler.AddRecurringTask( backgroundJob );

			NoiseLogger.Current.LogMessage( "Started Background FileUpdater." );
		}

		public void Shutdown() {
			mTaskScheduler.RemoveAllTasks();
			ProcessUnfinishedCommands( null );

			if( mUnfinishedCommands.Count > 0 ) {
				NoiseLogger.Current.LogMessage( "FileUpdater: There were unfinished commands at shutdown." );
			}
		}

		internal void ProcessUnfinishedCommands( RecurringTask task ) {
			if( mUnfinishedCommands.Count > 0 ) {
				var unfinishedCommands = new List<BaseCommandArgs>();

				lock( mUnfinishedCommands ) {
					unfinishedCommands.AddRange( mUnfinishedCommands );
					mUnfinishedCommands.Clear();
				}

				foreach( var command in unfinishedCommands ) {
					TypeSwitch.Do( command, TypeSwitch.Case<SetFavoriteCommandArgs>( OnSetFavorite ),
											TypeSwitch.Case<SetRatingCommandArgs>( OnSetRating ),
											TypeSwitch.Case<SetMp3TagCommandArgs>( SetMp3FileTags ),
											TypeSwitch.Case<UpdatePlayCountCommandArgs>( OnUpdatePlayCount ));
				}
			}
		}

		private void OnSetFavorite( SetFavoriteCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();

			var item = mDbBaseProvider.GetItem( args.ItemId );

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mStorageFileProvider.GetPhysicalFile( track );

				if( file != null ) {
					switch( mStorageFolderSupport.DetermineAudioEncoding( file )) {
						case eAudioEncoding.MP3:
							SetMp3Favorite( args, track, file );
							break;

//						case eAudioEncoding.FLAC:
//						case eAudioEncoding.OGG:
//							SetVorbisFavorite( args, track, file );
//							break;

//						case eAudioEncoding.WMA:
//							SetWmaFavorite( args, track, file );
//							break;
					}
				}
			}
		}

		private void SetMp3Favorite( SetFavoriteCommandArgs args, DbTrack track, StorageFile file ) {
			var filePath = mStorageFolderSupport.GetPath( file );

			ClearReadOnlyFlag( filePath );

			var tags = File.Create( filePath );
			var id3Tags = tags.GetTag( TagTypes.Id3v2 ) as TagLib.Id3v2.Tag;

			if( id3Tags != null ) {
				var	favoriteFrame = UserTextInformationFrame.Get( id3Tags, Constants.FavoriteFrameDescription, true );

				if( favoriteFrame != null ) {
					favoriteFrame.Text = new [] { args.Value.ToString( CultureInfo.InvariantCulture )};

					try {
						tags.Save();
					}
					catch( Exception ) {
						NoiseLogger.Current.LogMessage( string.Format( "FileUpdates:SetFavorite - Queueing for later: {0}", track.Name ));

						lock( mUnfinishedCommands ) {
							mUnfinishedCommands.Add( args );
						}
					}
				}
			}
		}

/*		private void SetVorbisFavorite( SetFavoriteCommandArgs args, DbTrack track, StorageFile file ) {
			var filePath = mNoiseManager.DataProvider.GetPhysicalFilePath( file );

			ClearReadOnlyFlag( filePath );

			var tags = File.Create( filePath );
			var flacTags = tags.GetTag( TagTypes.Xiph ) as TagLib.Ogg.XiphComment;

			if( flacTags != null ) {
				var	f = flacTags.GetFirstField( "" );
			}
		}
*/
/*		private void SetWmaFavorite( SetFavoriteCommandArgs args, DbTrack track, StorageFile file ) {
			var filePath = mNoiseManager.DataProvider.GetPhysicalFilePath( file );

			ClearReadOnlyFlag( filePath );

			var tags = File.Create( filePath );
			var wmaTags = tags.GetTag( TagTypes.Asf ) as TagLib.Asf.Tag;

			if( wmaTags != null ) {
				var d = wmaTags.GetDescriptorString( "" );
			}
		}
*/
		private void OnSetRating( SetRatingCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();
			
			var item = mDbBaseProvider.GetItem( args.ItemId );

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mStorageFileProvider.GetPhysicalFile( track );

				if( file != null ) {
					var filePath = mStorageFolderSupport.GetPath( file );

					ClearReadOnlyFlag( filePath );

					var tags = File.Create( filePath );
					var id3Tags = tags.GetTag( TagTypes.Id3v2 ) as TagLib.Id3v2.Tag;

					if( id3Tags != null ) {
						var popFrame = PopularimeterFrame.Get( id3Tags, Constants.Id3FrameUserName, true );

						if( popFrame != null ) {
							popFrame.Rating = mStorageFolderSupport.ConvertToId3Rating( args.Value );

							try {
								tags.Save();
							}
							catch( Exception ) {
								NoiseLogger.Current.LogMessage( string.Format( "FileUpdates:SetRating - Queueing for later: {0}", track.Name ));

								lock( mUnfinishedCommands ) {
									mUnfinishedCommands.Add( args );
								}
							}
						}
					}
				}
			}
		}

		private void OnUpdatePlayCount( UpdatePlayCountCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();
			
			var item = mDbBaseProvider.GetItem( args.ItemId );

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mStorageFileProvider.GetPhysicalFile( track );

				if( file != null ) {
					var filePath = mStorageFolderSupport.GetPath( file );

					ClearReadOnlyFlag( filePath );

					var tags = File.Create( filePath );
					var id3Tags = tags.GetTag( TagTypes.Id3v2 ) as TagLib.Id3v2.Tag;

					if( id3Tags != null ) {
						var popFrame = PopularimeterFrame.Get( id3Tags, Constants.Id3FrameUserName, true );

						if( popFrame != null ) {
							popFrame.PlayCount++;

							try {
								tags.Save();
							}
							catch( Exception ) {
								NoiseLogger.Current.LogMessage( string.Format( "FileUpdates:UpdatePlayCount - Queueing for later: {0}", track.Name ));

								lock( mUnfinishedCommands ) {
									mUnfinishedCommands.Add( args );
								}
							}
						}
					}
				}
			}
		}

		private void OnSetMp3Tags( SetMp3TagCommandArgs args ) {
			if( args.IsAlbum ) {
				NoiseLogger.Current.LogInfo( "Updating Mp3 file tags for album." );

				using( var trackList = mTrackProvider.GetTrackList( args.ItemId )) {
					foreach( var track in trackList.List ) {
						SetMp3FileTags( new SetMp3TagCommandArgs( track, args ));
					}
				}
			}
			else {
				NoiseLogger.Current.LogInfo( "Updating Mp3 file tags for file." );

				SetMp3FileTags( args );
			}
		}

		private void SetMp3FileTags( SetMp3TagCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();
			
			var item = mDbBaseProvider.GetItem( args.ItemId );

			Condition.Requires( item ).IsNotNull();
			Condition.Requires( item ).IsOfType( typeof( DbTrack ));

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mStorageFileProvider.GetPhysicalFile( track );

				if( file != null ) {
					var filePath = mStorageFolderSupport.GetPath( file );

					ClearReadOnlyFlag( filePath );

					var tags = File.Create( filePath );

					if( args.SetPublishedYear ) {
						tags.Tag.Year = (uint)args.PublishedYear;
					}
					if( args.SetName ) {
						tags.Tag.Title = args.Name;
					}

					try {
						tags.Save();
					}
					catch( Exception ) {
						NoiseLogger.Current.LogMessage( string.Format( "FileUpdates:SetMp3FileTags - Queueing for later: {0}", track.Name ));

						lock( mUnfinishedCommands ) {
							mUnfinishedCommands.Add( args );
						}
					}
				}
			}
		}

		private static void OnExecutionComplete( object sender, AsyncCommandCompleteEventArgs args ) {
			if(( args != null ) &&
			   ( args.Exception != null )) {
				NoiseLogger.Current.LogException( "Exception - FileUpdates:OnExecutionComplete:", args.Exception );
			}
		}

		private void ClearReadOnlyFlag( string file ) {
			if( mClearReadOnly ) {
				try {
					if( System.IO.File.Exists( file )) {
					   var fileAttributes = System.IO.File.GetAttributes( file );
					
						if( fileAttributes.HasFlag( FileAttributes.ReadOnly )) {
							fileAttributes &= ~FileAttributes.ReadOnly;

							System.IO.File.SetAttributes( file, fileAttributes );
						}
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( string.Format( "Exception FileUpdates:ClearReadOnlyFlag for file: ({0}) -", file ), ex );
				}
			}
		}
	}
}
