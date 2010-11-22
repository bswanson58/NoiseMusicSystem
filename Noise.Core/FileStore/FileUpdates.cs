using CuttingEdge.Conditions;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using TagLib;
using TagLib.Id3v2;

namespace Noise.Core.FileStore {
	internal class FileUpdates : IFileUpdates {
		private readonly IUnityContainer	mContainer;
		private readonly INoiseManager		mNoiseManager;
		private readonly ILog				mLog;

		private AsyncCommand<SetFavoriteCommandArgs>	mSetFavoriteCommand;
		private AsyncCommand<SetRatingCommandArgs>		mSetRatingCommand;

		public FileUpdates( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mLog = mContainer.Resolve<ILog>();
		}

		public bool Initialize() {
			mSetFavoriteCommand = new AsyncCommand<SetFavoriteCommandArgs>( OnSetFavorite );
			mSetFavoriteCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetFavorite.RegisterCommand( mSetFavoriteCommand );

			mSetRatingCommand = new AsyncCommand<SetRatingCommandArgs>( OnSetRating );
			mSetRatingCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetRating.RegisterCommand( mSetRatingCommand );
			return( true );
		}

		private void OnSetFavorite( SetFavoriteCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();

			var item = mNoiseManager.DataProvider.GetItem( args.ItemId );

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mNoiseManager.DataProvider.GetPhysicalFile( track );

				if( file != null ) {
					var tags = File.Create( mNoiseManager.DataProvider.GetPhysicalFilePath( file ));
					var id3Tags = tags.GetTag( TagTypes.Id3v2 ) as TagLib.Id3v2.Tag;

					if( id3Tags != null ) {
						var	favoriteFrame = UserTextInformationFrame.Get( id3Tags, Constants.FavoriteFrame, true );
						if( favoriteFrame != null ) {
							favoriteFrame.Description = Constants.FavoriteFrameDescription;
							favoriteFrame.Text = new [] { args.Value.ToString()};

							tags.Save();
						}
					}
				}
			}
		}

		private void OnSetRating( SetRatingCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();
			
			var item = mNoiseManager.DataProvider.GetItem( args.ItemId );

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mNoiseManager.DataProvider.GetPhysicalFile( track );

				if( file != null ) {
					var tags = File.Create( mNoiseManager.DataProvider.GetPhysicalFilePath( file ));
					var id3Tags = tags.GetTag( TagTypes.Id3v2 ) as TagLib.Id3v2.Tag;

					if( id3Tags != null ) {
						var popFrame = PopularimeterFrame.Get( id3Tags, Constants.Id3FrameUserName, true );

						if( popFrame != null ) {
							popFrame.Rating = StorageHelpers.ConvertToId3Rating( args.Value );

							tags.Save();
						}
					}
				}
			}
		}

		private void OnExecutionComplete( object sender, AsyncCommandCompleteEventArgs args ) {
			if(( args != null ) &&
			   ( args.Exception != null )) {
				mLog.LogException( "Exception - FileUpdates:OnExecutionComplete:", args.Exception );
			}
		}
	}
}
