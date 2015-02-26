using System;
using System.Runtime.CompilerServices;

namespace Noise.Metadata.Logging {
	internal interface ILogMetadata {
		void	LoadedMetadata( string provider, string forArtist );
		void	ArtistNotFound( string provider, string forArtist );

		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
