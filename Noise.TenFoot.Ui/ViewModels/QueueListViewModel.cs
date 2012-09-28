using Caliburn.Micro;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Interfaces;
using Noise.UI.Support;
using Noise.UI.ViewModels;

namespace Noise.TenFoot.Ui.ViewModels {
	public class QueueListViewModel : PlayQueueViewModel, ITitledScreen {
		public	string	Title { get; private set; }
		public	string	Context { get; private set; }

		public QueueListViewModel( IEventAggregator eventAggregator, ITagProvider tagProvider, IGenreProvider genreProvider,
								   IInternetStreamProvider internetStreamProvider, IPlayQueue playQueue,
								   IPlayListProvider playListProvider, IDialogService dialogService ) :
			base( eventAggregator, tagProvider, genreProvider, internetStreamProvider, playQueue, playListProvider, dialogService ) {
			Title = "Now Playing";
			Context = string.Empty;
		}
	}
}
