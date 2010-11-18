using System;
using System.Collections.Generic;
using System.Linq;

namespace Noise.Infrastructure.Dto {
	public class ParametricBand {
		public long		BandId { get; private set; }
		public float	CenterFrequency { get; private set; }
		public float	Gain { get; set; }

		public ParametricBand( float centerFrequency ) {
			CenterFrequency = centerFrequency;
			Gain = 0.0f;

			BandId = BitConverter.ToInt64( Guid.NewGuid().ToByteArray(), 0 );
		}

		public ParametricBand( float centerFrequency, float gain ) :
			this( centerFrequency ) {
			Gain = gain;
		}

		public ParametricBand( long bandId, float centerFrequency, float gain ) :
			this( centerFrequency, gain ) {
			BandId = bandId;
		}
	}

	public class ParametricEqualizer {
		public long				EqualizerId { get; private set; }
		public string			Name { get; set; }
		public string			Description { get; set; }
		public float			Bandwidth { get; set; }
		public bool				IsPreset { get; private set; }

		private	readonly List<ParametricBand>	mBands;

		public ParametricEqualizer() {
			EqualizerId = BitConverter.ToInt64( Guid.NewGuid().ToByteArray(), 0 );

			mBands = new List<ParametricBand>();
		}
			
		public ParametricEqualizer( long equalizerId, bool isPreset ) :
			this() {
			EqualizerId = equalizerId;
			IsPreset = isPreset;
		}

		public IEnumerable<ParametricBand> Bands {
			get{ return( from ParametricBand band in mBands orderby band.CenterFrequency ascending select band ); }
		}

		public void AddBand( float centerFrequency, float gain ) {
			mBands.Add( new ParametricBand( centerFrequency, gain ));
		}

		public void AddBands( ParametricBand[] bands ) {
			foreach( var band in bands ) {
				AddBand( band );
			}
		}

		public void AddBand( ParametricBand band ) {
			var current = ( from ParametricBand item in mBands
							where ( item.BandId == band.BandId ) || ( item.CenterFrequency == band.CenterFrequency )
							select item ).FirstOrDefault();

			if( current == null ) {
				mBands.Add( band );
			}
		}

		public void AdjustEq( long bandId, float gain ) {
			var band = ( from ParametricBand item in mBands where item.BandId == bandId select item ).FirstOrDefault();

			if( band != null ) {
				band.Gain = gain;
			}
		}
	}
}
