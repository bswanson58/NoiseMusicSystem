using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using HueLighting.Dto;
using HueLighting.Interfaces;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Interfaces;
using Q42.HueApi.Models.Groups;
using ReusableBits.Mvvm.ViewModelSupport;

namespace HueLighting.ViewModels {
    internal class UiGroupLights : PropertyChangeBase {
        private readonly GroupLights        mLights;

        public  GroupLightLocation      Location => mLights.Location;
        public  List<Bulb>              Lights => mLights.Lights;
        public  bool                    IsUtilized { get; private set; }

        public UiGroupLights( GroupLights lights ) {
            mLights = lights;
        }

        public void SetUtilization( bool state ) {
            IsUtilized = state;

            RaisePropertyChanged( () => IsUtilized );
        }
    }

    class EntertainmentGroupViewModel : PropertyChangeBase, IDisposable, IHandle<MilkBottle.Infrastructure.Events.CurrentZoneChanged> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly IHubManager                mHubManager;
        private readonly IPreferences               mPreferences;
        private readonly IZoneManager               mZoneManager;
        private Group                               mSelectedGroup;
        private EntertainmentGroup                  mEntertainmentGroup;

        public  EntertainmentGroupDisplayViewModel  GroupDisplay { get; }
        public  ObservableCollection<Group>         Groups { get; }
        public  ObservableCollection<UiGroupLights> GroupLights { get; }

        public EntertainmentGroupViewModel( IHubManager hubManager, IZoneManager zoneManager, EntertainmentGroupDisplayViewModel groupDisplay,
                                            IPreferences preferences, IEventAggregator eventAggregator ) {
            mHubManager = hubManager;
            mZoneManager = zoneManager;
            GroupDisplay = groupDisplay;
            mPreferences = preferences;
            mEventAggregator = eventAggregator;

            Groups = new ObservableCollection<Group>();
            GroupLights = new ObservableCollection<UiGroupLights>();

            LoadGroups();

            mEventAggregator.Subscribe( this );
        }

        public void Handle( MilkBottle.Infrastructure.Events.CurrentZoneChanged args ) {
            IndicateZones();
        }

        public Group SelectedGroup {
            get => mSelectedGroup;
            set {
                mSelectedGroup = value;

                RaisePropertyChanged( () => SelectedGroup );

                OnGroupSelected();
            }
        }

        private async void OnGroupSelected() {
            if( mSelectedGroup != null ) {
                var preferences = mPreferences.Load<HueConfiguration>();

                preferences.EntertainmentGroupId = mSelectedGroup.Id;

                mPreferences.Save( preferences );

                mEntertainmentGroup = await mHubManager.GetEntertainmentGroupLayout( mSelectedGroup );

                GroupLights.Clear();

                if( mEntertainmentGroup != null ) {
                    GroupLights.AddRange( from light in mEntertainmentGroup.LightGroups select new UiGroupLights( light ));

                    GroupDisplay.SetEntertainmentGroup( mEntertainmentGroup );
                }

                IndicateZones();
            }
        }

        private void IndicateZones() {
            var group = mZoneManager.GetCurrentGroup();

            if( group != null ) {
                foreach( var zone in GroupLights ) {
                    zone.SetUtilization( group.Zones.FirstOrDefault( z => z.LightLocation.Equals( zone.Location )) != null );
                }
            }
        }

        private async void LoadGroups() {
            Groups.Clear();

            if(!mHubManager.IsInitialized ) { 
                await mHubManager.InitializeConfiguredHub();
            }

            if( mHubManager.IsInitialized ) {
                Groups.AddRange( await mHubManager.GetEntertainmentGroups());
            }

            if( Groups.Any()) {
                var preferences = mPreferences.Load<HueConfiguration>();

                SelectedGroup = Groups.FirstOrDefault( g => g.Id.Equals( preferences.EntertainmentGroupId )) ?? 
                                Groups.FirstOrDefault();
            }
        }

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );
        }
    }
}
