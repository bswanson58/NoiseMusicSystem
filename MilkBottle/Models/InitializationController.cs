using System.Threading.Tasks;
using Caliburn.Micro;
using MilkBottle.Interfaces;
using OpenTK;

namespace MilkBottle.Models {
    class InitializationController : IInitializationController, IHandle<Events.MilkConfigurationUpdated>, IHandle<Events.ApplicationClosing> {
        private readonly IEventAggregator   mEventAggregator;
        private readonly IMilkController    mMilkController;
        private readonly IPresetController  mPresetController;
        private readonly Task<bool>         mLibraryInitializationTask;
        private GLControl                   mGlControl;

        public InitializationController( IMilkController milkController, IPresetController presetController, IPresetLibrarian librarian, IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
            mMilkController = milkController;
            mPresetController = presetController;

            mLibraryInitializationTask = librarian.Initialize();

            mEventAggregator.Subscribe( this );
        }

        public async void ContextReady( GLControl glControl ) {
            mGlControl = glControl;
            mGlControl.MakeCurrent();

            mMilkController.Initialize( mGlControl );

            if( await mLibraryInitializationTask ) {
                mPresetController.Initialize();

                mEventAggregator.PublishOnUIThread( new Events.InitializationComplete());

                mMilkController.StartVisualization();
                mPresetController.StartPresetCycling();
            }
        }

        public void Handle( Events.MilkConfigurationUpdated args ) {
            mMilkController.MilkConfigurationUpdated();
            mPresetController.MilkConfigurationUpdated();
        }

        public void Handle( Events.ApplicationClosing args ) {
            mMilkController.StopVisualization();
            mPresetController.StopPresetCycling();

            mEventAggregator.Unsubscribe( this );
        }
    }
}
