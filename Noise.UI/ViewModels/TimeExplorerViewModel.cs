using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class YearList {
		public int				Year { get; private set; }
		public List<DbAlbum>	Albums { get; private set; }
		public double			YearPercentage { get; set; }

		public YearList( int year ) {
			Year = year;
			Albums = new List<DbAlbum>();
		}

		public int AlbumsInYear {
			get{ return( Albums.Count ); }
		}

		public bool ShouldDisplayYear {
			get{ return( AlbumsInYear > 0 ); }
		}

		public string Title {
			get{ return( string.Format( "{0} - {1} albums", Year, AlbumsInYear )); }
		}
	}

	public class DecadeList {
		public int				Decade { get; private set; }
		public List<YearList>	YearList { get; private set; }
		public double			DecadePercentage { get; set; }

		public DecadeList( int decade ) {
			Decade = decade;
			YearList = new List<YearList>();

			for( int year = Decade; year < Decade + 10; year++ ) {
				YearList.Add( new YearList( year ));
			}
		}

		public string Title {
			get{ return( string.Format( "{0}'s", Decade )); }
		}

		public int AlbumsInDecade {
			get{ return( YearList.Sum( y => y.AlbumsInYear )); }
		}
	}

	public class TimeExplorerViewModel : AutomaticCommandBase,
										 IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly BindableCollection<DecadeList>	mDecadeList;

		public TimeExplorerViewModel( IEventAggregator eventAggregator, ILibraryConfiguration libraryConfiguration, IAlbumProvider albumProvider ) {
			mEventAggregator = eventAggregator;
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

		public BindableCollection<DecadeList> DecadeList {
			get{ return( mDecadeList ); }
		} 

		private void LoadAlbums() {
			using( var albumList = mAlbumProvider.GetAllAlbums() ) {
				var decadeGroups = from album in albumList.List
								   where album.PublishedYear != Constants.cUnknownYear && album.PublishedYear != Constants.cVariousYears
								   group album by album.PublishedYear / 10 into decadeGroup
								   orderby decadeGroup.Key
								   select new { Decade = decadeGroup.Key, Albums = decadeGroup };

				foreach( var decadeGroup in decadeGroups ) {
					var decadeList = new DecadeList( decadeGroup.Decade * 10 );
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
		}
	}
}
