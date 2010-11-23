using System;
using System.Collections.Generic;
using System.Linq;
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

	public class EqViewModel : DialogModelBase {
		private IPlayController								mPlayController;
		private List<ParametricEqualizer>					mEqs;
		private	readonly ObservableCollectionEx<UiEqBand>	mBands;

		public EqViewModel() {
			mBands = new ObservableCollectionEx<UiEqBand>();
		}

		public bool Initialize( IPlayController playController ) {
			mPlayController = playController;
			mEqs = new List<ParametricEqualizer>( from ParametricEqualizer eq in mPlayController.EqManager.EqPresets orderby eq.Name ascending select eq );

			LoadBands();

			return( true );
		}

		private void LoadBands() {
			mBands.Clear();

			if( mPlayController.CurrentEq != null ) {
				foreach( var band in mPlayController.EqManager.CurrentEq.Bands ) {
					mBands.Add( new UiEqBand( band, AdjustEq, mPlayController.EqManager.CurrentEq.IsPreset ));
				}
			}
		}

		private void AdjustEq( UiEqBand band ) {
			mPlayController.SetEqValue( band.BandId, band.Gain );
		}

		public ParametricEqualizer CurrentEq {
			get{ return( mPlayController.CurrentEq ); }
			set {
				mPlayController.CurrentEq = value;

				LoadBands();
			}
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

		public List<ParametricEqualizer> EqList {
			get{ return( mEqs ); }
		}

		[DependsUpon( "CurrentEq" )]
		public ObservableCollectionEx<UiEqBand> Bands {
			get{ return( mBands ); }
		}

		[DependsUpon( "CurrentEq" )]
		public bool IsEditable {
			get{ return(!mPlayController.EqManager.CurrentEq.IsPreset ); }
		}

		public void Execute_ResetBands() {
			foreach( var band in mBands ) {
				band.Gain = 0.0f;

				AdjustEq( band );
			}

			mPlayController.PreampVolume = 1.0f;

			RaisePropertyChanged( () => PreampVolume );
		}

		[DependsUpon( "CurrentEq" )]
		public bool CanExecute_ResetBands() {
			return( IsEditable );
		}
	}
}
