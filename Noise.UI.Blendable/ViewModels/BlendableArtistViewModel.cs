using System;
using System.ComponentModel.Composition;
using System.IO;
using Caliburn.Micro;
using Moq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;
using Noise.UI.ViewModels;
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
			var artworkProvider = new Mock<IArtworkProvider>();
			var tagManager = new Mock<ITagManager>();

			var vm = new ArtistViewModel( eventAggregator.Object, artistProvider.Object, artworkProvider.Object, tagManager.Object );
			// Set tpl tasks to use the current thread only.
			var taskScheduler = new CurrentThreadTaskScheduler();
			vm.ArtistTaskHandler = new TaskHandler<DbArtist>( taskScheduler, taskScheduler );
			vm.ArtworkTaskHandler = new TaskHandler<Artwork>( taskScheduler, taskScheduler );

			var artist = new DbArtist { Name = "The Rolling Stones", Website = "www.rollingstones.com", Rating = 50, IsFavorite = true };
			artistProvider.Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );

			var dbArtwork = new DbArtwork( 1, ContentType.ArtistPrimaryImage );
			var artistImage = new Artwork( dbArtwork );
			using( var imageStream = GetType().Assembly.GetManifestResourceStream( @"Noise.UI.Blendable.Resources.TheRollingStones.jpg" )) {
				if( imageStream != null ) {
					using( var binaryReader = new BinaryReader(  imageStream )) {
						artistImage.Image = binaryReader.ReadBytes((int)imageStream.Length );
					}
				}
			}
			artworkProvider.Setup( m => m.GetArtistArtwork( It.IsAny<long>(), It.IsAny<ContentType>())).Returns( artistImage );

			tagManager.Setup(  m => m.GetGenre( It.IsAny<long>())).Returns( new DbGenre( 1 ) { Name = "classic rock" });

			vm.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			return( vm );
		}
	}
}
