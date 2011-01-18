using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Fibre.Threading;
using Google.API.Search;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataProviders {
	internal class LyricsProvider : ILyricsProvider {
		private const string				cLyricsSearchPattern = "";

		private readonly IUnityContainer	mContainer;
		private readonly INoiseManager		mNoiseManager;
		private readonly ILog				mLog;
		private readonly Regex				mLyricsPattern;
		private GwebSearchClient			mSearchClient;

		private readonly AsyncCommand<LyricsRequestArgs>	mLyricsRequestCommand;

		public LyricsProvider( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mLog = mContainer.Resolve<ILog>();

			mLyricsPattern = new Regex( cLyricsSearchPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace );

			mLyricsRequestCommand = new AsyncCommand<LyricsRequestArgs>( OnLyricsRequested );
			GlobalCommands.RequestLyrics.RegisterCommand( mLyricsRequestCommand );
		}

		public bool Initialize() {
			mSearchClient = new GwebSearchClient( "http://www.bswanson.com" );

			return( true );
		}

		private void OnLyricsRequested( LyricsRequestArgs args ) {
			if(( args.Artist != null ) &&
			   ( args.Track != null )) {
				if(!LocateLyrics( args )) {
					SearchLyrics( args );
				}
			}
		}

		private static bool LocateLyrics( LyricsRequestArgs args ) {
			return( false );
		}

		private void SearchLyrics( LyricsRequestArgs args ) {
			var searchTerm = string.Format( "lyrics {0} {1}", args.Artist.Name, args.Track.Name );
			var	 asyncWork = new AsyncWork()
				.AddWork( () => ( mSearchClient.Search( searchTerm, 25 )))
				.WhenComplete( searchResult => {
							    foreach( var url in searchResult.Result ) {
									if( DownloadLyrics( url.Url )) {
										break;
									}
							    }
						    }
				   )
			   .PerformWork();
		}

		private bool DownloadLyrics( string url ) {
			var retValue = false;
			var webRequest = WebRequest.Create( url );
			var asyncWork = new AsyncWork()
			.AddWork( webRequest.BeginGetResponse, r => webRequest.EndGetResponse( r ))
			.WhenComplete( response => {
			               	var reader = new StreamReader( response.Result.GetResponseStream());
							var lyricsPage = reader.ReadToEnd();

							if( ParseResponse( lyricsPage )) {
								retValue = true;
							}
						} )
			.PerformWork();

			return( retValue );
		}

		private bool ParseResponse( string response ) {
			var retValue = false;
			var match = mLyricsPattern.Match( response );

			if( match.Success ) {
				retValue = true;
			}

			return( retValue );
		}
	}
}
