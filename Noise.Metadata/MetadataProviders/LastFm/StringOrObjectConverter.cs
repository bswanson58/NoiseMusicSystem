using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Noise.Metadata.MetadataProviders.LastFm {
	public class SingleOrObjectConverter<T> : JsonConverter where T : new() {
		public override bool CanConvert( Type objectType ) {
			return ( objectType == typeof( T ));
		}

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer ) {
			var token = JToken.Load( reader );

			if( token.Type == JTokenType.Object ) {
				return token.ToObject<T>();
			}

			return( new T());
		}

		public override bool CanWrite {
			get { return( false ); }
		}

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer ) {
			throw new NotImplementedException();
		}
	}
}
