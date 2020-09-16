using System.Threading.Tasks;
using Caliburn.Micro;
using MilkBottle.Interfaces;
using OpenTK;

namespace MilkBottle.Models {
    class InitializationController : IInitializationController, IHandle<Events.MilkConfigurationUpdated>, IHandle<Events.ApplicationClosing> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IStateManager          mStateManager;
        private readonly IMilkController        mMilkController;
        private readonly IPresetController      mPresetController;
        private readonly ILightPipePump         mLightPipePump;
        private readonly Task<bool>             mDatabaseBuildTask;
        private GLControl                       mGlControl;
        private bool                            mWaitForDatabaseReconcile;

        public InitializationController( IStateManager stateManager, IMilkController milkController, IPresetController presetController, ILightPipePump lightPipePump,
                                         IDatabaseBuilder databaseBuilder, IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
            mStateManager = stateManager;
            mMilkController = milkController;
            mPresetController = presetController;
            mLightPipePump = lightPipePump;

            mEventAggregator.Subscribe( this );

            mWaitForDatabaseReconcile = databaseBuilder.WaitForDatabaseReconcile();
            mDatabaseBuildTask = databaseBuilder.ReconcileDatabase();
        }

        public async void ContextReady( GLControl glControl ) {
            mGlControl = glControl;
            mGlControl.MakeCurrent();

            mMilkController.Initialize( mGlControl );

            if((!mWaitForDatabaseReconcile ) ||
               ( await mDatabaseBuildTask )) {
                mPresetController.Initialize();
                await mLightPipePump.Initialize();
                
                await mEventAggregator.PublishOnUIThreadAsync( new Events.InitializationComplete());
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
