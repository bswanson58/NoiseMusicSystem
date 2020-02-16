namespace MilkBottle.Interfaces {
    enum eStateTriggers {
        Stop,
        Run,
        Lock,
        Unlock,
        Suspend,
        Resume
    }

    interface IStateManager {
        void    EnterState( eStateTriggers toState );

        bool    PresetControllerLocked { get; }
        void    SetPresetLock( bool toState );
    }
}
