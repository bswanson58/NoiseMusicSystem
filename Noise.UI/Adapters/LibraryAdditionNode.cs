using System;
using System.Collections.Generic;
using AutoMapper;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;

namespace Noise.UI.Adapters {
	public class LibraryAdditionNode : ViewModelBase {
		public	DbArtist		Artist { get; private set; }
		public	DbAlbum			Album { get; private set; }
		public	List<UiTrack>	TrackList { get; private set; }
		private readonly Action<LibraryAdditionNode>	mAlbumPlayAction;
		private readonly Action<long>					mTrackPlayAction;
		private readonly Action<LibraryAdditionNode>	mAlbumSelectAction;

		public LibraryAdditionNode( DbArtist artist, DbAlbum album, DbTrack track,
									Action<LibraryAdditionNode> selectAction, Action<LibraryAdditionNode> albumPlayAction, Action<long> trackPlayAction ) {
			Artist = artist;
			Album = album;

			mAlbumPlayAction = albumPlayAction;
			mTrackPlayAction = trackPlayAction;
			mAlbumSelectAction = selectAction;

			var uiTrack = new UiTrack( mTrackPlayAction, OnTrackSelected );
			Mapper.DynamicMap( track, uiTrack );
			TrackList = new List<UiTrack> { uiTrack };
		}

		public string Name {
			get{ return( String.Format( "{0}/{1}", Artist.Name, Album.Name )); }
		}

		public void AddTrack( DbTrack track ) {
			var uiTrack = new UiTrack( mTrackPlayAction, OnTrackSelected );
			Mapper.DynamicMap( track, uiTrack );

			TrackList.Add( uiTrack );
		}

		public bool IsExpanded {
			get { return( Get( () => IsExpanded )); }
			set { Set( () => IsExpanded, value  ); }
		}

		public bool IsSelected {
			get { return( Get( () => IsSelected )); }
			set {
				Set( () => IsSelected, value  );
				if( value ) {
					mAlbumSelectAction( this );
				}
			}
		}

		public void Execute_Play( object sender ) {
			mAlbumPlayAction( this );
		}

		private void OnTrackSelected( long trackId ) {
			mAlbumSelectAction( this );
		}
	}
}
