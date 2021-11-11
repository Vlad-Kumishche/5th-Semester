using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Drawing;
using System.Threading;

namespace com_ports_communication
{
    class ComPort
    {
        SerialPort _serialPort;
        public delegate void SendMessageHandler(string message = "");
        public delegate void ReceiveMessageHandler(string message = "", int charsToDelete = 0);
        public delegate void DebugMessageHandler(string message = "", string color = "black", bool AddNewLine = true);
        public delegate void ErrorHandler(string message);
        public event SendMessageHandler SendMessageEvent;
        public event ReceiveMessageHandler ReceiveMessageEvent;
        public event DebugMessageHandler DebugMessageEvent;
        public event ErrorHandler ErrorEvent;
        private readonly int frameLength;
        private readonly int maxAttempts;
        private readonly int collisionWindowDuration;
        private readonly string collisionSymbol;
        private readonly string JAM;
        private readonly string endOfMessage;
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
            this.frameLength = 25;
            this.maxAttempts = 15;
            this.collisionWindowDuration = 20;
            this.collisionSymbol = "*";
            this.JAM = "*";
            this.endOfMessage = Environment.NewLine;
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
                string message = Encoding.Unicode.GetString(data);

                if (message.Length == 5)
                {
                    int charsToDelete = Convert.ToInt32(message, 2);
                    ReceiveMessageEvent(this.endOfMessage, charsToDelete);
                }
                else if (message == this.JAM)
                {
                    ReceiveMessageEvent(string.Empty, this.frameLength);
                }
                else
                {
                    
                    ReceiveMessageEvent(message);
                }
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

            int countOfFrames = (int)Math.Ceiling((double)message.Length / this.frameLength);
            string currentFrame = string.Empty;
            int realLenthOfLastFrame = 0;
            while (countOfFrames != 0)
            {
                currentFrame = countOfFrames > 1 ? message[..this.frameLength] : message;
                realLenthOfLastFrame = currentFrame.Length;
                message = message[currentFrame.Length..];
                currentFrame = currentFrame.PadRight(this.frameLength, '0');
                if (!CSMA_CD_Send_Algorithm(currentFrame))
                {
                    DebugMessageEvent();
                    DebugMessageEvent("The maximum number of sending attempts has been exceeded", "red", false);
                }

                DebugMessageEvent();
                countOfFrames--;
            }

            int charsToDelete = this.frameLength - realLenthOfLastFrame;
            string extraFrame = Convert.ToString(charsToDelete, 2);
            extraFrame = extraFrame.PadLeft(5, '0');
            SendData(extraFrame);
        }

        private bool CSMA_CD_Send_Algorithm(string frame)
        {
            int attemptsCounter = 0;
            DebugMessageEvent(frame + ":", "black", false);
            while (true)
            {
                while (isChannelBusy()) { /*wait until the channel isn't busy*/ }
                SendData(frame);
                Thread.Sleep(this.collisionWindowDuration);
                if (isCollision())
                {
                    SendData(JAM);
                    Thread.Sleep(this.collisionWindowDuration);
                    DebugMessageEvent(this.collisionSymbol, "black", false);
                    attemptsCounter++;
                    if (attemptsCounter > this.maxAttempts)
                    {
                        return false;
                    }

                    int randomDelay = new Random().Next((int)Math.Pow(2.0d, Math.Min(attemptsCounter, 10))) * 3;
                    Thread.Sleep(randomDelay);
                }
                else
                {
                    return true;
                }
            }
        }
        private void SendData(string frame)
        {
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(frame);
                _serialPort.Write(data, 0, data.Length);
                SendMessageEvent(frame);
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.Message);
            }
        }

        private bool isChannelBusy()
        {
            return new Random().Next(10) % 5 == 0;
        }

        private bool isCollision()
        {
            return new Random().Next(10) % 2 == 0;
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
