using Google.API.Search;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataProviders {
	internal class LyricsProvider : ILyricsProvider {
		private readonly IUnityContainer	mContainer;
		private readonly INoiseManager		mNoiseManager;
		private readonly ILog				mLog;
		private GwebSearchClient			mSearchClient;

		private readonly AsyncCommand<LyricsRequestArgs>	mLyricsRequestCommand;

		public LyricsProvider( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mLog = mContainer.Resolve<ILog>();

			mLyricsRequestCommand = new AsyncCommand<LyricsRequestArgs>( OnLyricsRequested );
			GlobalCommands.RequestLyrics.RegisterCommand( mLyricsRequestCommand );
		}

		public bool Initialize() {
			mSearchClient = new GwebSearchClient( "http://www.bswanson.com" );

			return( true );
		}

		public void OnLyricsRequested( LyricsRequestArgs args ) {
			if(( args.Artist != null ) &&
			   ( args.Track != null )) {
				var searchTerm = string.Format( "lyrics {0} {1}", args.Artist.Name, args.Track.Name );
				var results = mSearchClient.Search( searchTerm, 25 );

				foreach( var result in results ) {
					var url = result.Url;
				}
			}
		}
	}
}
