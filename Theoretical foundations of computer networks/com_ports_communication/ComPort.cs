using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace com_ports_communication
{
    class ComPort
    {
        SerialPort _serialPort;
        public delegate void MessageHandler(string message);
        public delegate void ErrorHandler(string message);
        public event MessageHandler SendMessageEvent;
        public event MessageHandler ReceiveMessageEvent;
        public event ErrorHandler ErrorEvent;

        public bool OpenPort(string portName, int baudRate)
        {
            bool isOpen = false;
            try
            {
                _serialPort = new SerialPort
                {
                    PortName = portName,
                    BaudRate = baudRate
                };
                _serialPort.Open();
                isOpen = true;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.Message);
            }
            return isOpen;
        }

        public string[] GetgetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] data = new byte[_serialPort.BytesToRead];
                _serialPort.Read(data, 0, data.Length);

                ReceiveMessageEvent(Encoding.Unicode.GetString(data));
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.Message);
            }
        }

        public void SerialPort_DataSend(string message)
        {
            message = $"<{_serialPort.PortName}>: {message}\n";
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                _serialPort.Write(data, 0, data.Length);

                SendMessageEvent(message);
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.Message);
            }
        }

        public void ClosePort()
        {
            try
            {
                _serialPort.Close();
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.Message);
            }
        }
    }
}
