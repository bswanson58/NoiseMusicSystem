using System;
using System.Linq;
using System.ServiceModel;
using AutoMapper;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteQueueServer : INoiseRemoteQueue {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;
		private	readonly INoiseManager		mNoiseManager;

		public RemoteQueueServer( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();
		}

		public BaseResult EnqueueTrack( long trackId ) {
			var retValue = new BaseResult();

			try {
				var track = mNoiseManager.DataProvider.GetTrack( trackId );

				if( track != null ) {
					GlobalCommands.PlayTrack.Execute( track );

					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "RemoteQueueServer:EnqueueTrack", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public BaseResult EnqueueAlbum( long albumId ) {
			var retValue = new BaseResult();

			try {
				var album = mNoiseManager.DataProvider.GetAlbum( albumId );

				if( album != null ) {
					GlobalCommands.PlayAlbum.Execute( album );

					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "RemoteQueueServer:EnqueueAlbum", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		private static RoPlayQueueTrack TransformQueueTrack( PlayQueueTrack queueTrac ) {
			var retValue = new RoPlayQueueTrack();

			Mapper.DynamicMap( queueTrac, retValue );

			return( retValue );
		}

		public PlayQueueListResult GetQueuedTrackList() {
			var retValue = new PlayQueueListResult();

			try {
				retValue.Tracks = mNoiseManager.PlayQueue.PlayList.Select( TransformQueueTrack).ToArray();

				retValue.Success = true;
			}
			catch( Exception ex ) {
				mLog.LogException( "RemoteQueueServer:GetQueuedTrackList", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}
	}
}
