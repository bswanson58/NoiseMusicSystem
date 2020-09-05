using System;
using System.Collections.Generic;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    internal class PairingSet {
        public  List<LightPipePairing>  Pairings { get; }

        public PairingSet() {
            Pairings = new List<LightPipePairing>();
        }

        public PairingSet( IEnumerable<LightPipePairing> pairings ) {
            Pairings = new List<LightPipePairing>( pairings );
        }
    }

    class PairingManager : IPairingManager {
        private readonly IPreferences               mPreferences;
        private readonly List<LightPipePairing>     mPairings;

        public PairingManager( IPreferences preferences ) {
            mPreferences = preferences;

            mPairings = new List<LightPipePairing>();
        }

        public IEnumerable<LightPipePairing> GetPairings() {
            if(!mPairings.Any()) {
                LoadPairings();
            }

            return mPairings;
        }

        public void DeletePairing( LightPipePairing pairing ) {
            if(!mPairings.Any()) {
                LoadPairings();
            }

            var existing = mPairings.FirstOrDefault( p => p.PairingId.Equals( pairing.PairingId ));
            if( existing != null ) {
                mPairings.Remove( existing );
            }

            SavePairings();
        }

        public LightPipePairing AddPairing( string pairName, string entertainmentGroupId, string zoneGroupId ) {
            var retValue = new LightPipePairing( pairName, entertainmentGroupId, zoneGroupId );

            if(!mPairings.Any()) {
                LoadPairings();
            }

            mPairings.Add( retValue );
            SavePairings();

            return retValue;
        }

        public void SetCurrentPairing( LightPipePairing pairing ) {
            var preferences = mPreferences.Load<MilkPreferences>();

            preferences.LightPipePairing = pairing != null ? pairing.PairingId : String.Empty;

            mPreferences.Save( preferences );
        }

        public LightPipePairing GetCurrentPairing() {
            var retValue = default( LightPipePairing );
            var preferences = mPreferences.Load<MilkPreferences>();

            if(!mPairings.Any()) {
                LoadPairings();
            }

            if(!string.IsNullOrEmpty( preferences.LightPipePairing )) {
                retValue = mPairings.FirstOrDefault( p => p.PairingId.Equals( preferences.LightPipePairing ));
            }

            return retValue;
        }

        private void LoadPairings() {
            var pairings = mPreferences.Load<PairingSet>();

            mPairings.Clear();

            if( pairings?.Pairings != null ) {
                mPairings.AddRange( pairings.Pairings );
            }
        }

        private void SavePairings() {
            var pairingSet = new PairingSet( mPairings );

            mPreferences.Save( pairingSet );
        }
    }
}
