using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Caliburn.Micro;
using LightPipe.Dto;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Interfaces;

namespace LightPipe.Models {
    internal class ZoneDefinitions {
        public  List<ZoneGroup> ZoneGroups { get; set; }

        public ZoneDefinitions() {
            ZoneGroups = new List<ZoneGroup>();
        }
    }

    class ZoneManager : IZoneManager {
        private readonly IEventAggregator   mEventAggregator;
        private readonly IPreferences       mPreferences;
        private readonly List<ZoneGroup>    mZones;

        public ZoneManager( IPreferences preferences, IEventAggregator  eventAggregator ) {
            mPreferences = preferences;
            mEventAggregator = eventAggregator;

            mZones = new List<ZoneGroup>();

            AddDebugZones();
        }

        private void AddDebugZones() {
            UpdateZones();

            if(!mZones.Any()) {
                var zoneGroup = new ZoneGroup( "Bar Room L/R/Front" );

                zoneGroup.Zones.Add( new ZoneDefinition( "Left", new RectangleF( 5, 20, 20, 50 ), GroupLightLocation.Left ));
                zoneGroup.Zones.Add( new ZoneDefinition( "Center", new RectangleF( 35, 35, 30, 30 ), GroupLightLocation.Front ));
                zoneGroup.Zones.Add( new ZoneDefinition( "Right", new RectangleF( 75, 5, 20, 50 ), GroupLightLocation.Right ));
                mZones.Add( zoneGroup );

                zoneGroup = new ZoneGroup( "Bar Front/Ground" );
                zoneGroup.Zones.Add( new ZoneDefinition( "Center", new RectangleF( 35, 35, 30, 30 ), GroupLightLocation.Front ));
                zoneGroup.Zones.Add( new ZoneDefinition( "Bottom", new RectangleF( 30, 85, 40, 15 ), GroupLightLocation.Ground ));
                mZones.Add( zoneGroup );

                zoneGroup = new ZoneGroup( "Bar L/TV/R/G" );
                zoneGroup.Zones.Add( new ZoneDefinition( "Left", new RectangleF( 1, 1, 30, 30 ), GroupLightLocation.Left ));
                zoneGroup.Zones.Add( new ZoneDefinition( "Center", new RectangleF( 35, 35, 30, 30 ), GroupLightLocation.Television ));
                zoneGroup.Zones.Add( new ZoneDefinition( "Right", new RectangleF( 75, 5, 20, 50 ), GroupLightLocation.Right ));
                zoneGroup.Zones.Add( new ZoneDefinition( "Bottom", new RectangleF( 30, 85, 40, 15 ), GroupLightLocation.Ground ));
                mZones.Add( zoneGroup );

                SaveZoneDefinitions();
                SetCurrentGroup( zoneGroup.GroupId );
            }
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

        public void SetCurrentGroup( string groupId ) {
            var preferences = mPreferences.Load<LightPipeConfiguration>();

            preferences.ZoneGroupId = groupId;

            mPreferences.Save( preferences );

            mEventAggregator.PublishOnUIThread( new MilkBottle.Infrastructure.Events.CurrentZoneChanged());
        }

        public ZoneGroup GetCurrentGroup() {
            var preferences = mPreferences.Load<LightPipeConfiguration>();

            UpdateZones();

            return mZones.FirstOrDefault( z => z.GroupId.Equals( preferences.ZoneGroupId ));
        }

        private void UpdateZones() {
            if(!mZones.Any()) {
                LoadZoneDefinitions();
            }
        }

        private void LoadZoneDefinitions() {
            var definitions = mPreferences.Load<ZoneDefinitions>();

            mZones.Clear();

            if( definitions?.ZoneGroups != null ) {
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
