﻿using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class SelectPlayListDialogModel : DialogModelBase {
		private readonly List<DbPlayList>	mPlayLists;
		public	DbPlayList					SelectedItem { get; set; }

		public SelectPlayListDialogModel( IPlayListMgr playListMgr ) {
			mPlayLists = playListMgr.PlayLists;
		}

		public IEnumerable<DbPlayList> PlayLists {
			get{ return( mPlayLists ); }
		}
	}
}
