using System.Collections.Generic;
using System.Linq;
using LightPipe.Dto;
using LightPipe.Interfaces;
using MilkBottle.Infrastructure.Interfaces;

namespace LightPipe.Models {
    internal class ZoneDefinitions {
        public  List<ZoneGroup> ZoneGroups { get; set; }
    }

    class ZoneManager : IZoneManager {
        private readonly IPreferences       mPreferences;
        private readonly List<ZoneGroup>    mZones;

        public ZoneManager( IPreferences preferences ) {
            mPreferences = preferences;

            mZones = new List<ZoneGroup>();
        }

        public IEnumerable<ZoneGroup> GetZones() {
            var retValue = new List<ZoneGroup>();

            UpdateZones();
            retValue.AddRange( mZones );

            return retValue;
        }

        public ZoneGroup GetZone( string groupId ) {
            UpdateZones();

            return mZones.FirstOrDefault( z => z.GroupId.Equals( groupId ));
        }

        public ZoneGroup CreateZone( string zoneName ) {
            var retValue = new ZoneGroup( zoneName );

            UpdateZones();

            mZones.Add( retValue );
            SaveZoneDefinitions();

            return retValue;
        }

        public void UpdateZone( ZoneGroup zone ) {
            UpdateZones();

            var existing = mZones.FirstOrDefault( z => z.GroupId.Equals( zone.GroupId ));
            if( existing != null ) {
                mZones.Remove( existing );
            }

            mZones.Add( zone );

            SaveZoneDefinitions();
        }

        public void DeleteZone( string groupId ) {
            UpdateZones();

            var existing = mZones.FirstOrDefault( z => z.GroupId.Equals( groupId ));
            if( existing != null ) {
                mZones.Remove( existing );

                UpdateZones();
            }
        }

        private void UpdateZones() {
            if(!mZones.Any()) {
                LoadZoneDefinitions();
            }
        }

        private void LoadZoneDefinitions() {
            var definitions = mPreferences.Load<ZoneDefinitions>();

            mZones.Clear();

            if( definitions != null ) {
                mZones.AddRange( definitions.ZoneGroups );
            }
        }

        private void SaveZoneDefinitions() {
            var definitions = new ZoneDefinitions();

            definitions.ZoneGroups.AddRange( mZones );

            mPreferences.Save( definitions );
        }
    }
}
