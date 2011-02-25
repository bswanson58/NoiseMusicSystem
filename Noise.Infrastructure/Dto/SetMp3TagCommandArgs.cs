using System;

namespace Noise.Infrastructure.Dto {
	public class SetMp3TagCommandArgs : BaseCommandArgs {
		public	bool	IsAlbum { get; private set; }

		private	UInt32	mPublishedYear;
		public	bool	SetPublishedYear { get; private set; }

		private string	mName;
		public	bool	SetName { get; private set; }

		public SetMp3TagCommandArgs( DbTrack track ) :
			base( track.DbId ) {
		}

		public SetMp3TagCommandArgs( DbAlbum album ) :
			base( album.DbId ) {
			IsAlbum = true;
		}

		public SetMp3TagCommandArgs( DbTrack track, SetMp3TagCommandArgs copyInstance ) :
			base( track.DbId ) {

			if( copyInstance.SetPublishedYear ) {
				PublishedYear = copyInstance.PublishedYear;
			}

			if( copyInstance.SetName ) {
				Name = copyInstance.Name;
			}
		}

		public string Name {
			get{ return( mName ); }
			set {
				mName = value;
				SetName = true;
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
