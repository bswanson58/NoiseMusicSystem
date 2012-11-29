﻿using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class TimeExplorerYearViewModel : AutomaticCommandBase, IHandle<Events.TimeExplorerAlbumFocus> {
		private readonly IEventAggregator	mEventAggregator;

		public TimeExplorerYearViewModel( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;

			YearValid = false;

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.TimeExplorerAlbumFocus args ) {
			AlbumCount = args.AlbumList.Count();
			YearValid = AlbumCount > 0;

			if( YearValid ) {
				CurrentYear = args.AlbumList.First().PublishedYear;
				ArtistCount = args.AlbumList.Select( x => x.Artist ).Distinct().Count();
			}
		}

		public bool YearValid {
			get{ return( Get( () => YearValid )); }
			set{ Set( () => YearValid, value ); }
		}

		public int CurrentYear {
			get{ return( Get( () => CurrentYear )); }
			set{ Set( () => CurrentYear, value ); }
		}

		public int ArtistCount {
			get{ return( Get( () => ArtistCount )); }
			set{ Set( () => ArtistCount, value ); }
		}

		public int AlbumCount {
			get{ return( Get( () => AlbumCount )); }
			set{ Set( () => AlbumCount, value ); }
		}
	}
}