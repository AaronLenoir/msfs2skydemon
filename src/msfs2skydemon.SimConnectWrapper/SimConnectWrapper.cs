using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace msfs2skydemon.SimConnectWrapper
{
    // SIMCONNECT_DATATYPE
    public struct String8
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string Value;
    }

    public struct String32
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Value;
    }

    public struct String64
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Value;
    }

    public struct String128
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Value;
    }

    public struct String256
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Value;
    }

    public struct String260
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string Value;
    }

    // TODO: STRINGV?

    public class SimConnectWrapper : IDisposable
    {
        public event EventHandler<Exception> OnError;

        // ID used to identify the SimConnect message in the Windows Message Loop
        private const int WM_USER_SIMCONNECT = 0x0402;

        public string Title { get; }

        public IntPtr Handle { get; }

        public SimConnect Sim { get; private set; }

        public Dictionary<SimConnectProperty, SimConnectPropertyValue> LatestData { get; }

        public DateTime LastDataReceivedOn { get; private set; }

        public List<SimConnectProperty> PropertiesToWatch { get; }

        private Timer _timer;

        private ISynchronizeInvoke _synchronizationObject;

        private bool _opened = false;

        private SimConnect _simConnect;

        public SimConnectWrapper(string title,
                                 IntPtr handle,
                                 ISynchronizeInvoke synchronizationObject,
                                 IEnumerable<SimConnectProperty> propertiesToWatch)
        {
            Title = title;
            Handle = handle;
            _synchronizationObject = synchronizationObject;
            PropertiesToWatch = propertiesToWatch.ToList();

            LastDataReceivedOn = DateTime.UtcNow.AddYears(-100);

            LatestData = new Dictionary<SimConnectProperty, SimConnectPropertyValue>();
            foreach (var property in propertiesToWatch)
            {
                LatestData.Add(property, new SimConnectPropertyValue());
            }

            _timer = StartTimer();
        }

        private Timer StartTimer()
        {
            Timer timer = new Timer();

            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += GetData;
            timer.SynchronizingObject = _synchronizationObject;
            timer.Start();

            return timer;
        }

        private void GetData(object sender, EventArgs e)
        {
            try
            {
                var connection = GetConnection();

                if (!_opened) { return; }

                foreach (var property in PropertiesToWatch)
                {
                    _simConnect.RequestDataOnSimObjectType(property.Key, property.Key, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
                }
            } catch (Exception ex)
            {
                if (_simConnect != null)
                {
                    _simConnect.Dispose();
                    _simConnect = null;
                }

                OnError?.Invoke(this, ex);
            }
        }

        private SimConnect GetConnection()
        {
            if (_simConnect == null)
            {
                _simConnect  = new SimConnect(Title, Handle, WM_USER_SIMCONNECT, null, 0);
                _simConnect.OnRecvOpen += SimConnect_OnRecvOpen;
                _simConnect.OnRecvSimobjectDataBytype += SimConnect_OnRecvSimobjectDataBytype;
            }

            return _simConnect;
        }

        public void HandleWndProc(int msg)
        {
            if (msg == WM_USER_SIMCONNECT && _simConnect != null)
            {
                try
                {
                    _simConnect.ReceiveMessage();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, ex);
                }
            }
        }

        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            foreach (var property in PropertiesToWatch)
            {
                _simConnect.AddToDataDefinition(property.Key, property.Name, property.Unit, property.SimConnectDataType, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                // TODO: Support other data types and structs ...
                if (property.SimConnectDataType == SIMCONNECT_DATATYPE.STRING64)
                {
                    _simConnect.RegisterDataDefineStruct<String64> (property.Key);
                } else
                {
                    _simConnect.RegisterDataDefineStruct<double>(property.Key);
                }
            }

            _opened = true;
        }

        private void RegisterDataDefineStruct(SIMCONNECT_DATATYPE dataType, SimConnectPropertyKey key)
        {
            if (dataType == SIMCONNECT_DATATYPE.STRING8)
            {
                _simConnect.RegisterDataDefineStruct<String8>(key);
            }
            else if (dataType == SIMCONNECT_DATATYPE.STRING32)
            {
                _simConnect.RegisterDataDefineStruct<String32>(key);
            }
            else if (dataType == SIMCONNECT_DATATYPE.STRING64)
            {
                _simConnect.RegisterDataDefineStruct<String64>(key);
            }
            else if (dataType == SIMCONNECT_DATATYPE.STRING128)
            {
                _simConnect.RegisterDataDefineStruct<String128>(key);
            }
            else if (dataType == SIMCONNECT_DATATYPE.STRING256)
            {
                _simConnect.RegisterDataDefineStruct<String256>(key);
            }
            else if (dataType == SIMCONNECT_DATATYPE.STRING260)
            {
                _simConnect.RegisterDataDefineStruct<String260>(key);
            }
            else if (dataType == SIMCONNECT_DATATYPE.INT32)
            {
                _simConnect.RegisterDataDefineStruct<Int32>(key);
            }
            else if (dataType == SIMCONNECT_DATATYPE.INT64)
            {
                _simConnect.RegisterDataDefineStruct<Int64>(key);
            }
            else
            {
                _simConnect.RegisterDataDefineStruct<double>(key);
            }
        }

        private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            LastDataReceivedOn = DateTime.UtcNow;

            // TODO: Support other data types and structs ...
            var property = PropertiesToWatch.SingleOrDefault(prop => (uint)prop.Key == data.dwRequestID);

            if (!property.IsEmpty)
            {
                LatestData[property] = GetValue(data, property.SimConnectDataType);
            }
        }

        private SimConnectPropertyValue GetValue(SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data, SIMCONNECT_DATATYPE dataType)
        {
            if (dataType == SIMCONNECT_DATATYPE.STRING8)
            {
                return new SimConnectPropertyValue(((String8)data.dwData[0]).Value);
            }
            else if (dataType == SIMCONNECT_DATATYPE.STRING64)
            {
                return new SimConnectPropertyValue(((String64)data.dwData[0]).Value);
            }
            else
            {
                return new SimConnectPropertyValue(data.dwData[0]);
            }
        }

        public void Dispose()
        {
            if (_timer != null) {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

            if (_simConnect != null) {
                _simConnect.Dispose();
                _simConnect = null;
            }
        }
    }
}
