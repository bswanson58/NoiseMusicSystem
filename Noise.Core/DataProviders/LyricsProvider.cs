using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Google.API.Search;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Core.Support.AsyncTask;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataProviders {
	internal class LyricsProvider : ILyricsProvider {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly INoiseManager		mNoiseManager;
		private readonly ILog				mLog;

		private readonly AsyncCommand<LyricsRequestArgs>	mLyricsRequestCommand;

		public LyricsProvider( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mLog = mContainer.Resolve<ILog>();

			mLyricsRequestCommand = new AsyncCommand<LyricsRequestArgs>( OnLyricsRequested );
			GlobalCommands.RequestLyrics.RegisterCommand( mLyricsRequestCommand );
		}

		public bool Initialize() {
			return( true );
		}

		private void OnLyricsRequested( LyricsRequestArgs args ) {
			if(( args.Artist != null ) &&
			   ( args.Track != null )) {
				var lyricsInfo = LocateLyrics( args );

				if(!lyricsInfo.HasMatchedLyric ) {
					try {
						AsyncTaskEnumerator.Begin( LyricsDownloadTask( args, lyricsInfo ));
					}
					catch( Exception ex ) {
						mLog.LogException( "Exception - OnLyricsRequested:", ex );
					}
				}
				else {
					mEvents.GetEvent<Events.SongLyricsInfo>().Publish( lyricsInfo );
				}
			}
		}

		private LyricsInfo LocateLyrics( LyricsRequestArgs args ) {
			LyricsInfo	retValue;

			using( var lyricsList = mNoiseManager.DataProvider.GetPossibleLyrics( args.Artist, args.Track )) {
				var match = lyricsList.List.FirstOrDefault( lyric => lyric.ArtistId == args.Artist.DbId && lyric.TrackId == args.Track.DbId );
				if( match == null ) {
					match = lyricsList.List.FirstOrDefault( lyric => lyric.ArtistId == args.Artist.DbId && 
															lyric.SongName.Equals( args.Track.Name,StringComparison.CurrentCultureIgnoreCase ));
				}

				retValue = new LyricsInfo( args.Artist.DbId, args.Track.DbId, match, lyricsList.List );
			}

			return( retValue );
		}

		private IEnumerable<IAsyncTaskResult> LyricsDownloadTask( LyricsRequestArgs args, LyricsInfo lyricsInfo ) {
			var searchTerm = string.Format( @"lyrics ""{0}"" ""{1}""", args.Track.Name, args.Artist.Name );
			var search = new GoogleSearch( searchTerm );

			yield return search;

			foreach( var result in search.SearchResults ) {
				var downloader = new WebPageDownloader( result.Url );

				yield return downloader;

				if( downloader.Exception == null ) {
					if(!string.IsNullOrWhiteSpace( downloader.PageText )) {
						var parser = new LyricsPageParser( downloader.PageText );

						yield return parser;

						if( parser.Success ) {
							var dbLyric = new DbLyric( args.Artist.DbId, args.Track.DbId, args.Track.Name ) { Lyrics = parser.Lyrics, SourceUrl = result.Url };

							mNoiseManager.DataProvider.StoreLyric( dbLyric );
							lyricsInfo.SetMatchingLyric( dbLyric );

							mEvents.GetEvent<Events.SongLyricsInfo>().Publish( lyricsInfo );
							mLog.LogMessage( string.Format( "Downloaded lyrics for '{0}' from: {1}", dbLyric.SongName, dbLyric.SourceUrl ));
							break;
						}
					}
				}
				else {
					mLog.LogException( string.Format( "Exception - Downloading lyrics page: {0}", result.Url ), downloader.Exception );
				}
			}
		}
	}

	internal class GoogleSearch : IAsyncTaskResult {
		private readonly string				mSearchTerm;
		private readonly GwebSearchClient	mSearchClient;

		public	IEnumerable<IWebResult>		SearchResults { get; private set; }
		public	event EventHandler			Completed = delegate { };

		public GoogleSearch( string searchTerm ) {
			mSearchTerm = searchTerm;
			mSearchClient = new GwebSearchClient( "http://www.bswanson.com" );
		}

		public void Execute() {
			mSearchClient.BeginSearch( mSearchTerm, 25, OnSearchCompleted, null );
		} 

		private void OnSearchCompleted( IAsyncResult result ) {
			SearchResults = mSearchClient.EndSearch( result );

			Completed( this, EventArgs.Empty );
		}
	}

	internal class WebPageDownloader : IAsyncTaskResult {
		private	readonly WebRequest		mWebRequest;

		public	string					PageText { get; private set; }
		public	Exception				Exception { get; private set; }
		public	event EventHandler		Completed = delegate { };

		public WebPageDownloader( string url ) {
			mWebRequest = WebRequest.Create( url );
		}

		public void Execute() {
			mWebRequest.BeginGetResponse( OnRequestCompleted, null );
		}

		private void OnRequestCompleted( IAsyncResult result ) {
			try {
				var	response = mWebRequest.EndGetResponse( result );
				var reader = new StreamReader( response.GetResponseStream());

				PageText = reader.ReadToEnd();
			}
			catch( Exception ex ) {
				Exception = ex;
			}

			Completed( this, EventArgs.Empty );
		}
	}

	internal class LyricsPageParser : IAsyncTaskResult {
		private const string		cLyricsSearchPattern = @">(?:(?<lyric>[0-9a-z\s\]\[?',.()!:*`"";-]*)<br\s*/*>\s*){5,}(?:(?<lyric>[0-9a-z\s\]\[?',.()!:*`"";-]*))<";
		private readonly string		mPageText;
		private readonly Regex		mLyricsPattern;

		public	bool				Success { get; private set; }
		public string				Lyrics { get; private set; }
		public	event EventHandler	Completed = delegate { };

		public LyricsPageParser( string pageText ) {
			mPageText = pageText;			
			mLyricsPattern = new Regex( cLyricsSearchPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace );
		}

		public void Execute() {
			var match = mLyricsPattern.Match( mPageText );

			if( match.Success ) {
				var sb = new StringBuilder();

				foreach( var capture in match.Groups["lyric"].Captures ) {
					if(( sb.Length > 0 ) &&
						(!capture.ToString().StartsWith( Environment.NewLine )) &&
						(!sb.ToString().EndsWith( Environment.NewLine ))) {
						sb.AppendLine();
					}

					if(( sb.Length > 0 ) ||
						(!string.IsNullOrWhiteSpace( capture.ToString()))) {
						sb.Append( capture.ToString());
					}
				}

				Lyrics = sb.ToString();
				Success = true;
			}
			else {
				Success = false;
			}

			Completed( this, EventArgs.Empty );
		}
	}
}
