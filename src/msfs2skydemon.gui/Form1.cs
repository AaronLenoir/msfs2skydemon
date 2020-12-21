using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace msfs2skydemon.gui
{
    public partial class Form1 : Form
    {
        private SimConnectWrapper _simConnectWrapper;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _simConnectWrapper = new SimConnectWrapper(this.Name, this.Handle);
            _simConnectWrapper.OnEmitMessage += _simConnectWrapper_OnEmitMessage;
            _simConnectWrapper.OnConnectionOpened += _simConnectWrapper_OnConnectionOpened;
            _simConnectWrapper.OnConnectionLost += _simConnectWrapper_OnConnectionLost;
            _simConnectWrapper.OnListeningStarted += _simConnectWrapper_OnListeningStarted;
            _simConnectWrapper.OnListeningStopped += _simConnectWrapper_OnListeningStopped;
            _simConnectWrapper.OnDataReceived += _simConnectWrapper_OnDataReceived;
        }

        DateTime _lastSendTime = DateTime.UtcNow;

        private void _simConnectWrapper_OnDataReceived(uint key, double value)
        {
            if (_simConnectWrapper.LatestData.Count == 6 && DateTime.UtcNow.Subtract(_lastSendTime).TotalSeconds >= 1)
            {
                _lastSendTime = DateTime.UtcNow;

                // We have all the data ...
                var longtitude = _simConnectWrapper.LatestData[1];
                var latitude = _simConnectWrapper.LatestData[2];
                var altitudeInFeet = _simConnectWrapper.LatestData[4];
                var altitudeInMeters = altitudeInFeet * 0.3048;
                var headingTrue = _simConnectWrapper.LatestData[3];
                var groundSpeed = _simConnectWrapper.LatestData[6];

                var xgpsMessage = $"XGPSMSFS,{longtitude:F2},{latitude:F2},{altitudeInMeters:F1},{headingTrue:F2},{groundSpeed:F1}";
                var message = $"[{DateTime.UtcNow}] {xgpsMessage}";
                txtData.Text = message;

                try
                {
                    var udpMessage = new UdpMessage(txtHost.Text, 49002, xgpsMessage);
                    udpMessage.Send();
                } catch (Exception ex)
                {
                    SetLastMessage($"Could not send to UDP: {ex.ToString()}");
                }
            }

            SetLastMessage($"Received data: {key} = {value} (total values: {_simConnectWrapper.LatestData.Count}) - (elapsed: {DateTime.UtcNow.Subtract(_lastSendTime).TotalSeconds})");
        }

        private void _simConnectWrapper_OnListeningStopped()
        {
            SetLastMessage("OnListeningStopped");
        }

        private void _simConnectWrapper_OnListeningStarted()
        {
            SetLastMessage("OnListeningStarted");
        }

        private void _simConnectWrapper_OnConnectionLost()
        {
            SetLastMessage("OnConnectionLost");
        }

        private void _simConnectWrapper_OnConnectionOpened()
        {
            SetLastMessage("OnConnectionOpened");
        }

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            if (_simConnectWrapper != null)
            {
                _simConnectWrapper.HandleWndProc(ref m);
            }

            base.WndProc(ref m);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (_simConnectWrapper != null)
                {
                    _simConnectWrapper.Connect();
                    _simConnectWrapper.StartListening();
                    button1.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                SetLastMessage($"Connection error: {ex.ToString()}");
            }
        }

        private void _simConnectWrapper_OnEmitMessage(string message)
        {
            SetLastMessage(message);
        }

        private void SetLastMessage(string text)
        {
            txtLastMessage.Text = $"[{DateTime.UtcNow.ToString()}] {text}";
        }
    }
}
