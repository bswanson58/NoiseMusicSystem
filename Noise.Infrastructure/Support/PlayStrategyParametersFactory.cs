using System;
using Newtonsoft.Json;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Support {
	public static class PlayStrategyParametersFactory {
		public static IPlayStrategyParameters FromString( string encoded ) {
			IPlayStrategyParameters	retValue = null;

			if((!string.IsNullOrWhiteSpace( encoded )) &&
			   (!encoded.Equals( "null", StringComparison.OrdinalIgnoreCase ))) {
				try {
					dynamic	jsonObject = JsonConvert.DeserializeObject( encoded );

					if( jsonObject.ParameterType == typeof( PlayStrategyParameterDbId ).Name ) {
						retValue = JsonConvert.DeserializeObject<PlayStrategyParameterDbId>( encoded );
					}
				}
				catch( Exception ex ) {
					if( !string.IsNullOrWhiteSpace( encoded ) ) {
						NoiseLogger.Current.LogException( string.Format( "PlayStrategyParametersFactory failed to decode: '{0}'", encoded ), ex );
					}
				}
			}
			return( retValue );
		}

		public static string ToString( IPlayStrategyParameters parameters ) {
			return( JsonConvert.SerializeObject( parameters ));
		}
	}
}
