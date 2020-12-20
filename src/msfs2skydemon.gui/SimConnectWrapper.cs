using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using static Microsoft.FlightSimulator.SimConnect.SimConnect;

namespace msfs2skydemon
{


    public class SimConnectWrapper : IDisposable
    {
        #region Events

        public delegate void EmitMessageHandler(string message);
        public event EmitMessageHandler OnEmitMessage;

        public delegate void OnConnectionOpenedHandler();
        public event OnConnectionOpenedHandler OnConnectionOpened;

        public delegate void OnConnectionLostHandler();
        public event OnConnectionLostHandler OnConnectionLost;

        public delegate void OnListeningStartedHandler();
        public event OnListeningStartedHandler OnListeningStarted;

        public delegate void OnListeningStoppedHandler();
        public event OnListeningStoppedHandler OnListeningStopped;

        public delegate void OnDataReceivedHandler(uint key, double value);
        public event OnDataReceivedHandler OnDataReceived;

        #endregion

        private bool _connected = false;

        public Dictionary<uint, double> LatestData
        {
            get;
        }

        public enum DUMMYENUM
        {
            Dummy = 0
        }

        /// <summary>
        /// Contains the list of all the SimConnect properties we will read, the unit is separated by coma by our own code.
        /// </summary>
        Dictionary<int, string> simConnectProperties = new Dictionary<int, string>
        {
            {1,"PLANE LONGITUDE,degree" },
            {2,"PLANE LATITUDE,degree" },
            {3,"PLANE HEADING DEGREES TRUE,degree" },
            {4,"PLANE ALTITUDE,feet" },
            {5,"AIRSPEED INDICATED,knots" },
            {6,"GPS GROUND SPEED,knots" },
        };

        /// User-defined win32 event => put basically any number?
        private const int WM_USER_SIMCONNECT = 0x0402;

        public string Title { get; }
        public IntPtr Handle { get; }
        public SimConnect Sim { get; private set; }

        private Timer _timer;

        public SimConnectWrapper(string title) : this(title, Process.GetCurrentProcess().MainWindowHandle)
        { }

        public SimConnectWrapper(string title, IntPtr handle)
        {
            Title = title;
            Handle = handle;
            LatestData = new Dictionary<uint, double>();
        }

        public void Connect()
        {
            Sim = new SimConnect(Title, Handle, WM_USER_SIMCONNECT, null, 0);
            Sim.OnRecvOpen += Sim_OnRecvOpen;
            //Sim.OnRecvOpen += onReceiveOpenHandler;
            //Sim.OnRecvQuit += onReceiveQuitHandler;
            Sim.OnRecvSimobjectDataBytype += Sim_OnRecvSimobjectDataBytype;

            OnEmitMessage?.Invoke("Connected!");
        }

        private void Sim_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            double value = (double)data.dwData[0];
            uint key = data.dwRequestID;

            if (LatestData.ContainsKey(key))
            {
                LatestData[key] = value;
            } else
            {
                LatestData.Add(key, value);
            }

            OnDataReceived?.Invoke(key, value);
        }

        private void Sim_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {

            foreach (var toConnect in simConnectProperties)
            {
                var values = toConnect.Value.Split(new char[] { ',' });
                /// Define a data structure
                Sim.AddToDataDefinition((DUMMYENUM)toConnect.Key, values[0], values[1], SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                //GetLabelForUid(100 + toConnect.Key).Content = values[1];
                /// IMPORTANT: Register it with the simconnect managed wrapper marshaller
                /// If you skip this step, you will only receive a uint in the .dwData field.
                Sim.RegisterDataDefineStruct<double>((DUMMYENUM)toConnect.Key);
            }

            OnEmitMessage?.Invoke("Connection opened.");
            OnConnectionOpened?.Invoke();
            _connected = true;
        }

        public void StartListening()
        {
            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += _timer_Tick;
            _timer.Start();

            OnEmitMessage?.Invoke("Timer started ...");
            OnListeningStarted?.Invoke();
        }

        public void StopListening()
        {
            if (_timer != null)
            {
                var localTimer = _timer;
                _timer = null;
                localTimer.Stop();
                localTimer.Dispose();

                OnListeningStopped?.Invoke();
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            OnEmitMessage?.Invoke("Timer ticked ...");

            try
            {
                foreach (var toConnect in simConnectProperties)
                {
                    Sim.RequestDataOnSimObjectType((DUMMYENUM)toConnect.Key, (DUMMYENUM)toConnect.Key, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
                }

                if (!_connected)
                {
                    _connected = true;
                    OnConnectionOpened?.Invoke();
                }
            }
            catch (Exception ex)
            {
                OnEmitMessage?.Invoke($"Could not send Request for data: {ex.ToString()}");

                OnConnectionLost?.Invoke();
            }
        }

        public void HandleWndProc(ref Message messageData)
        {
            if (messageData.Msg == WM_USER_SIMCONNECT && Sim != null)
            {
                try
                {
                    Sim.ReceiveMessage();
                } catch (Exception ex)
                {
                    OnEmitMessage?.Invoke($"Could not receive message for data: {ex.ToString()}");
                }
            }
        }

        public void Dispose()
        {
            if (Sim != null)
            {
                var localSim = Sim;
                Sim = null;
                localSim.Dispose();
            }

            StopListening();
        }
    }
}
