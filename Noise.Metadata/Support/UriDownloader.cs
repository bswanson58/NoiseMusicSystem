using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Noise.Infrastructure;

namespace Noise.Metadata.Support {
	// from: http://stackoverflow.com/questions/19189275/asynchronously-and-parallelly-downloading-files

	public class DownloadFileDetails {
		public string Uri { get; private set; }
		public string LocalPath { get; private set; }

		public DownloadFileDetails( string uri, string localPath ) {
			Uri = uri;
			LocalPath = localPath;
		}
	}

	public class UriDownloader {
		public static async Task<List<Tuple<string, string, Exception>>> DownloadFileListAsync( IEnumerable<DownloadFileDetails> uriList ) {
		    var results = new List<Tuple<string, string, Exception>>();

			await uriList.ForEachAsync( fileDetail => DownloadFileTaskAsync( fileDetail ), ( input, output ) => results.Add( output ));

			return( results );
		}

		public static async Task<Tuple<string, string, Exception>> DownloadFileTaskAsync( DownloadFileDetails fileDetail, int timeOut = 3000 ) {
			return( await DownloadFileTaskAsync( fileDetail.Uri, fileDetail.LocalPath, timeOut ));
			
		}

		/// <summary>
		///     Downloads a file from a specified Internet address.
		/// </summary>
		/// <param name="remotePath">Internet address of the file to download.</param>
		/// <param name="localPath">
		///     Local file name where to store the content of the download.
		/// </param>
		/// <param name="timeOut">Duration in miliseconds before cancelling the  operation.</param>
		/// <returns>A tuple containing the remote path, the local path and an exception if one occurred.</returns>
		public static async Task<Tuple<string, string, Exception>> DownloadFileTaskAsync( string remotePath, string localPath, int timeOut = 3000 ) {
			try {
				if( remotePath == null ) {
					throw new ArgumentNullException( "remotePath" );
				}

				if( localPath == null ) {
					throw new ArgumentNullException( "localPath" );
				}

				using( var client = new WebClient()) {
					TimerCallback timerCallback = c => {
						var webClient = (WebClient)c;

						if(!webClient.IsBusy ) {
							return;
						}

						webClient.CancelAsync();
					};

					client.Headers.Add ("user-agent", Constants.ApplicationName );

					using( new Timer( timerCallback, client, timeOut, Timeout.Infinite )) {
						await client.DownloadFileTaskAsync( remotePath, localPath );
					}

					return new Tuple<string, string, Exception>( remotePath, localPath, null );
				}
			}
			catch( Exception ex ) {
				return new Tuple<string, string, Exception>( remotePath, localPath, ex );
			}
		}
	}

	public static class Extensions {
		public static Task ForEachAsync<TSource, TResult>( this IEnumerable<TSource> source, 
														   Func<TSource, Task<TResult>> taskSelector, Action<TSource, TResult> resultProcessor ) {
			var oneAtATime = new SemaphoreSlim( 5, 10 );

			return Task.WhenAll( from item in source select ProcessAsync( item, taskSelector, resultProcessor, oneAtATime ));
		}

		private static async Task ProcessAsync<TSource, TResult>( TSource item,
																  Func<TSource, Task<TResult>> taskSelector, Action<TSource, TResult> resultProcessor,
																  SemaphoreSlim oneAtATime ) {
			TResult result = await taskSelector( item );

			await oneAtATime.WaitAsync();

			try {
				resultProcessor( item, result );
			}
			finally {
				oneAtATime.Release();
			}
		}
	}
}
