using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Subjects;
using HueLighting.Dto;
using HueLighting.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace HueLighting.ViewModels {
    public abstract class UiBulbGroupBase {
        public  string  Name { get; protected set; }
        public  bool    IsEnabled { get; protected set; }

        public abstract IEnumerable<Bulb> AsBulbList();
    }

    public class UiBulbGroupHeader : UiBulbGroupBase {
        public UiBulbGroupHeader( string headerName ) {
            Name = headerName;
            IsEnabled = false;
        }

        public override IEnumerable<Bulb> AsBulbList() {
            return new List<Bulb>();
        }
    }

    public class UiSingleBulb : UiBulbGroupBase {
        private readonly Bulb   mBulb;

        public UiSingleBulb( Bulb bulb ) {
            mBulb = bulb;

            Name = mBulb.Name;
            IsEnabled = true;
        }

        public override IEnumerable<Bulb> AsBulbList() {
            return new []{ mBulb };
        }
    }

    public class UiBulbGroup : UiBulbGroupBase {
        private readonly BulbGroup  mGroup;

        public UiBulbGroup( BulbGroup group ) {
            mGroup = group;

            Name = mGroup.Name;
            IsEnabled = true;
        }

        public override IEnumerable<Bulb> AsBulbList() {
            return mGroup.Bulbs;
        }
    }

    public class BulbSelectorViewModel : PropertyChangeBase {
        private readonly IHubManager    mHub;
        private UiBulbGroupBase         mSelectedLight;

        public  ObservableCollection<UiBulbGroupBase>   LightList { get; }
        public  Subject<IEnumerable<Bulb>>              TargetLightsChanged { get; }

        public BulbSelectorViewModel( IHubManager hub ) {
            mHub = hub;

            TargetLightsChanged = new Subject<IEnumerable<Bulb>>();
            LightList = new ObservableCollection<UiBulbGroupBase>();

            CollectBulbs();
        }

        public UiBulbGroupBase SelectedLight {
            get => mSelectedLight;
            set {
                mSelectedLight = value;

                RaisePropertyChanged( () => SelectedLight );

                TargetLightsChanged.OnNext( mSelectedLight?.AsBulbList());
            }
        }

        private async void CollectBulbs() {
            LightList.Clear();

            LightList.Add( new UiBulbGroupHeader( "Bulbs:" ));
            LightList.AddRange( from b in await mHub.GetBulbs() orderby b.Name select new UiSingleBulb( b ));
            LightList.Add( new UiBulbGroupHeader( "Groups:" ));
            LightList.AddRange( from g in await mHub.GetBulbGroups() orderby g.Name select new UiBulbGroup( g ));
        }
    }
}
