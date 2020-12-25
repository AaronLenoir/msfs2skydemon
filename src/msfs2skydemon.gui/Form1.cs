using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using msfs2skydemon.SimConnectWrapper;

namespace msfs2skydemon.gui
{
    public partial class Form1 : Form
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SimConnectWrapper.SimConnectWrapper _simConnectWrapper;

        private Timer _timer;

        DateTime _lastSendTime = DateTime.UtcNow;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Log.Debug("Loading form ...");

            _simConnectWrapper = new SimConnectWrapper.SimConnectWrapper(Name, Handle, this,
                new[] {
                    SimConnectProperties.PlaneLongitude,
                    SimConnectProperties.PlaneLatitude,
                    SimConnectProperties.PlaneAltitude,
                    SimConnectProperties.PlaneHeadingDegreesTrue,
                    SimConnectProperties.GpsGroundSpeed
                });
            _simConnectWrapper.OnError += _simConnectWrapper_OnError;

            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += CheckForData;
            _timer.Start();

            chkBroadcast.Checked = Properties.Settings.Default.UdpBroadcast;
            txtHost.Text = Properties.Settings.Default.UdpTargetHost;
        }

        private void CheckForData(object sender, EventArgs e)
        {
            if (DateTime.UtcNow.Subtract(_simConnectWrapper.LastDataReceivedOn).TotalSeconds >= 5)
            {
                SetConnectionStatus(false);
                return;
            }

            SetConnectionStatus(true);

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
                    var xgpsMessage = $"XGPSMSFS,{longitude:F6},{latitude:F6},{altitude:F1},{headingTrue:F2},{groundSpeed:F6}";
                    try
                    {
                        var host = txtHost.Text;
                        if (chkBroadcast.Checked) { host = "255.255.255.255"; }

                        Log.Debug($"Send '{xgpsMessage}' to {host}");

                        var udpMessage = new UdpMessage(host, 49002, xgpsMessage);
                        udpMessage.Send();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }
            }
        }

        private void SetConnectionStatus(bool connectionOk)
        {
            if (connectionOk)
            {
                txtConnectionStatus.Text = "Connected";
                txtConnectionStatus.ForeColor = Color.LimeGreen;
            } else
            {
                txtConnectionStatus.Text = "Connecting ...";
                if (txtConnectionStatus.ForeColor == Color.DarkRed)
                {
                    txtConnectionStatus.ForeColor = Color.IndianRed;
                } else
                {
                    txtConnectionStatus.ForeColor = Color.DarkRed;
                }
            }
        }

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            if (_simConnectWrapper != null)
            {
                _simConnectWrapper.HandleWndProc(m.Msg);
            }

            base.WndProc(ref m);
        }

        private void ChkBroadcast_CheckedChanged(object sender, EventArgs e)
        {
            txtHost.Enabled = !chkBroadcast.Checked;
            Properties.Settings.Default.UdpBroadcast = chkBroadcast.Checked;
            Properties.Settings.Default.Save();
        }

        private void TxtHost_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.UdpTargetHost = txtHost.Text;
            Properties.Settings.Default.Save();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _simConnectWrapper.Dispose();
            Application.Exit();
        }

        private void OpenLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("msfs2skydemon.log.txt");
        }

        private void _simConnectWrapper_OnError(object sender, Exception exception)
        {
            Log.Error(exception);
        }
    }
}
