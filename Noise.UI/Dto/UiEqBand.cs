using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Dto {
	public class UiEqBand : ViewModelBase {
		public	long						BandId { get; private set; }
		public	float						CenterFrequency { get; private set; }
		private float						mGain;
		private readonly Action<UiEqBand>	mOnChange;

		public	bool						IsEditable { get; private set; }

		public UiEqBand( ParametricBand band, Action<UiEqBand> onChange, bool isPreset ) {
			BandId = band.BandId;
			CenterFrequency = band.CenterFrequency;
			IsEditable = !isPreset;
			mGain = band.Gain;
			mOnChange = onChange;
		}

		public float Gain {
			get{ return( mGain ); }
			set {
				mGain = value;

				RaisePropertyChanged( () => Gain );

				if( mOnChange != null ) {
					mOnChange( this );
				}
			}
		}
	}
}
