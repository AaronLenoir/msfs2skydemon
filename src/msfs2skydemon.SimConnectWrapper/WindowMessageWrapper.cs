using System;
using System.Collections.Generic;
using System.Text;

namespace msfs2skydemon.SimConnectWrapper
{
    public class WindowMessageWrapper : IWindowsMessage
    {
        public WindowMessageWrapper(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, IntPtr result)
        {
            HWnd = hWnd;
            Msg = msg;
            WParam = wParam;
            LParam = lParam;
            Result = result;
        }

        public IntPtr HWnd { get; set; }
        public int Msg { get; set; }
        public IntPtr WParam { get; set; }
        public IntPtr LParam { get; set; }
        public IntPtr Result { get; set; }
    }
}
