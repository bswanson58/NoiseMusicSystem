using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Prism.Commands;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Stream = {" + nameof(Name) + "}")]
	public class UiInternetStream : UiBase {
        private readonly Action<UiInternetStream>	mOnClick;
        private readonly Action<long>				mOnPlay;

        [Required( AllowEmptyStrings=false, ErrorMessage = "Empty stream names are not allowed.")]
		public	string					Name { get; set; }
		public	string					Description { get; set; }
		[Required( AllowEmptyStrings=false, ErrorMessage = "Empty stream Urls are not allowed.")]
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
        public	bool					IsUserRating => true;
        public	bool					IsLinked => !string.IsNullOrWhiteSpace( Website );

		public	DelegateCommand			LinkClicked { get; }
		public	DelegateCommand			PlayStream { get; }

        protected UiInternetStream() {
            DateAdded = DateTime.Now;
            Encoding = eAudioEncoding.Unknown;

            Description = String.Empty;
            Website = String.Empty;

            ExternalGenre = Constants.cDatabaseNullOid;
            UserGenre = Constants.cDatabaseNullOid;

            LinkClicked = new DelegateCommand( OnLinkClicked );
			PlayStream = new DelegateCommand( OnPlayStream );
        }

		public UiInternetStream( DbInternetStream stream, Action<UiInternetStream> onClick, Action<long> onPlay ) :
            this () {
			mOnClick = onClick;
			mOnPlay = onPlay;

			UpdateFromSource( stream );
		}

		private void UpdateFromSource( DbInternetStream stream ) {
			if( stream != null ) {
				DbId = stream.DbId;

				Name = stream.Name;
				Description = stream.Description;
				Url = stream.Url;
				Bitrate = stream.Bitrate;
				Channels = stream.Channels;
				Rating = stream.Rating;
				DateAdded = stream.DateAdded;
				Encoding = stream.Encoding;
				ExternalGenre = stream.ExternalGenre;
				UserGenre = stream.UserGenre;
				IsPlaylistWrapped = stream.IsPlaylistWrapped;
				IsFavorite = stream.IsFavorite;
				Website = stream.Website;

				UiIsFavorite = stream.IsFavorite;
				UiRating = stream.Rating;
            }
        }

		public long Genre {
			get => ( UserGenre == Constants.cDatabaseNullOid ?  ExternalGenre : UserGenre );
            set => UserGenre = value;
        }

        private void OnLinkClicked() {
            mOnClick?.Invoke( this );
        }

		private void OnPlayStream() {
            mOnPlay?.Invoke( DbId );
        }
	}
}
