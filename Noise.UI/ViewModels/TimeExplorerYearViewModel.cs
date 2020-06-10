using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class TimeExplorerYearViewModel : AutomaticPropertyBase, IHandle<Events.TimeExplorerAlbumFocus> {
		private readonly IEventAggregator	mEventAggregator;
		private IEnumerable<DbAlbum>		mAlbumList;

		public	DelegateCommand				PlayRandom { get; }

		public TimeExplorerYearViewModel( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;

			PlayRandom = new DelegateCommand( OnPlayRandom, CanPlayRandom );
            YearValid = false;

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.TimeExplorerAlbumFocus args ) {
			mAlbumList = args.AlbumList;

			AlbumCount = args.AlbumList.Count();
			YearValid = AlbumCount > 0;

			if( YearValid ) {
				CurrentYear = args.AlbumList.First().PublishedYear;
				ArtistCount = args.AlbumList.Select( x => x.Artist ).Distinct().Count();
			}
		}

		public bool YearValid {
			get{ return( Get( () => YearValid )); }
			set {
                Set( () => YearValid, value );

				PlayRandom.RaiseCanExecuteChanged();
            }
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

		private void OnPlayRandom() {
			if( CanPlayRandom()) {
				mEventAggregator.PublishOnUIThread( new Events.PlayAlbumTracksRandom( mAlbumList ));
			}
		}

		private bool CanPlayRandom() {
			return(( mAlbumList != null ) &&
				   ( mAlbumList.Any()));
		}
	}
}
