using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com_ports_communication
{
    public partial class ComForm : Form
    {
        ComPort port;
        public ComForm()
        {
            InitializeComponent();

            port = new ComPort();
            port.SendMessageEvent += SendMessageHandler;
            port.ReceiveMessageEvent += ReceiveMessageHandler;
            port.DebugMessageEvent += DebugMessageHandler;
            port.ErrorEvent += ErrorHandler;

            BaudRateComboBox.SelectedIndex = BaudRateComboBox.Items.IndexOf("9600");
            ControlAndDebug.Text = "";
            PortOpeningFirstTime(int.Parse(BaudRateComboBox.Text));
        }

        public void PortOpeningFirstTime(int BaudRate)
        {
            try
            {
                port.OpenPort("COM1", BaudRate);
                DebugMessageHandler("COM1 is open");
                DebugMessageHandler("Flag: 00011001");
            }
            catch
            {
                try
                {
                    port.OpenPort("COM2", BaudRate);
                    DebugMessageHandler("COM2 is open");
                    DebugMessageHandler("Flag: 00011001");
                }
                catch
                {
                    ErrorHandler("Paired ports COM1 and COM2 were not found. Check COM1 and COM2 ports, then reopen this application.");
                }
            }
        }

        public void AppendText(RichTextBox box, string text, Color color, bool AddNewLine = true)
        {
            if (AddNewLine)
            {
                text += Environment.NewLine;
            }

            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
            box.SelectionStart = box.Text.Length;
            box.ScrollToCaret();
        }

        private void SendMessageHandler(string message)
        {
            Input.Clear();
        }

        private void ReceiveMessageHandler(string message)
        {
            Output.Text += message;
            Output.SelectionStart = Output.Text.Length;
            Output.ScrollToCaret();
        }

        private void DebugMessageHandler(string message, string color = "black", bool AddNewLine = true)
        {
            Color Color = color switch
            {
                "black" => Color.Black,
                "red" => Color.Red,
                "green" => Color.Green,
                _ => throw new ArgumentOutOfRangeException(nameof(color)),
            };

            AppendText(ControlAndDebug, message, Color, AddNewLine);
        }

        private void ErrorHandler(string message)
        {
            AppendText(ControlAndDebug, message, Color.Red);
        }

        private void BaudRateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            port.ClosePort();
            string portName = port.Name;
            try
            {
                port.OpenPort(portName, int.Parse(BaudRateComboBox.Text));
                DebugMessageHandler("Baudrate changed. Present value: " + BaudRateComboBox.Text);
            }
            catch
            {
                ErrorHandler("An error occurred when changing the baud rate of the " + portName + ". The port was closed. Check the status of the " + portName);
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            port.SerialPortDataSend(Input.Text);
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            Input.Text = "";
            Output.Text = "";
            ControlAndDebug.Text = "";
        }
    }
}
