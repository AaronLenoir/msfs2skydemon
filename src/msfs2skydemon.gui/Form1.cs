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
                    _simConnectWrapper.Connect(Sim_OnRecvOpen, Sim_OnRecvQuit, Sim_OnRecvSimobjectDataBytype);
                    _simConnectWrapper.StartListening();
                    button1.Enabled = false;
                }
            } catch (Exception ex)
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

        private void Sim_OnRecvSimobjectDataBytype(Microsoft.FlightSimulator.SimConnect.SimConnect sender, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            this.txtData.Text = DateTime.UtcNow.ToString();

            double dValue = (double)data.dwData[0];

            this.txtData.Text = $"{data.dwRequestID}={dValue}";
        }

        private void Sim_OnRecvQuit(Microsoft.FlightSimulator.SimConnect.SimConnect sender, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_RECV data)
        {
            this.txtQuit.Text = DateTime.UtcNow.ToString();
        }

        private void Sim_OnRecvOpen(Microsoft.FlightSimulator.SimConnect.SimConnect sender, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_RECV_OPEN data)
        {
            this.txtOpen.Text = DateTime.UtcNow.ToString();
        }
    }
}
