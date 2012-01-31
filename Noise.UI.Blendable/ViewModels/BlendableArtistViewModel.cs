﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Caliburn.Micro;
using Moq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;
using Noise.UI.ViewModels;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.TestSupport.Threading;

namespace Noise.UI.Blendable.ViewModels {
	[Export( typeof( IBlendableViewModelFactory ))]
	public class BlendableArtistViewModel : IBlendableViewModelFactory {

		public Type ViewModelType {
			get{ return( typeof( ArtistViewModel )); }
		}

		public object CreateViewModel() {
			// Set the ui dispatcher to run on the current thread.
			Execute.ResetWithoutDispatcher();

			var eventAggregator = new Mock<IEventAggregator>();
			var artistProvider = new Mock<IArtistProvider>();
			var discographyProvider = new Mock<IDiscographyProvider>();
			var tagManager = new Mock<ITagManager>();
			var databaseShell = new Mock<IDatabaseShell>();

			var vm = new ArtistViewModel( eventAggregator.Object, artistProvider.Object, discographyProvider.Object, tagManager.Object );
			// Set tpl tasks to use the current thread only.
			var taskScheduler = new CurrentThreadTaskScheduler();
			vm.TaskHandler = new TaskHandler<ArtistSupportInfo>( taskScheduler, taskScheduler );

			var artist = new DbArtist { Name = "The Rolling Stones", Website = "www.rollingstones.com", Rating = 50, IsFavorite = true };
			artistProvider.Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );

			var dbTextInfo = new DbTextInfo( 1, ContentType.Biography );
			var biography = new TextInfo( dbTextInfo );
			using ( var textStream = GetType().Assembly.GetManifestResourceStream( @"Noise.UI.Blendable.Resources.TheRollingStonesBiography.txt" )) {
				if( textStream != null ) {
					using ( var reader = new StreamReader( textStream )) {
						biography.Text = reader.ReadToEnd();
					}
				}
			}
			var dbArtwork = new DbArtwork( 1, ContentType.ArtistPrimaryImage );
			var artistImage = new Artwork( dbArtwork );
			using( var imageStream = GetType().Assembly.GetManifestResourceStream( @"Noise.UI.Blendable.Resources.TheRollingStones.jpg" )) {
				if( imageStream != null ) {
					using( var binaryReader = new BinaryReader(  imageStream )) {
						artistImage.Image = binaryReader.ReadBytes((int)imageStream.Length );
					}
				}
			}
			var	similarArtists = new DbAssociatedItemList( 1, ContentType.SimilarArtists );
			similarArtists.SetItems( new List<string> { "Alice Cooper", "Van Halen", "The Doodars", "Frank Sinatra", "The Doors" });
			var topAlbums = new DbAssociatedItemList( 1, ContentType.TopAlbums );
			topAlbums.SetItems( new List<string> { "Sticky Fingers", "Let It Bleed", "Get Yer Ya-Ya's Out!", "Some Girls" });
			var bandMembers = new DbAssociatedItemList( 1, ContentType.BandMembers );
			bandMembers.SetItems( new List<string> { "Mick Jagger", "Keith Richards", "Charlie Watts", "Ronnie Woods", });

			artistProvider.Setup( m => m.GetArtistSupportInfo( It.IsAny<long>()))
				.Returns( new ArtistSupportInfo( biography, artistImage, similarArtists, topAlbums, bandMembers ));

			var discographyList = new List<DbDiscographyRelease> { new DbDiscographyRelease( artist.DbId, "Let It Bleed", "LP", "Decca", 1970, DiscographyReleaseType.Release ),
																   new DbDiscographyRelease( artist.DbId, "Some Girls", "LP", "Decca", 1981, DiscographyReleaseType.Release ),
																   new DbDiscographyRelease( artist.DbId, "Undercover", "CD", "Decca", 1987, DiscographyReleaseType.Release )};
			discographyProvider.Setup( m => m.GetDiscography( It.IsAny<long>())).Returns( new DataProviderList<DbDiscographyRelease>( databaseShell.Object, discographyList ));

			tagManager.Setup(  m => m.GetGenre( It.IsAny<long>())).Returns( new DbGenre( 1 ) { Name = "classic rock" });

			vm.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			return( vm );
		}
	}
}
