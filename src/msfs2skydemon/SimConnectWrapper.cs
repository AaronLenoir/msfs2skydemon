using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static Microsoft.FlightSimulator.SimConnect.SimConnect;

namespace msfs2skydemon
{
    public class SimConnectWrapper : IDisposable
    {
        /// User-defined win32 event => put basically any number?
        private const int WM_USER_SIMCONNECT = 0x0402;

        public string Title { get; }
        public IntPtr Handle { get; }
        public SimConnect Sim { get; private set; }

        public SimConnectWrapper(string title) : this(title, Process.GetCurrentProcess().MainWindowHandle)
        { }

        public SimConnectWrapper(string title, IntPtr handle)
        {
            Title = title;
            Handle = handle;
        }

        public void Connect(RecvOpenEventHandler onReceiveOpenHandler,
                            RecvQuitEventHandler onReceiveQuitHandler,
                            RecvSimobjectDataBytypeEventHandler onReceiveDataHandler)
        {
            Sim = new SimConnect(Title, Handle, WM_USER_SIMCONNECT, null, 0);
            Sim.OnRecvOpen += onReceiveOpenHandler;
            Sim.OnRecvQuit += onReceiveQuitHandler;
            Sim.OnRecvSimobjectDataBytype += onReceiveDataHandler;
        }

        public void Dispose()
        {
            if (Sim != null)
            {
                Sim.Dispose();
                Sim = null;
            }
        }
    }
}
