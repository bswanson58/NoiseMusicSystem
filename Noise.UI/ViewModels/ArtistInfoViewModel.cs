﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Microsoft.Practices.Prism;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class ArtistInfoViewModel : AutomaticCommandBase, IActiveAware, IHandle<Events.ViewDisplayRequest>, IHandle<Events.DatabaseClosing>,
									   IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>,
									   IHandle<Events.ArtistMetadataUpdated> {
		private readonly IEventAggregator				mEventAggregator;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IMetadataManager				mMetadataManager;
		private long									mCurrentArtistId;
		private string									mCurrentArtistName;
		private TaskHandler								mTaskHandler; 
		private readonly BindableCollection<LinkNode>	mSimilarArtists;
		private readonly BindableCollection<LinkNode>	mTopAlbums;
		private readonly BindableCollection<LinkNode>	mBandMembers;
		private readonly BindableCollection<DbDiscographyItem>	mDiscography;

		public	event	EventHandler					IsActiveChanged;

		public ArtistInfoViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider, IMetadataManager metadataManager ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mMetadataManager = metadataManager;
			mCurrentArtistId = Constants.cDatabaseNullOid;
			mCurrentArtistName = string.Empty;

			mEventAggregator.Subscribe( this );

			mSimilarArtists = new BindableCollection<LinkNode>();
			mTopAlbums = new BindableCollection<LinkNode>();
			mBandMembers = new BindableCollection<LinkNode>();
			mDiscography = new SortableCollection<DbDiscographyItem>();
		}

		public bool IsActive {
			get{ return( Get( () => IsActive )); }
			set{ Set( () => IsActive, value ); }
		}

		private void ClearCurrentArtist() {
			mSimilarArtists.Clear();
			mTopAlbums.Clear();
			mBandMembers.Clear();
			mDiscography.Clear();
			ArtistBiography = string.Empty;
			mCurrentArtistId = Constants.cDatabaseNullOid;
			mCurrentArtistName = string.Empty;

			ArtistValid = false;
		}

		private void SetCurrentArtist( long artistId ) {
			if( mCurrentArtistId != artistId ) {
				ClearCurrentArtist();

				mCurrentArtistId = artistId;

				var	artist = mArtistProvider.GetArtist( artistId );
				if( artist != null ) {
					mCurrentArtistName = artist.Name;

					RetrieveArtistMetadata( mCurrentArtistName );
				}
			}
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearCurrentArtist();
		}

		public void Handle( Events.ArtistFocusRequested request ) {
			SetCurrentArtist( request.ArtistId );

			if(!IsActive ) {
				mEventAggregator.Publish( new Events.ViewDisplayRequest( ViewNames.ArtistInfoView ));
			}
		}

		public void Handle( Events.AlbumFocusRequested request ) {
			SetCurrentArtist( request.ArtistId );
		}

		public void Handle( Events.ArtistMetadataUpdated eventArgs ) {
			if( string.Equals( mCurrentArtistName, eventArgs.ArtistName )) {
				RetrieveArtistMetadata( mCurrentArtistName );
			}
		}

		internal TaskHandler TaskHandler {
			get {
				if( mTaskHandler == null ) {
					Execute.OnUIThread( () => mTaskHandler = new TaskHandler());
				}

				return( mTaskHandler );
			}

			set { mTaskHandler = value; }
		} 

		private void RetrieveArtistMetadata( string artistName ) {
			TaskHandler.StartTask( () => {
									var info = mMetadataManager.GetArtistMetadata( artistName );

									ArtistBiography = info.GetMetadata( eMetadataType.Biography );

									mBandMembers.Clear();
									mBandMembers.AddRange( info.GetMetadataArray( eMetadataType.BandMembers ).Select( item => new LinkNode( item )));

									mSimilarArtists.Clear();
									mSimilarArtists.AddRange( info.GetMetadataArray( eMetadataType.SimilarArtists ).Select( item => new LinkNode( item )));

									mTopAlbums.Clear();
									mTopAlbums.AddRange( info.GetMetadataArray( eMetadataType.TopAlbums ).Select( item => new LinkNode( item )));

									var discography = mMetadataManager.GetArtistDiscography( artistName );
									mDiscography.Clear();
									mDiscography.AddRange( from d in discography.Discography orderby d.Year descending select  d );
								},
								() => ArtistValid = true,
								exception => NoiseLogger.Current.LogException( "ArtistInfoViewModel:RetrieveSupportInfo", exception )
				);
		}

//		private void OnSimilarArtistClicked( long artistId ) {
//			mEventAggregator.Publish( new Events.ArtistFocusRequested( artistId ));
//		}

//		private void OnTopAlbumClicked( long albumId ) {
//			mEventAggregator.Publish( new Events.AlbumFocusRequested( mCurrentArtistId, albumId ));
//		}

		public void Handle( Events.ViewDisplayRequest eventArgs ) {
			if( ViewNames.ArtistInfoView.Equals( eventArgs.ViewName )) {
				IsDisplayed = !IsDisplayed;

				eventArgs.ViewWasOpened = IsDisplayed;
			}
		}

		public bool IsDisplayed {
			get{ return( Get( () => IsDisplayed )); }
			set{ Set( () => IsDisplayed, value ); }
		}

		public bool ArtistValid {
			get{ return( Get( () => ArtistValid )); }
			set{ Set( () => ArtistValid, value ); }
		}

		public string ArtistBiography {
			get { return( Get( () => ArtistBiography )); }
			set { Set( () => ArtistBiography, value ); }
		}

		public IEnumerable<LinkNode> TopAlbums {
			get{ return( mTopAlbums ); }
		}

		public IEnumerable<LinkNode> SimilarArtist {
			get { return( mSimilarArtists ); }
		}

		public IEnumerable<LinkNode> BandMembers {
			get { return( mBandMembers ); }
		}

		public IEnumerable<DbDiscographyItem> Discography {
			get{ return( mDiscography ); }
		}
	}
}
