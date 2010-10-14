using System.Collections;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class SelectPlayListDialogModel : DialogModelBase {
		private readonly List<DbPlayList>	mPlayLists;
		public	DbPlayList					SelectedItem { get; set; }

		public SelectPlayListDialogModel( IUnityContainer container ) {
			var noiseManager = container.Resolve<INoiseManager>();

			mPlayLists = noiseManager.PlayListMgr.PlayLists;
		}

		public IEnumerable<DbPlayList> PlayLists {
			get{ return( mPlayLists ); }
		}
	}
}
