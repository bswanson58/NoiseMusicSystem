using System;
using System.Globalization;
using System.Windows.Data;
using Noise.Infrastructure.Dto;

namespace Noise.UI.ValueConverters {
	public class PlayQueueTrackNameConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = "";

			if( value is PlayQueueTrack ) {
				var track = value as PlayQueueTrack;

				if(( track.Artist != null ) &&
				   ( track.Track != null )) {
					retValue = string.Format( "{0}/{1}", track.Artist.Name, track.Track.Name );
				}
				else {
					retValue = "Unknown";
				}
			}

			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
