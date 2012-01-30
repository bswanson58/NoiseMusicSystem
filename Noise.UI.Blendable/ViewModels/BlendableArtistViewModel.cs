using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Moq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.ViewModels;

namespace Noise.UI.Blendable.ViewModels {
	[Export( typeof( IBlendableViewModelFactory ))]
	public class BlendableArtistViewModel : IBlendableViewModelFactory {
		private readonly Mock<IEventAggregator>		mEventAggregator;
		private readonly Mock<IArtistProvider>		mArtistProvider;
		private readonly Mock<IDiscographyProvider>	mDiscographyProvider;
		private readonly Mock<ITagManager>			mTagManager;
		private readonly Mock<IDatabaseShell>		mDatabaseShell;

		public BlendableArtistViewModel() {
			mEventAggregator = new Mock<IEventAggregator>();
			mArtistProvider = new Mock<IArtistProvider>();
			mDiscographyProvider = new Mock<IDiscographyProvider>();
			mTagManager = new Mock<ITagManager>();
			mDatabaseShell = new Mock<IDatabaseShell>();
		}

		public Type ViewModelType {
			get{ return( typeof( ArtistViewModel )); }
		}

		public object CreateViewModel() {
			var vm = new ArtistViewModel( mEventAggregator.Object, mArtistProvider.Object, mDiscographyProvider.Object, mTagManager.Object );

			var artist = new DbArtist { Name = "The Rolling Stones", Website = "www.rollingstones.com", Rating = 50, IsFavorite = true };
			var dbTextInfo = new DbTextInfo( 1, ContentType.Biography );
			var biography = new TextInfo( dbTextInfo ) { Text = "Blah, blah, blah" };
			var dbArtwork = new DbArtwork( 1, ContentType.ArtistPrimaryImage );
			var artistImage = new Artwork( dbArtwork );
			var	similarArtists = new DbAssociatedItemList( 1, ContentType.SimilarArtists );
			similarArtists.SetItems( new List<string> { "Alice Cooper", "Van Halen", "The Doodars", "Frank Sinatra", "The Doors" });
			var topAlbums = new DbAssociatedItemList( 1, ContentType.TopAlbums );
			topAlbums.SetItems( new List<string> { "Sticky Fingers", "Let It Bleed", "Get Yer Ya-Ya's Out!", "Some Girls" });
			var bandMembers = new DbAssociatedItemList( 1, ContentType.BandMembers );
			bandMembers.SetItems( new List<string> { "Mick Jagger", "Keith Richards", "Charlie Watts", "Ronnie Woods", });

			mArtistProvider.Setup( m => m.GetArtist( It.IsAny<long>() ) ).Returns( artist );
			mArtistProvider.Setup( m => m.GetArtistSupportInfo( It.IsAny<long>()))
				.Returns( new ArtistSupportInfo( biography, artistImage, similarArtists, topAlbums, bandMembers ));
			var discographyList = new List<DbDiscographyRelease> { new DbDiscographyRelease( artist.DbId, "Let It Bleed", "LP", "Decca", 1970, DiscographyReleaseType.Release ),
																   new DbDiscographyRelease( artist.DbId, "Some Girls", "LP", "Decca", 1981, DiscographyReleaseType.Release ),
																   new DbDiscographyRelease( artist.DbId, "Undercover", "CD", "Decca", 1987, DiscographyReleaseType.Release )};
			mDiscographyProvider.Setup( m => m.GetDiscography( It.IsAny<long>())).Returns( new DataProviderList<DbDiscographyRelease>( mDatabaseShell.Object, discographyList ));

			mTagManager.Setup(  m => m.GetGenre( It.IsAny<long>())).Returns( new DbGenre( 1 ) { Name = "classic rock" });

			vm.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			return( vm );
		}
	}
}
