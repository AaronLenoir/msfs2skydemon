using System;
using System.Windows.Forms;

namespace msfs2skydemon.gui
{
    public partial class Form1 : Form
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SimConnectWrapper _simConnectWrapper;

        private Timer _timer;

        DateTime _lastSendTime = DateTime.UtcNow;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Log.Debug("Loading form ...");

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
            if (DateTime.UtcNow.Subtract(_lastSendTime).TotalSeconds >= 1)
            {
                var longitude = _simConnectWrapper.LatestData[SimConnectProperties.PlaneLongitude];
                var latitude = _simConnectWrapper.LatestData[SimConnectProperties.PlaneLatitude];
                var altitude = _simConnectWrapper.LatestData[SimConnectProperties.PlaneAltitude] * 0.3048;
                var headingTrue = _simConnectWrapper.LatestData[SimConnectProperties.PlaneHeadingDegreesTrue];
                var groundSpeed = _simConnectWrapper.LatestData[SimConnectProperties.GpsGroundSpeed];

                if (longitude.HasValue && 
                    latitude.HasValue && 
                    altitude.HasValue &&
                    headingTrue.HasValue &&
                    groundSpeed.HasValue)
                {
                    var xgpsMessage = $"XGPSMSFS,{longitude:F2},{latitude:F2},{altitude:F1},{headingTrue:F2},{groundSpeed:F1}";
                    txtData.Text = $"[{DateTime.UtcNow}] {xgpsMessage}";
                    try
                    {
                        var udpMessage = new UdpMessage(txtHost.Text, 49002, xgpsMessage);
                        udpMessage.Send();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
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
