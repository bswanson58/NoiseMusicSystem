using System;
using System.Collections.Generic;
using CuttingEdge.Conditions;

namespace Noise.Metadata.Dto {
	public enum eMetadataType {
		BandMembers,
		Biography,
		Genre,
		SimilarArtists,
		TopAlbums,
		WebSite,
		Unknown
	}

	public class StringMetadata {
		private static readonly string cArraySeparator = Environment.NewLine;

		public	eMetadataType	MetadataType { get; set; }
		public	string				Metadata { get; set; }

		public StringMetadata() {
			MetadataType = eMetadataType.Unknown;
			Metadata = string.Empty;
		}

		public void FromArray( IEnumerable<string> array ) {
			Condition.Requires( array ).IsNotNull();

			Metadata = string.Join( cArraySeparator, array );
		}
 		public IEnumerable<string> ToArray() {
			return( Metadata.Split( new [] { cArraySeparator }, StringSplitOptions.None ));
		} 
	}
}
