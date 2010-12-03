using Noise.Infrastructure.Support;

namespace Noise.Infrastructure.Interfaces {
	public static class LicenseKeys {
		public static string	BassAudio		= "BassAudio";
		public static string	Discogs			= "Discogs";
		public static string	LastFm			= "LastFm";
	}

	public interface ILicenseManager {
		bool		Initialize( string licenseFile );

		LicenseKey	RetrieveKey( string keyId );
	}
}
