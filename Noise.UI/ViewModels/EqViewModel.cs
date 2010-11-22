using System;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class UiEqBand : ViewModelBase {
		public	long						BandId { get; private set; }
		public	float						CenterFrequency { get; private set; }
		private float						mGain;
		private readonly Action<UiEqBand>	mOnChange;

		public UiEqBand( ParametricBand band, Action<UiEqBand> onChange ) {
			BandId = band.BandId;
			CenterFrequency = band.CenterFrequency;
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

	public class EqViewModel : DialogModelBase {
		private IPlayController				mPlayController;
		private	readonly List<UiEqBand>		mBands;

		public EqViewModel() {
			mBands = new List<UiEqBand>();
		}

		public bool Initialize( IPlayController playController ) {
			mPlayController = playController;

			LoadBands();

			return( true );
		}

		private void LoadBands() {
			mBands.Clear();

			if( mPlayController.ParametricEq != null ) {
				foreach( var band in mPlayController.ParametricEq.Bands ) {
					mBands.Add( new UiEqBand( band, AdjustEq ));
				}
			}
		}

		private void AdjustEq( UiEqBand band ) {
			mPlayController.SetEqValue( band.BandId, band.Gain );
		}

		public ParametricEqualizer Eq {
			get{ return( mPlayController.ParametricEq ); }
		}

		public double PreampVolume {
			get{ return( mPlayController.PreampVolume ); }
			set{ mPlayController.PreampVolume = value; }
		}

		public bool ReplayGainEnabled {
			get{ return( mPlayController.ReplayGainEnable ); }
			set{ mPlayController.ReplayGainEnable = value; }
		}

		public bool EqEnabled {
			get{ return( mPlayController.EqEnabled ); }
			set{ mPlayController.EqEnabled = value; }
		}

		public List<UiEqBand> Bands {
			get{ return( mBands ); }
		}

		public void Execute_ResetBands() {
			foreach( var band in mBands ) {
				band.Gain = 0.0f;

				AdjustEq( band );
			}

			mPlayController.PreampVolume = 1.0f;

			RaisePropertyChanged( () => PreampVolume );
		}
	}
}
