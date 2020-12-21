using System;
using System.Windows.Forms;

namespace msfs2skydemon.gui
{
    public partial class Form1 : Form
    {
        private SimConnectWrapper _simConnectWrapper;

        private Timer _timer;

        DateTime _lastSendTime = DateTime.UtcNow;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _simConnectWrapper = new SimConnectWrapper(Name, Handle, 
                new[] {
                    SimConnectProperties.PlaneLongitude,
                    SimConnectProperties.PlaneLatitude,
                    SimConnectProperties.PlaneAltitude,
                    SimConnectProperties.PlaneHeadingDegreesTrue,
                    SimConnectProperties.GpsGroundSpeed
                });

            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += CheckForData;
            _timer.Start();
        }

        private void CheckForData(object sender, EventArgs e)
        {
            if (_simConnectWrapper.LatestData.Count == 6 && DateTime.UtcNow.Subtract(_lastSendTime).TotalSeconds >= 1)
            {
                var longitude = _simConnectWrapper.LatestData[SimConnectProperties.PlaneLongitude];
                var latitude = _simConnectWrapper.LatestData[SimConnectProperties.PlaneLatitude];
                var altitude = _simConnectWrapper.LatestData[SimConnectProperties.PlaneAltitude] * 0.3048;
                var headingTrue = _simConnectWrapper.LatestData[SimConnectProperties.PlaneHeadingDegreesTrue];
                var groundSpeed = _simConnectWrapper.LatestData[SimConnectProperties.GpsGroundSpeed];

                var xgpsMessage = $"XGPSMSFS,{longitude:F2},{latitude:F2},{altitude:F1},{headingTrue:F2},{groundSpeed:F1}";
                txtData.Text = $"[{DateTime.UtcNow}] {xgpsMessage}";
                try
                {
                    var udpMessage = new UdpMessage(txtHost.Text, 49002, xgpsMessage);
                    udpMessage.Send();
                }
                catch
                {
                    // TODO: *something*!
                }
            }
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
    }
}
