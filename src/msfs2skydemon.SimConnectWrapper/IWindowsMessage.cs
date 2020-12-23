using System;
using System.Collections.Generic;
using System.Text;

namespace msfs2skydemon.SimConnectWrapper
{
    public interface IWindowsMessage
    {
        //
        // Summary:
        //     Gets or sets the window handle of the message.
        //
        // Returns:
        //     The window handle of the message.
        IntPtr HWnd { get; set; }
        //
        // Summary:
        //     Gets or sets the ID number for the message.
        //
        // Returns:
        //     The ID number for the message.
        int Msg { get; set; }
        //
        // Summary:
        //     Gets or sets the System.Windows.Forms.Message.WParam field of the message.
        //
        // Returns:
        //     The System.Windows.Forms.Message.WParam field of the message.
        IntPtr WParam { get; set; }
        //
        // Summary:
        //     Specifies the System.Windows.Forms.Message.LParam field of the message.
        //
        // Returns:
        //     The System.Windows.Forms.Message.LParam field of the message.
        IntPtr LParam { get; set; }
        //
        // Summary:
        //     Specifies the value that is returned to Windows in response to handling the message.
        //
        // Returns:
        //     The return value of the message.
        IntPtr Result { get; set; }
    }
}
