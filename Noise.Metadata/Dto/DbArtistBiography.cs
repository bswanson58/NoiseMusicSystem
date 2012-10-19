﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Noise.Metadata.Dto {
	internal class DbArtistBiography {
		private readonly static IEnumerable<string>	mEmptyArray = new List<string>();
 
		public	string					ArtistName { get; set; }
		public	List<StringMetadata>	Metadata { get; set; }

 		public DbArtistBiography() {
 			ArtistName = string.Empty;
			Metadata = new List<StringMetadata>();
 		}

		public void SetMetadata( eMetadataType metadataType, string metadata ) {
			var current = ( from m in Metadata where m.MetadataType == metadataType select m ).FirstOrDefault();

			if( current == null ) {
				current = new StringMetadata { MetadataType = metadataType };

				Metadata.Add( current );
			}

			current.Metadata = metadata;
		}

		public void SetMetadata( eMetadataType metadataType, List<string> metadata ) {
			var current = ( from m in Metadata where m.MetadataType == metadataType select m ).FirstOrDefault();

			if( current == null ) {
				current = new StringMetadata { MetadataType = metadataType };

				Metadata.Add( current );
			}

			current.FromArray( metadata );
		}

		public string GetMetadata( eMetadataType metadataType ) {
			var retValue = string.Empty;
			var metadata = ( from m in Metadata where m.MetadataType == metadataType select m ).FirstOrDefault();

			if( metadata != null ) {
				retValue = metadata.Metadata;
			}

			return( retValue );
		}
		public IEnumerable<String> GetMetadataArray( eMetadataType metadataType ) {
			var retValue = mEmptyArray;

			var metadata = ( from m in Metadata where m.MetadataType == metadataType select m ).FirstOrDefault();

			if( metadata != null ) {
				retValue = metadata.ToArray();
			}

			return( retValue );
		}
	}
}
