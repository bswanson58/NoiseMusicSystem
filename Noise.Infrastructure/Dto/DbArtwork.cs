﻿using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbArtwork : ExpiringContent {
		public	long			FolderLocation { get; set; }
		public	InfoSource		Source { get; set; }
		public	bool			IsUserSelection { get; set; }
		public	string			Name { get; set; }
		public	Int16			Rotation { get; set; }

		protected DbArtwork( DbArtwork clone ) :
			base( clone ) {
			FolderLocation = clone.FolderLocation;
			Source = clone.Source;
			IsUserSelection = clone.IsUserSelection;
			Name = clone.Name;
			Rotation = clone.Rotation;
		}

		public DbArtwork( long associatedItem, ContentType contentType ) :
			base( associatedItem, contentType ) {
			Source = InfoSource.Unknown;
			FolderLocation = Constants.cDatabaseNullOid;
			IsUserSelection = false;
			Rotation = 0;
		}

		public void Copy( DbArtwork copy ) {
			base.Copy( copy );

			FolderLocation = copy.FolderLocation;
			Source = copy.Source;
			IsUserSelection = copy.IsUserSelection;
			Name = copy.Name;
			Rotation = copy.Rotation;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbArtwork )); }
		}
	}
}
