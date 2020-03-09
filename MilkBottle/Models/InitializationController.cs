using System.Threading.Tasks;
using Caliburn.Micro;
using MilkBottle.Interfaces;
using OpenTK;

namespace MilkBottle.Models {
    class InitializationController : IInitializationController, IHandle<Events.MilkConfigurationUpdated>, IHandle<Events.ApplicationClosing> {
        private readonly IEventAggregator   mEventAggregator;
        private readonly IStateManager      mStateManager;
        private readonly IMilkController    mMilkController;
        private readonly IPresetController  mPresetController;
        private Task<bool>                  mDatabaseBuildTask;
        private GLControl                   mGlControl;

        public InitializationController( IStateManager stateManager, IMilkController milkController, IPresetController presetController, IDatabaseBuilder databaseBuilder,
                                         IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
            mStateManager = stateManager;
            mMilkController = milkController;
            mPresetController = presetController;

            mEventAggregator.Subscribe( this );

            mDatabaseBuildTask = databaseBuilder.ReconcileDatabase();
        }

        public async void ContextReady( GLControl glControl ) {
            mGlControl = glControl;
            mGlControl.MakeCurrent();

            mMilkController.Initialize( mGlControl );

            if( await mDatabaseBuildTask ) {
                mPresetController.Initialize();

                mEventAggregator.PublishOnUIThread( new Events.InitializationComplete());
                mDatabaseBuildTask = null;
            }
        }

        public void Handle( Events.MilkConfigurationUpdated args ) {
            mMilkController.MilkConfigurationUpdated();
            mPresetController.MilkConfigurationUpdated();
        }

        public void Handle( Events.ApplicationClosing args ) {
            mStateManager.EnterState( eStateTriggers.Stop );

            mEventAggregator.Unsubscribe( this );
        }
    }
}
