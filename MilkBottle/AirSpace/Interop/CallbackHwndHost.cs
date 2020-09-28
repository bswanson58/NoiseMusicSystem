using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using AirSpace;
using System.Diagnostics;
using AirSpace.Win32.User32;

namespace AirSpace.Interop
{
    /// <summary>
    ///     A simple HwndHost that accepts callbacks for creating and
    ///     destroying the hosted window.
    /// </summary>
    public class CallbackHwndHost : HwndHostEx
    {
        public CallbackHwndHost(Func<HWND, HWND> buildWindow, Action<HWND> destroyWindow)
        {
            _buildWindow = buildWindow;
            _destroyWindow = destroyWindow;
        }

        protected override HWND BuildWindowOverride(HWND hwndParent)
        {
            return _buildWindow(hwndParent);
        }

        protected override void DestroyWindowOverride(HWND hwnd)
        {
            _destroyWindow(hwnd);
        }

        private Func<HWND, HWND> _buildWindow;
        private Action<HWND> _destroyWindow;
    }
}
