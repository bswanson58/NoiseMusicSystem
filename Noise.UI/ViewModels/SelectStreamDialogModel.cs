using System.Collections.Generic;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class SelectStreamDialogModel : DialogModelBase {
		private readonly List<DbInternetStream>	mStreamList;
		public	DbInternetStream				SelectedItem { get; set; }

		public SelectStreamDialogModel( IUnityContainer container ) {
			var noiseManager = container.Resolve<INoiseManager>();

			using( var streamList = noiseManager.DataProvider.GetStreamList()) {
				mStreamList = new List<DbInternetStream>( streamList.List );
			}
		}

		public IEnumerable<DbInternetStream> StationList {
			get{ return( mStreamList ); }
		}
	}
}
