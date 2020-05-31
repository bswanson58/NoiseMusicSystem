using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.ExtensionClasses.MoreLinq;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    class TagAssociationDialogModel : PropertyChangeBase, IDialogAware {
        public const string                 cTrackParameter = "track";

        private readonly IUserTagManager    mTagManager;

        public BindableCollection<UiTag>    TagList { get; }
        public DbTrack                      Track { get; private set; }
        public string                       TrackName => Track != null ? Track.Name : String.Empty;

        public  string                      Title { get; }
        public  DelegateCommand             Ok { get; }
        public  DelegateCommand             Cancel { get; }
        public  event Action<IDialogResult> RequestClose;

        public TagAssociationDialogModel( IUserTagManager tagManager ) {
            mTagManager = tagManager;

            TagList = new BindableCollection<UiTag>();
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            Title = "Tag Associations";
        }

        public IEnumerable<DbTag> GetSelectedTags() {
            return from tag in TagList where tag.IsChecked select tag.Tag;
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            Track = parameters.GetValue<DbTrack>( cTrackParameter );

            if( Track != null ) {
                TagList.AddRange( from tag in mTagManager.GetUserTagList() orderby tag.Name select new UiTag( tag ));

                mTagManager.GetAssociatedTags( Track.DbId ).ForEach( tag => {
                    var uiTag = TagList.FirstOrDefault( t => t.Tag.DbId.Equals( tag.DbId ));

                    if( uiTag!= null ) {
                        uiTag.IsChecked = true;
                    }
                });

                RaisePropertyChanged( () => TrackName );
            }
        }

        public void OnOk() {
            mTagManager.UpdateAssociations( Track, GetSelectedTags());
            
            RaiseRequestClose( new DialogResult( ButtonResult.OK ));
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
