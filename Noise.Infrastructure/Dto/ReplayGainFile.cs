using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay( "File = {FilePath}" )]
	public class ReplayGainFile {
		public	long		FileId { get; private set; }
		public	string		FilePath { get; private set; }
		public	double		TrackGain { get; private set; }
		public	bool		Success { get; private set; }
		public	string		FailureMessage { get; private set; }

		public ReplayGainFile( long id, string filePath ) {
			FilePath = filePath;
			FileId = id;
		}

		public void SetTrackGain( double gain ) {
			TrackGain = gain;

			Success = true;
		}

		public void SetTrackFailure( string message ) {
			Success = false;
			FailureMessage = message;
		}
	}
}
