using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using LightPipe.Dto;
using LightPipe.Views;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace LightPipe.ViewModels {
    class ZoneSelectionViewModel : PropertyChangeBase {
        private readonly IZoneManager                   mZoneManager;
        private readonly IDialogService                 mDialogService;
        private readonly List<Color>                    mLegendColors;
        private ZoneGroup                               mCurrentZone;

        public  ObservableCollection<ZoneGroup>         Zones { get; }
        public  ObservableCollection<UiZoneDefinition>  ZoneList {get; }
        public  ZoneDisplayViewModel                    ZoneDisplay { get; }

        public  DelegateCommand                         NewZone { get; }
        public  DelegateCommand                         EditZone { get; }
        public  DelegateCommand                         DeleteZone { get; }

        public ZoneSelectionViewModel( IZoneManager zoneManager, ZoneDisplayViewModel zoneDisplayViewModel, IDialogService dialogService ) {
            mZoneManager = zoneManager;
            mDialogService = dialogService;
            ZoneDisplay = zoneDisplayViewModel;

            Zones = new ObservableCollection<ZoneGroup>();
            ZoneList = new ObservableCollection<UiZoneDefinition>();

            NewZone = new DelegateCommand( OnNewZone );
            EditZone = new DelegateCommand( OnEditZone, CanEditZone );
            DeleteZone = new DelegateCommand( OnDeleteZone, CanDeleteZone );

            mLegendColors = new List<Color> { Colors.OrangeRed, Colors.LimeGreen, Colors.CornflowerBlue, Colors.Goldenrod,   Colors.BlueViolet };
            LoadZones();
        }

        public ZoneGroup CurrentZone {
            get => mCurrentZone;
            set {
                mCurrentZone = value;

                ZoneList.Clear();

                if( mCurrentZone != null ) {
                    mZoneManager.SetCurrentGroup( CurrentZone.GroupId );

                    mCurrentZone.Zones.ForEach( z => {
                        ZoneList.Add( new UiZoneDefinition( z, mLegendColors[ZoneList.Count % mLegendColors.Count]));
                    });
                }

                ZoneDisplay.SetZones( ZoneList );

                RaisePropertyChanged( () => CurrentZone );
                RaisePropertyChanged( () => ZoneList );

                EditZone.RaiseCanExecuteChanged();
                DeleteZone.RaiseCanExecuteChanged();
            }
        }

        private void OnNewZone() {
            var newZone = mZoneManager.CreateZone( "Unnamed Zone" );
            var parameters = new DialogParameters{{ ZoneEditViewModel.cZoneParameter, newZone }};

            mDialogService.ShowDialog( nameof( ZoneEditView ), parameters, result => {
                if( result.Result == ButtonResult.OK ) {
                    mZoneManager.UpdateZone( newZone );
                }
            });

            LoadZones();
        }

        private void OnEditZone() {
            if( CurrentZone != null ) {
                var parameters = new DialogParameters{{ ZoneEditViewModel.cZoneParameter, CurrentZone }};

                mDialogService.ShowDialog( nameof( ZoneEditView ), parameters, result => {
                    if( result.Result == ButtonResult.OK ) {
                        mZoneManager.UpdateZone( CurrentZone );
                    }
                });

                LoadZones();
            }
        }

        private bool CanEditZone() {
            return CurrentZone != null;
        }

        private void OnDeleteZone() { }
        private bool CanDeleteZone() {
            return CurrentZone != null;
        }

        private void LoadZones() {
            Zones.Clear();
            Zones.AddRange( mZoneManager.GetZones());

            var currentGroup = mZoneManager.GetCurrentGroup();

            CurrentZone = Zones.FirstOrDefault( z => z.GroupId.Equals( currentGroup?.GroupId )) ??
                          Zones.FirstOrDefault();
        }
    }
}
