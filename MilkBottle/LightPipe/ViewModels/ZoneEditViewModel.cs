using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using LightPipe.Dto;
using MilkBottle.Infrastructure.Dto;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using Color = System.Windows.Media.Color;

namespace LightPipe.ViewModels {
    class ZoneEditViewModel : PropertyChangeBase, IDialogAware {
        public  const string                        cZoneParameter = "zone";

        private readonly List<Color>                mLegendColors;
        private ZoneGroup                           mZoneGroup;
        private UiZoneEdit                          mCurrentZone;
        private int                                 mNextZoneColor;

        public  ObservableCollection<UiZoneEdit>    Zones { get; }
        public  ObservableCollection<GroupLightLocation>    Locations { get; }

        public  DelegateCommand                     AddZone { get; }
        public  DelegateCommand                     DeleteZone { get; }
        public  DelegateCommand                     Ok { get; }
        public  DelegateCommand                     Cancel { get; }

        public  string                              Title { get; }

        public  event Action<IDialogResult>         RequestClose;

        public ZoneEditViewModel() {
            AddZone = new DelegateCommand( OnAddZone );
            DeleteZone = new DelegateCommand( OnDeleteZone, CanDeleteZone );
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            Zones = new ObservableCollection<UiZoneEdit>();
            Locations = new ObservableCollection<GroupLightLocation>();
            Locations.AddRange( Enum.GetValues( typeof( GroupLightLocation )).Cast<GroupLightLocation>());

            mLegendColors = new List<Color> { Colors.OrangeRed, Colors.LimeGreen, Colors.CornflowerBlue, Colors.Goldenrod,   Colors.BlueViolet };

            Title = "Edit Zone Group";
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            mZoneGroup = parameters.GetValue<ZoneGroup>( cZoneParameter );

            Zones.Clear();
            if( mZoneGroup != null ) {
                Zones.AddRange( from z in mZoneGroup.Zones select new UiZoneEdit( z, mLegendColors[Zones.Count % mLegendColors.Count]));
                mNextZoneColor = Zones.Count % mLegendColors.Count;
            }
            CurrentZone = Zones.FirstOrDefault();

            RaisePropertyChanged( () => GroupName );
        }

        public string GroupName {
            get => mZoneGroup?.GroupName;
            set {
                if( mZoneGroup != null ) {
                    mZoneGroup.GroupName = value;
                }
            }
        }

        public UiZoneEdit CurrentZone {
            get => mCurrentZone;
            set {
                mCurrentZone = value;

                RaisePropertyChanged( () => CurrentZone );
                RaisePropertyChanged( () => ZoneName );
                RaisePropertyChanged( () => CurrentLocation );
                DeleteZone.RaiseCanExecuteChanged();
            }
        }

        public string ZoneName {
            get => CurrentZone?.ZoneName;
            set {
                if( CurrentZone != null ) {
                    CurrentZone.ZoneName = value;
                }
            }
        }

        public GroupLightLocation CurrentLocation {
            get {
                GroupLightLocation retValue = default;

                if( CurrentZone != null ) {
                    retValue = CurrentZone.LightLocation;
                }

                return retValue;
            }
            set {
                if( CurrentZone != null ) {
                    CurrentZone.LightLocation = value;
                }
            }
        }

        private void OnAddZone() {
            var zoneDefinition = new ZoneDefinition( "unnamed", new RectangleF( 40, 40, 20, 20 ), GroupLightLocation.Center );
            var zone = new UiZoneEdit( zoneDefinition, mLegendColors[mNextZoneColor]);

            mNextZoneColor = ( mNextZoneColor + 1 ) % mLegendColors.Count;

            Zones.Add( zone );
            CurrentZone = zone;
        }
        private void OnDeleteZone() {
            if( CurrentZone != null ) {
                Zones.Remove( CurrentZone );
            }

            CurrentZone = Zones.FirstOrDefault();
        }

        private bool CanDeleteZone() {
            return CurrentZone != null;
        }

        public void OnOk() {
            RaiseRequestClose(new DialogResult( ButtonResult.OK, new DialogParameters()));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
