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

		public YearList( int year ) {
			Year = year;
			Albums = new List<DbAlbum>();
		}
	}

	public class DecadeList {
		public int				Decade { get; private set; }
		public List<YearList>	YearList { get; private set; }

		public DecadeList( int decade ) {
			Decade = decade;
			YearList = new List<YearList>();
		}
	}

	public class TimeExplorerViewModel : AutomaticCommandBase,
										 IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly BindableCollection<DecadeList>	mDecadeList;

		public TimeExplorerViewModel( IEventAggregator eventAggregator, IAlbumProvider albumProvider ) {
			mEventAggregator = eventAggregator;
			mAlbumProvider = albumProvider;

			mDecadeList = new BindableCollection<DecadeList>();

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.DatabaseOpened args ) {
			LoadAlbums();
		}

		public void Handle( Events.DatabaseClosing args ) {
			mDecadeList.Clear();
		}

		private void LoadAlbums() {
			using( var albumList = mAlbumProvider.GetAllAlbums() ) {
				var decadeGroups = from album in albumList.List
								   where album.PublishedYear != 0
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
						var yearList = new YearList( yearGroup.Year );

						yearList.Albums.AddRange( yearGroup.Albums );
						decadeList.YearList.Add( yearList );
					}

					mDecadeList.Add( decadeList );
				}
			}
		}
	}
}
