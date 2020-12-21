﻿using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace msfs2skydemon.gui
{
    public class SimConnectWrapper : IDisposable
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // ID used to identify the SimConnect message in the Windows Message Loop
        private const int WM_USER_SIMCONNECT = 0x0402;

        public string Title { get; }

        public IntPtr Handle { get; }

        public SimConnect Sim { get; private set; }

        public Dictionary<SimConnectProperty, double?> LatestData { get; }

        public DateTime LastDataReceivedOn { get; private set; }

        public List<SimConnectProperty> PropertiesToWatch { get; }

        private Timer _timer;

        private bool _opened = false;

        private SimConnect _simConnect;

        public SimConnectWrapper(string title, 
                                 IntPtr handle,
                                 IEnumerable<SimConnectProperty> propertiesToWatch)
        {
            Title = title;
            Handle = handle;
            PropertiesToWatch = propertiesToWatch.ToList();

            LatestData = new Dictionary<SimConnectProperty, double?>();
            foreach (var property in propertiesToWatch)
            {
                LatestData.Add(property, null);
            }

            _timer = StartTimer();
        }

        private Timer StartTimer()
        {
            Timer timer = new Timer();

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += GetData;
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

                Log.Warn(ex);
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

        public void HandleWndProc(ref Message messageData)
        {
            if (messageData.Msg == WM_USER_SIMCONNECT && _simConnect != null)
            {
                try
                {
                    _simConnect.ReceiveMessage();
                }
                catch (Exception ex)
                {
                    Log.Warn(ex);
                }
            }
        }

        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            foreach (var property in PropertiesToWatch)
            {
                _simConnect.AddToDataDefinition(property.Key, property.Name, property.Unit, property.SimConnectDataType, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                // TODO: Support other data types and structs ...
                _simConnect.RegisterDataDefineStruct<double>(property.Key);
            }

            _opened = true;
        }

        private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            LastDataReceivedOn = DateTime.UtcNow;

            // TODO: Support other data types and structs ...
            double value = (double)data.dwData[0];

            var property = PropertiesToWatch.SingleOrDefault(prop => (uint)prop.Key == data.dwRequestID);

            if (!property.IsEmpty)
            {
                LatestData[property] = value;
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
