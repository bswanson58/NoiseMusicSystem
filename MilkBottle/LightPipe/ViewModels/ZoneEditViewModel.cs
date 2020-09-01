﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using LightPipe.Dto;
using MilkBottle.Infrastructure.Dto;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace LightPipe.ViewModels {
    class ZoneEditViewModel : PropertyChangeBase, IDialogAware {
        public  const string                            cZoneParameter = "zone";

        private ZoneGroup                               mZoneGroup;
        private ZoneDefinition                          mCurrentZone;

        public  ObservableCollection<UiZoneDefinition>  Zones { get; }

        public  DelegateCommand                         Ok { get; }
        public  DelegateCommand                         Cancel { get; }

        public  string                                  Title { get; }

        public  event Action<IDialogResult>             RequestClose;

        public ZoneEditViewModel() {
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            Zones = new ObservableCollection<UiZoneDefinition>();

            Title = "Edit Zone Group";
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            mZoneGroup = parameters.GetValue<ZoneGroup>( cZoneParameter );

            Zones.Clear();
            if( mZoneGroup != null ) {
                Zones.AddRange( from z in mZoneGroup.Zones select new UiZoneDefinition( z, Colors.White ));
            }

            RaisePropertyChanged( () => ZoneName );
        }

        public string ZoneName {
            get => mZoneGroup?.GroupName;
            set {
                if( mZoneGroup != null ) {
                    mZoneGroup.GroupName = value;
                }
            }
        }

        public ZoneDefinition CurrentZone {
            get => mCurrentZone;
            set {
                mCurrentZone = value;

                OnZoneChanged();
                RaisePropertyChanged( () => CurrentZone );
            }
        }

        private void OnZoneChanged() { }

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
