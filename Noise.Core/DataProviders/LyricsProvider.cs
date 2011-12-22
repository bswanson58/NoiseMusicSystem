using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Google.API.Search;
using Microsoft.Practices.Prism.Events;
using Noise.Core.Support;
using Noise.Core.Support.AsyncTask;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataProviders {
	internal class LyricsSearcher : ILyricsSearcher, IRequireConstruction {
		private readonly IEventAggregator	mEvents;
		private readonly ILyricProvider		mLyricsProvider;
		private readonly bool				mHasNetworkAccess;

		private readonly AsyncCommand<LyricsRequestArgs>	mLyricsRequestCommand;

		public LyricsSearcher( IEventAggregator eventAggregator, ILyricProvider lyricProvider ) {
			mEvents = eventAggregator;
			mLyricsProvider = lyricProvider;

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				mHasNetworkAccess = configuration.HasNetworkAccess;
			}

			mLyricsRequestCommand = new AsyncCommand<LyricsRequestArgs>( OnLyricsRequested );
			GlobalCommands.RequestLyrics.RegisterCommand( mLyricsRequestCommand );
		}

		private void OnLyricsRequested( LyricsRequestArgs args ) {
			if(( args.Artist != null ) &&
			   ( args.Track != null )) {
				var lyricsInfo = LocateLyrics( args );

				if(!lyricsInfo.HasMatchedLyric ) {
					if( mHasNetworkAccess ) {
						try {
							AsyncTaskEnumerator.Begin( LyricsDownloadTask( args, lyricsInfo ));
						}
						catch( Exception ex ) {
							NoiseLogger.Current.LogException( "Exception - OnLyricsRequested:", ex );
						}
					}
				}
				else {
					mEvents.GetEvent<Events.SongLyricsInfo>().Publish( lyricsInfo );
				}
			}
		}

		private LyricsInfo LocateLyrics( LyricsRequestArgs args ) {
			LyricsInfo	retValue;

			using( var lyricsList = mLyricsProvider.GetPossibleLyrics( args.Artist, args.Track )) {
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

							mLyricsProvider.StoreLyric( dbLyric );
							lyricsInfo.SetMatchingLyric( dbLyric );

							mEvents.GetEvent<Events.SongLyricsInfo>().Publish( lyricsInfo );
							NoiseLogger.Current.LogMessage( string.Format( "Downloaded lyrics for '{0}' from: {1}", dbLyric.SongName, dbLyric.SourceUrl ));
							break;
						}
					}
				}
				else {
					NoiseLogger.Current.LogException( string.Format( "Exception - Downloading lyrics page: {0}", result.Url ), downloader.Exception );
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
			try {
				SearchResults = mSearchClient.EndSearch( result );
			}
			catch( Exception ) {
				SearchResults = new List<IWebResult>();
			}
			finally {
				Completed( this, EventArgs.Empty );
			}
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
				var stream = response.GetResponseStream();

				if( stream != null ) {
					var reader = new StreamReader( stream );

					PageText = reader.ReadToEnd();
				}
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
