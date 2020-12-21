using System;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.Dialogs {
    class EditTrackTagsViewModel : BindableBase, IDialogAware {
        public  const string        cTrackParameter = "trackParameter";
        public  const string        cDialogAccepted = "dialogAccepted";

        private readonly ITagInformationProvider    mTagProvider;
        private TrackInfo                           mTrack;

        public  string                              TrackName => mTrack?.TrackName;
        public  ObservableCollection<UiTag>         TagList { get; }

        public  DelegateCommand                     Ok { get; }
        public  DelegateCommand                     Cancel { get; }

        public  event Action<IDialogParameters>     RequestClose;

        public EditTrackTagsViewModel( ITagInformationProvider tagProvider ) {
            mTagProvider = tagProvider;

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            TagList = new ObservableCollection<UiTag>();
        }

        public async void OnDialogOpened( IDialogParameters parameters ) {
            var tagList = await mTagProvider.GetUserTags();

            if( tagList.Success ) {
                TagList.Clear();
                tagList.TagList.OrderBy( t => t.TagName ).ForEach( t => TagList.Add( new UiTag( t )));

                mTrack = parameters.GetValue<TrackInfo>( cTrackParameter );

                mTrack?.Tags.ForEach( t => {
                    var uiTag = TagList.FirstOrDefault( tag => tag.TagId.Equals( t.TagId ));

                    if( uiTag != null ) {
                        uiTag.IsTagged = true;
                    }
                });
            }

            RaisePropertyChanged( TrackName );
        }

        private void OnOk() {
            mTrack.Tags.Clear();

            TagList.Where( t => t.IsTagged ).ForEach( t => {
                mTrack.Tags.Add( new TrackTagInfo{ TagId = t.TagId, TagName = t.TagName });
            });

            RaiseRequestClose( new DialogParameters{{ cTrackParameter, mTrack }, { cDialogAccepted, true }});
        }

        private void OnCancel() {
            RaiseRequestClose( new DialogParameters{{ cDialogAccepted, false }});
        }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        private void RaiseRequestClose( IDialogParameters parameters ) {
            RequestClose?.Invoke( parameters );
        }
    }
}
