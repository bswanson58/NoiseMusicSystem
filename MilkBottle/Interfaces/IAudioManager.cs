using System;

namespace MilkBottle.Interfaces {
    interface IAudioManager : IDisposable {
        bool    InitializeAudioCapture();
        void    StartCapture( Action<byte[], int, int> onAudioData );
        void    StopCapture();
    }
}
