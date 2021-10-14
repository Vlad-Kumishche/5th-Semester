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
        private string messageBuffer;
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

        public ComPort(string bitStuffingFlag)
        {
            if (!IsItBinarySequence(bitStuffingFlag) || bitStuffingFlag.Length != 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bitStuffingFlag));
            }

            this.bitStuffingFlag = bitStuffingFlag;
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
                ReceiveMessageEvent(message);
                DebugMessageEvent("Message received: " + message);
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

            if (!BitStuffing(message))
            {
                SendMessageEvent();
                return;
            }

            try
            {
                byte[] data = Encoding.Unicode.GetBytes(messageBuffer);
                _serialPort.Write(data, 0, data.Length);

                SendMessageEvent(messageBuffer);
                messageBuffer = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.Message);
            }
        }

        private bool BitStuffing(string message)
        {
            DebugMessageEvent("Added to buffer: " + message);
            messageBuffer += message;
            if (messageBuffer.Length < 8)
            {
                return false;
            }

            DebugMessageEvent("Before bit-stuffing: " + messageBuffer, "black");
            DebugMessageEvent("After bit-stuffing:   ", "black", false);

            int currentIndex = 0;
            while (currentIndex < messageBuffer.Length)
            {
                int index = messageBuffer.IndexOf(bitStuffingFlag, currentIndex);
                if (index == -1)
                {
                    DebugMessageEvent(messageBuffer[currentIndex..messageBuffer.Length], "black", false);
                    currentIndex = messageBuffer.Length;
                }
                else
                {
                    DebugMessageEvent(messageBuffer[currentIndex..index], "black", false);
                    DebugMessageEvent(messageBuffer[index..(index + 8)], "green", false);
                    messageBuffer = messageBuffer.Insert(index + 8, "1");
                    DebugMessageEvent(messageBuffer[index + 8].ToString(), "red", false);
                    currentIndex = index + 9;
                }
            }

            DebugMessageEvent();
            return true;
        }

        private string DeBitStuffing(string message)
        {
            return message.Replace(bitStuffingFlag + "1", bitStuffingFlag);
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
