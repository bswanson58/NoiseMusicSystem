using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Stream = {Name}")]
	public class UiInternetStream : UiBase {
		[Required( AllowEmptyStrings=false, ErrorMessage = "Emty stream names are not allowed.")]
		public	string					Name { get; set; }
		public	string					Description { get; set; }
		[Required( AllowEmptyStrings=false, ErrorMessage = "Emty stream Urls are not allowed.")]
		public	string					Url { get; set; }
		public	Int32					Bitrate { get; set; }
		public	Int16					Channels { get; set; }
		public	Int16					Rating { get; set; }
		public	DateTime				DateAdded { get; private set; }
		public	eAudioEncoding			Encoding { get; set; }
		public	long					ExternalGenre { get; set; }
		public	long					UserGenre { get; set; }
		public	bool					IsPlaylistWrapped { get; set; }
		public	bool					IsFavorite { get; set; }
		public	string					Website { get; set; }
		private readonly Action<UiInternetStream>	mOnClick;
		private readonly Action<long>				mOnPlay;

        protected UiInternetStream() { }

		public UiInternetStream( Action<UiInternetStream> onClick, Action<long> onPlay ) {
			mOnClick = onClick;
			mOnPlay = onPlay;

			DateAdded = DateTime.Now;
			Encoding = eAudioEncoding.Unknown;

			Description = "";
			Website = "";

			ExternalGenre = Constants.cDatabaseNullOid;
			UserGenre = Constants.cDatabaseNullOid;
		}

		public long Genre {
			get{ return( UserGenre == Constants.cDatabaseNullOid ?  ExternalGenre : UserGenre ); }
			set{ UserGenre = value; }
		}

		public bool IsUserRating {
			get{ return( true ); }
		}

		public bool IsLinked {
			get{ return(!string.IsNullOrWhiteSpace( Website )); }
		}

		public void Execute_LinkClicked() {
			if( mOnClick != null ) {
				mOnClick( this );
			}
		}

		public void Execute_PlayStream() {
			if( mOnPlay != null ) {
				mOnPlay( DbId );
			}
		}
	}
}
