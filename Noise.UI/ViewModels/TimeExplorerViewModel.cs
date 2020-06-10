using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Logging;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class YearList {
		public int				Year { get; }
		public List<DbAlbum>	Albums { get; }
		public double			YearPercentage { get; set; }

        public int				AlbumsInYear => Albums.Count;
        public bool				ShouldDisplayYear => AlbumsInYear > 0;
        public string			Title => $"{Year} - {AlbumsInYear} albums";

		public YearList( int year ) {
			Year = year;
			Albums = new List<DbAlbum>();
		}
    }

	public class DecadeList {
		private readonly Action<YearList>	mOnYearSelected;

		public int				Decade { get; }
		public List<YearList>	YearList { get; }
		public double			DecadePercentage { get; set; }
        public string			Title => $"{Decade}'s";

		public DecadeList( int decade, Action<YearList> onYearSelected ) {
			Decade = decade;
			mOnYearSelected = onYearSelected;

			YearList = new List<YearList>();

			for( int year = Decade; year < Decade + 10; year++ ) {
				YearList.Add( new YearList( year ));
			}
		}

        public int AlbumsInDecade {
			get{ return( YearList.Sum( y => y.AlbumsInYear )); }
		}

		public YearList SelectedYear {
			get => null;
            set => mOnYearSelected( value );
        }
	}

	internal class TimeExplorerViewModel : PropertyChangeBase,
										   IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IUiLog				mLog;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly BindableCollection<DecadeList>	mDecadeList;
		private TaskHandler					mAlbumLoaderTaskHandler;

		public TimeExplorerViewModel( IEventAggregator eventAggregator, ILibraryConfiguration libraryConfiguration, IAlbumProvider albumProvider, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mAlbumProvider = albumProvider;

			mDecadeList = new BindableCollection<DecadeList>();

			mEventAggregator.Subscribe( this );

			if( libraryConfiguration.Current != null ) {
				LoadAlbums();
			}
		}

		public void Handle( Events.DatabaseOpened args ) {
			LoadAlbums();
		}

		public void Handle( Events.DatabaseClosing args ) {
			mDecadeList.Clear();
		}

		public BindableCollection<DecadeList> DecadeList => mDecadeList;

        public void OnYearSelected( YearList year ) {
			mEventAggregator.PublishOnUIThread( new Events.TimeExplorerAlbumFocus( year.Albums ));	
		}

		internal TaskHandler AlbumLoaderTask {
			get {
				if( mAlbumLoaderTaskHandler == null ) {
					Execute.OnUIThread( () => mAlbumLoaderTaskHandler = new TaskHandler());
				}

				return( mAlbumLoaderTaskHandler );
			}

			set => mAlbumLoaderTaskHandler = value;
        }

		private void LoadAlbums() {
			AlbumLoaderTask.StartTask( () => {
				mDecadeList.Clear();
				mDecadeList.IsNotifying = false;

				using( var albumList = mAlbumProvider.GetAllAlbums() ) {
					var decadeGroups = from album in albumList.List
									   where album.PublishedYear != Constants.cUnknownYear && album.PublishedYear != Constants.cVariousYears
									   group album by album.PublishedYear / 10 into decadeGroup
									   orderby decadeGroup.Key
									   select new { Decade = decadeGroup.Key, Albums = decadeGroup };

					foreach( var decadeGroup in decadeGroups ) {
						var decadeList = new DecadeList( decadeGroup.Decade * 10, OnYearSelected );
						var yearGroups = from album in decadeGroup.Albums
										 group album by album.PublishedYear into yearGroup
										 orderby yearGroup.Key
										 select new { Year = yearGroup.Key, Albums = yearGroup };

						foreach( var yearGroup in yearGroups ) {
							var yearList = ( from year in decadeList.YearList where year.Year == yearGroup.Year select year ).FirstOrDefault();

							if( yearList != null ) {
								yearList.Albums.AddRange( yearGroup.Albums );
							}
						}

						mDecadeList.Add( decadeList );
					}
				}
				var albumCount = mDecadeList.Sum( d => d.AlbumsInDecade );
				var maxInYear = ( from decade in mDecadeList from year in decade.YearList select year.AlbumsInYear ).Concat( new[]{ 0 } ).Max();

				foreach( var decade in mDecadeList ) {
					decade.DecadePercentage = (double)decade.AlbumsInDecade / albumCount;

					foreach( var year in decade.YearList ) {
						year.YearPercentage = (double)year.AlbumsInYear / maxInYear;
					}
				}

				mDecadeList.IsNotifying = true;
				mDecadeList.Refresh();
			},
			() => { },
			ex => mLog.LogException( "Loading Albums", ex ));
		}
	}
}
