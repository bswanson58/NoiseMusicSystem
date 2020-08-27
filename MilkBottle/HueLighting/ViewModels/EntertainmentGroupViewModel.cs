using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HueLighting.Dto;
using HueLighting.Interfaces;
using MilkBottle.Infrastructure.Interfaces;
using Q42.HueApi.Models.Groups;
using ReusableBits.Mvvm.ViewModelSupport;

namespace HueLighting.ViewModels {
    class EntertainmentGroupViewModel : PropertyChangeBase {
        private readonly IHubManager    mHubManager;
        private readonly IPreferences   mPreferences;
        private Group                   mSelectedGroup;
        private EntertainmentGroup      mEntertainmentGroup;

        public  ObservableCollection<Group> Groups { get; }
        public  IEnumerable<GroupLights>    GroupLights => mEntertainmentGroup?.Lights;

        public EntertainmentGroupViewModel( IHubManager hubManager, IPreferences preferences ) {
            mHubManager = hubManager;
            mPreferences = preferences;

            Groups = new ObservableCollection<Group>();

            LoadGroups();
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
                RaisePropertyChanged( () => GroupLights );
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
    }
}
