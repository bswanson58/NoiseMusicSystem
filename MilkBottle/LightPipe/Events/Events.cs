using System;

namespace LightPipe.Events {
    public class FrameRendered {
        public IntPtr WindowPtr { get; }

        public FrameRendered( IntPtr hWnd ) {
            WindowPtr = hWnd;
        }
    }
}
