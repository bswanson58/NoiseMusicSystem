using System;

namespace Noise.Infrastructure.Dto {
	public class SetMp3TagCommandArgs : BaseCommandArgs {
		public	bool	IsAlbum { get; private set; }

		private	UInt32	mPublishedYear;
		public	bool	SetPublishedYear { get; private set; }

		public SetMp3TagCommandArgs( long itemId, bool isAlbum ) :
			base( itemId ) {
			IsAlbum = isAlbum;
		}

		public SetMp3TagCommandArgs( long itemId, SetMp3TagCommandArgs copyInstance ) :
			base( itemId ) {

			if( copyInstance.SetPublishedYear ) {
				PublishedYear = copyInstance.PublishedYear;
			}
		}

		public UInt32 PublishedYear {
			get{ return( mPublishedYear ); }
			set {
				mPublishedYear = value;
				SetPublishedYear = true;
			}
		}
	}
}
