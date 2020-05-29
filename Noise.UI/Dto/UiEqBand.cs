using System;
using Noise.Infrastructure.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
	public class UiEqBand : PropertyChangeBase {
		public	long						BandId { get; }
		public	float						CenterFrequency { get; }
		private float						mGain;
		private readonly Action<UiEqBand>	mOnChange;

		public	bool						IsEditable { get; }

		public UiEqBand( ParametricBand band, Action<UiEqBand> onChange, bool isPreset ) {
			BandId = band.BandId;
			CenterFrequency = band.CenterFrequency;
			IsEditable = !isPreset;
			mGain = band.Gain;
			mOnChange = onChange;
		}

		public float Gain {
			get => ( mGain );
            set {
				mGain = value;

				RaisePropertyChanged( () => Gain );

                mOnChange?.Invoke( this );
            }
		}
	}
}
