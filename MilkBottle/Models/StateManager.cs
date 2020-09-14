using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    class StateManager : IStateManager {
        private readonly IMilkController    mMilkController;
        private readonly IPresetController  mPresetController;
        private readonly ILightPipePump     mLightPipePump;
        private bool                        mMilkControllerWasRunning;
        private bool                        mPresetControllerWasRunning;
        private bool                        mLightPipeWasRunning;
        private bool                        mIsSuspended;

        public  bool                        PresetControllerLocked {  get; private set; }
        public  bool                        IsRunning => mMilkController.IsRunning;

        public StateManager( IMilkController milkController, IPresetController presetController, ILightPipePump lightPipePump ) {
            mMilkController = milkController;
            mPresetController = presetController;
            mLightPipePump = lightPipePump;

            PresetControllerLocked = false;
            mIsSuspended = false;
            mMilkControllerWasRunning = false;
            mPresetControllerWasRunning = false;
        }

        public void SetPresetLock( bool toState ) {
            EnterState( toState ? eStateTriggers.Lock : eStateTriggers.Unlock );
        }

        public void EnterState( eStateTriggers toState ) {
            switch( toState ) {
                case eStateTriggers.Stop:
                    mMilkController.StopVisualization();
                    mPresetController.StopPresetCycling();
                    break;

                case eStateTriggers.Run:
                    mMilkController.StartVisualization();
                    if(!PresetControllerLocked ) {
                        mPresetController.StartPresetCycling();
                    }
                    break;

                case eStateTriggers.Lock:
                    mPresetController.StopPresetCycling();
                    PresetControllerLocked = true;
                    break;

                case eStateTriggers.Unlock:
                    mPresetController.StartPresetCycling();
                    PresetControllerLocked = false;
                    break;

                case eStateTriggers.Suspend:
                    mMilkControllerWasRunning = mMilkController.IsRunning;
                    mPresetControllerWasRunning = mPresetController.IsRunning;
                    mLightPipeWasRunning = mLightPipePump.IsEnabled;
                    mIsSuspended = true;

                    mPresetController.StopPresetCycling();
                    mMilkController.StopVisualization();
                    mLightPipePump.EnableLightPipe( false );
                    break;

                case eStateTriggers.Resume:
                    if( mIsSuspended ) {
                        if( mMilkControllerWasRunning ) {
                            mMilkController.StartVisualization();
                        }

                        if(( mPresetControllerWasRunning ) &&
                           (!PresetControllerLocked )) {
                            mPresetController.StartPresetCycling();
                        }

                        if( mLightPipeWasRunning ) {
                            mLightPipePump.EnableLightPipe( true, true );
                        }

                        mIsSuspended = false;
                    }
                    break;
            }
        }
    }
}
