using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	internal class ImportStreams : IDataImport {
		private readonly IUnityContainer	mContainer;
		private readonly INoiseManager		mNoiseManager;

		public ImportStreams( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
		}

		public int Import( XElement rootElement, bool eliminateDuplicates ) {
			var retValue = 0;
			var streamList = from element in rootElement.Descendants( ExchangeConstants.cStreamItem ) select element;

			using( var currentStreams = mNoiseManager.DataProvider.GetStreamList()) {
				foreach( var stream in streamList ) {
					var dbStream = new DbInternetStream { Name = (string)stream.Element( ExchangeConstants.cName ),
														  Description = (string)stream.Element( ExchangeConstants.cDescription ),
														  Url = (string)stream.Element( ExchangeConstants.cStreamUrl ),
														  Website = (string)stream.Element( ExchangeConstants.cWebsite ) };
					var currentStream = ( from current in currentStreams.List 
										  where current.Name.Equals( dbStream.Name ) || current.Url.Equals( dbStream.Url ) 
										  select current ).FirstOrDefault();
					if( currentStream == null ) {
						mNoiseManager.DataProvider.InsertItem( dbStream );

						retValue++;
					}
				}
			}

			return( retValue );
		}
	}
}
