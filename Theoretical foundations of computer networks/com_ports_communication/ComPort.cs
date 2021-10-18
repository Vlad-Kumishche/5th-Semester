using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Drawing;

namespace com_ports_communication
{
    class ComPort
    {
        SerialPort _serialPort;
        public delegate void MessageHandler(string message = "");
        public delegate void DebugMessageHandler(string message = "", string color = "black", bool AddNewLine = true);
        public delegate void ErrorHandler(string message);
        public event MessageHandler SendMessageEvent;
        public event MessageHandler ReceiveMessageEvent;
        public event DebugMessageHandler DebugMessageEvent;
        public event ErrorHandler ErrorEvent;
        private readonly string bitStuffingFlag;
        private string sentMessagesBuffer;
        private string receivedMessagesBuffer;
        private string name;
        public string Name
        {
            get
            {
                return name;
            }

            private set
            {
                name = value;
            }
        }

        public ComPort()
        {
            this.bitStuffingFlag = "00011001";
            this.sentMessagesBuffer = string.Empty;
            this.receivedMessagesBuffer = string.Empty;
        }

        public void OpenPort(string portName, int baudRate)
        {
            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate
            };
            _serialPort.Open();
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPortDataReceived); 
            Name = portName;
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] data = new byte[_serialPort.BytesToRead];
                _serialPort.Read(data, 0, data.Length);

                string message = DeBitStuffing(Encoding.Unicode.GetString(data));
                ReceiveMessageEvent(message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.Message);
            }
        }

        public void SerialPortDataSend(string message)
        {
            if (!IsItBinarySequence(message))
            {
                ErrorEvent("Invalid input. Only 1 and 0 are allowed");
                return;
            }

            if (message == string.Empty)
            {
                ErrorEvent("Blank messages are prohibited");
                return;
            }

            DebugMessageEvent("Input: " + message);

            message = BitStuffing(message);

            try
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                _serialPort.Write(data, 0, data.Length);

                SendMessageEvent(message);
                message = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.Message);
            }
        }

        private string BitStuffing(string message)
        {
            string result = sentMessagesBuffer + message;
            bool bitStuffingHappened = false;
            if (result.Length >= 7)
            {
                int currentIndex = 0;
                while (currentIndex < result.Length)
                {
                    int index = result.IndexOf(bitStuffingFlag[..7], currentIndex);
                    if (index == -1)
                    {
                        currentIndex = result.Length;
                    }
                    else
                    {
                        result = result.Insert(index + 7, "[0]");
                        currentIndex = index + 5;
                        bitStuffingHappened = true;
                    }
                }
            }

            result = result[sentMessagesBuffer.Length..];
            sentMessagesBuffer += message;
            if (sentMessagesBuffer.Length >= 7)
            {
                sentMessagesBuffer = sentMessagesBuffer[^7..];
                if (sentMessagesBuffer == bitStuffingFlag[..7])
                {
                    sentMessagesBuffer = sentMessagesBuffer[^6..];
                }
            }

            if (bitStuffingHappened)
            {
                DebugMessageEvent("After bit-stuffing: " + result, "black");
            }

            result = result.Replace("[", "");
            result = result.Replace("]", "");
            return result;
        }

        private string DeBitStuffing(string message)
        {
            string result = receivedMessagesBuffer + message;
            bool needForDeBitStuffing = result.Length >= 8;
            if (needForDeBitStuffing)
            {
                int currentIndex = 0;
                while (currentIndex < result.Length)
                {
                    int index = result.IndexOf(bitStuffingFlag[..7] + "0", currentIndex);
                    if (index == -1)
                    {
                        currentIndex = result.Length;
                    }
                    else
                    {
                        result = result.Remove(index + 7, 1);
                        currentIndex = index + 5;
                    }
                }
            }

            result = result[receivedMessagesBuffer.Length..];
            receivedMessagesBuffer += result;
            if (receivedMessagesBuffer.Length >= 7)
            {
                receivedMessagesBuffer = receivedMessagesBuffer[^7..];
                if (receivedMessagesBuffer == bitStuffingFlag[..7])
                {
                    receivedMessagesBuffer = receivedMessagesBuffer[^6..];
                }
            }

            return result;
        }

        private bool IsItBinarySequence(string message)
        {
            foreach (char letter in message)
            {
                if (letter != '1' && letter != '0')
                {
                    return false;
                }
            }
            return true;
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
