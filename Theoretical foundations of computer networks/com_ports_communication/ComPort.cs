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
        public delegate void MessageHandler(string message = "");
        public delegate void DebugMessageHandler(string message = "", string color = "black", bool AddNewLine = true);
        public delegate void ErrorHandler(string message);
        public event MessageHandler SendMessageEvent;
        public event MessageHandler ReceiveMessageEvent;
        public event DebugMessageHandler DebugMessageEvent;
        public event ErrorHandler ErrorEvent;
        private readonly string bitStuffingFlag;
        private readonly int frameLength;
        private readonly int frameLengthL;
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
            this.frameLength = 25;
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
                int?[] errorPositions = new int?[] { null, null };
                message = GenerateError(message, ref errorPositions);

                if (errorPositions[0] is not null && errorPositions[1] is not null)
                {
                    int firstError, secondError;
                    if (errorPositions[0] < errorPositions[1])
                    {
                        firstError = (int)errorPositions[0];
                        secondError = (int)errorPositions[1];

                    }
                    else
                    {
                        firstError = (int)errorPositions[1];
                        secondError = (int)errorPositions[0];
                    }

                    DebugMessageEvent(message[..(firstError + 1)], "black", false);
                    DebugMessageEvent(message[(firstError + 1)..(firstError + 2)], "red", false);
                    DebugMessageEvent(message[(firstError + 2)..(secondError + 1)], "black", false);
                    DebugMessageEvent(message[(secondError + 1)..(secondError + 2)], "red", false);
                    DebugMessageEvent(message[(secondError + 2)..this.frameLength], "black", false);
                    DebugMessageEvent(":", "black", false);
                    DebugMessageEvent(message[this.frameLength..(this.frameLength + 1)], "orange", false);
                    DebugMessageEvent(":", "black", false);
                    DebugMessageEvent(message[(this.frameLength + 1)..], "green");
                }
                else if (errorPositions[0] is not null && errorPositions[1] is null)
                {
                    int firstError = (int)errorPositions[0];
                    DebugMessageEvent(message[..(firstError + 1)], "black", false);
                    DebugMessageEvent(message[(firstError + 1)..(firstError + 2)], "red", false);
                    DebugMessageEvent(message[(firstError + 2)..this.frameLength], "black", false);
                    DebugMessageEvent(":", "black", false);
                    DebugMessageEvent(message[this.frameLength..(this.frameLength + 1)], "orange", false);
                    DebugMessageEvent(":", "black", false);
                    DebugMessageEvent(message[(this.frameLength + 1)..], "green");
                }
                else
                {
                    DebugMessageEvent(message[..this.frameLength], "black", false);
                    DebugMessageEvent(":", "black", false);
                    DebugMessageEvent(message[this.frameLength..(this.frameLength + 1)], "orange", false);
                    DebugMessageEvent(":", "black", false);
                    DebugMessageEvent(message[(this.frameLength + 1)..], "green");
                }

                //message = "1110010100111010011001011" + message[^6..];

                //DebugMessageEvent("Message after error:\r\n" + message);
                string receivedСode = message[^6..];
                message = GenerateHammingCode(message[..this.frameLength]);

                DebugMessageEvent("Hamming's code is calculated:\r\n" + message[..this.frameLength], "black", false);
                DebugMessageEvent(":", "black", false);
                DebugMessageEvent(message[this.frameLength..(this.frameLength + 1)], "orange", false);
                DebugMessageEvent(":", "black", false);
                DebugMessageEvent(message[(this.frameLength + 1)..], "green");

                string calculatedСode = message[^6..];
                //DebugMessageEvent("receivedСode:\r\n" + receivedСode + "\r\ncalculatedСode:\r\n" + calculatedСode);
                int receivedСodeNumber = Convert.ToInt32(receivedСode[1..], 2);
                int calculatedСodeNumber = Convert.ToInt32(calculatedСode[1..], 2);
                int difference = receivedСodeNumber ^ calculatedСodeNumber;
                string differenceB = Convert.ToString(difference, 2);
                if (difference != 0 && receivedСode[0] != calculatedСode[0])  // single error
                {
                    DebugMessageEvent("Single error detected.");
                    char bit = message[difference - 1];
                    message = message.Remove(difference - 1, 1);
                    message = message.Insert(difference - 1, (bit == '1' ? '0' : '1').ToString());

                    DebugMessageEvent("Message after recovery:");
                    DebugMessageEvent(message[..(difference - 1)], "black", false);
                    DebugMessageEvent(message[(difference - 1)..difference], "green", false);
                    DebugMessageEvent(message[difference..this.frameLength], "black");
                    ReceiveMessageEvent(message[..this.frameLength] + Environment.NewLine);
                }
                else if (difference != 0 && receivedСode[0] == calculatedСode[0]) // double error
                {
                    DebugMessageEvent("Double error detected!", "red");
                    DebugMessageEvent("The message will not be displayed in the \"Output\" window.", "red");
                }
                else
                {
                    DebugMessageEvent("No errors found: " + message[..this.frameLength]);
                    ReceiveMessageEvent(message[..this.frameLength] + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.Message);
            }

            
        }

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private void ReceivedTest(string mes)
        {
            string message = DeBitStuffing(mes);
            int?[] errorPositions = new int?[] { null, null };
            message = GenerateError(message, ref errorPositions);

            if (errorPositions[0] is not null && errorPositions[1] is not null)
            {
                int firstError, secondError;
                if (errorPositions[0] < errorPositions[1])
                {
                    firstError = (int)errorPositions[0];
                    secondError = (int)errorPositions[1];
                    
                }
                else
                {
                    firstError = (int)errorPositions[1];
                    secondError = (int)errorPositions[0];
                }

                DebugMessageEvent(message[..(firstError + 1)], "black", false);
                DebugMessageEvent(message[(firstError + 1)..(firstError + 2)], "red", false);
                DebugMessageEvent(message[(firstError + 2)..(secondError + 1)], "black", false);
                DebugMessageEvent(message[(secondError + 1)..(secondError + 2)], "red", false);
                DebugMessageEvent(message[(secondError + 2)..this.frameLength], "black", false);
                DebugMessageEvent(":", "black", false);
                DebugMessageEvent(message[this.frameLength..(this.frameLength + 1)], "orange", false);
                DebugMessageEvent(":", "black", false);
                DebugMessageEvent(message[(this.frameLength + 1)..], "green");
            }
            else if (errorPositions[0] is not null && errorPositions[1] is null)
            {
                int firstError = (int)errorPositions[0];
                DebugMessageEvent(message[..(firstError + 1)], "black", false);
                DebugMessageEvent(message[(firstError + 1)..(firstError + 2)], "red", false);
                DebugMessageEvent(message[(firstError + 2)..this.frameLength], "black", false);
                DebugMessageEvent(":", "black", false);
                DebugMessageEvent(message[this.frameLength..(this.frameLength + 1)], "orange", false);
                DebugMessageEvent(":", "black", false);
                DebugMessageEvent(message[(this.frameLength + 1)..], "green");
            }
            else
            {
                DebugMessageEvent(message[..this.frameLength], "black", false);
                DebugMessageEvent(":", "black", false);
                DebugMessageEvent(message[this.frameLength..(this.frameLength + 1)], "orange", false);
                DebugMessageEvent(":", "black", false);
                DebugMessageEvent(message[(this.frameLength + 1)..], "green");
            }

            //message = "1110010100111010011001011" + message[^6..];

            //DebugMessageEvent("Message after error:\r\n" + message);
            string receivedСode = message[^6..];
            message = GenerateHammingCode(message[..this.frameLength]);

            DebugMessageEvent("Hamming's code is calculated:\r\n" + message[..this.frameLength], "black", false);
            DebugMessageEvent(":", "black", false);
            DebugMessageEvent(message[this.frameLength..(this.frameLength + 1)], "orange", false);
            DebugMessageEvent(":", "black", false);
            DebugMessageEvent(message[(this.frameLength + 1)..], "green");

            string calculatedСode = message[^6..];
            //DebugMessageEvent("receivedСode:\r\n" + receivedСode + "\r\ncalculatedСode:\r\n" + calculatedСode);
            int receivedСodeNumber = Convert.ToInt32(receivedСode[1..], 2);
            int calculatedСodeNumber = Convert.ToInt32(calculatedСode[1..], 2);
            int difference = receivedСodeNumber ^ calculatedСodeNumber;
            string differenceB = Convert.ToString(difference, 2);
            if (difference != 0 && receivedСode[0] != calculatedСode[0])  // single error
            {
                DebugMessageEvent("Single error detected.");
                char bit = message[difference - 1];
                message = message.Remove(difference - 1, 1);
                message = message.Insert(difference - 1, (bit == '1' ? '0' : '1').ToString());

                DebugMessageEvent("Message after recovery:");
                DebugMessageEvent(message[..(difference - 1)], "black", false);
                DebugMessageEvent(message[(difference - 1)..difference], "green", false);
                DebugMessageEvent(message[difference..this.frameLength], "black");
                ReceiveMessageEvent(message[..this.frameLength] + Environment.NewLine);
            }
            else if (difference != 0 && receivedСode[0] == calculatedСode[0]) // double error
            {
                DebugMessageEvent("Double error detected!", "red");
                DebugMessageEvent("The message will not be displayed in the \"Output\" window.", "red");
            }
            else
            {
                DebugMessageEvent("No errors found: " + message[..this.frameLength]);
                ReceiveMessageEvent(message[..this.frameLength] + Environment.NewLine);
            }
        }

        private string GenerateError(string message, ref int?[] errorPositions)
        {
            var rand = new Random();
            int result = rand.Next(11);
            int errorCount = result <= 5 ? 1 : 0;
            errorCount = result == 6 || result == 7 ? 2 : errorCount;
            //errorCount = 2;
            if (errorCount == 0)
            {
                DebugMessageEvent("Message received (no error was generated):");
            }

            if (errorCount == 1)
            {
                DebugMessageEvent("Message received (single error was generated):");
            }

            if (errorCount == 2)
            {
                DebugMessageEvent("Message received (", "black", false);
                DebugMessageEvent("double error was generated", "red", false);
                DebugMessageEvent("):");
            }

            int? changedBit = null;
            for (int i = 0; i < errorCount; i++)
            {
                int bitToChange;
                do
                {
                    bitToChange = rand.Next(1, this.frameLength - 3);
                }
                while (bitToChange == changedBit);

                char bit = message[bitToChange + 1];
                message = message.Insert(bitToChange + 1, bit == '1' ? "0" : "1");
                message = message.Remove(bitToChange + 2, 1);
                
                errorPositions[i] = bitToChange;
                changedBit = bitToChange;
            }

            return message;
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

            //message = "1110010100111010001001011";

            DebugMessageEvent("Input:\r\n" + message);
            int countOfFrames = (int)Math.Ceiling((double)message.Length / frameLength);
            while (countOfFrames != 0)
            {
                int length = message.Length;
                string len = Convert.ToString(length, 2);
                len = len.PadLeft(frameLength, '0');
                string currentFrame = countOfFrames > 1 ? message[..this.frameLength] : message;
                message = message[currentFrame.Length..];
                currentFrame = GenerateHammingCode(currentFrame);
                DebugMessageEvent("Sent message:\r\n" + currentFrame[..this.frameLength], "black", false);
                DebugMessageEvent(":", "black", false);
                DebugMessageEvent(currentFrame[this.frameLength..(this.frameLength + 1)], "orange", false);
                DebugMessageEvent(":", "black", false);
                DebugMessageEvent(currentFrame[(this.frameLength + 1)..], "green");
                currentFrame = BitStuffing(currentFrame);
                try
                {
                    byte[] data = Encoding.Unicode.GetBytes(currentFrame);
                    _serialPort.Write(data, 0, data.Length);
                    //ReceivedTest(currentFrame);
                    SendMessageEvent(currentFrame);
                }
                catch (Exception ex)
                {
                    ErrorEvent(ex.Message);
                }

                Thread.Sleep(1);
                countOfFrames--;
            }
        }

        private string GenerateHammingCode(string frame)
        {
            frame = frame.PadRight(frameLength, '0');
            List<int> numbersOfOnes = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                numbersOfOnes.Add(0);
            }

            int oneClock = 0, twoClock = 0, fourClock = 0, eightClock = 0, sixteenClock = 0;
            bool oneReadFlag = false, twoReadFlag = false, fourReadFlag = false, eightReadFlag = false, sixteenReadFlag = false;
            for (int i = 1; i <= frame.Length; i++)
            {
                oneClock++;
                twoClock++;
                fourClock++;
                eightClock++;
                sixteenClock++;

                if (oneClock == 0) { oneReadFlag = false; }
                if (twoClock == 0) { twoReadFlag = false; }
                if (fourClock == 0) { fourReadFlag = false; }
                if (eightClock == 0) { eightReadFlag = false; }
                if (sixteenClock == 0) { sixteenReadFlag = false; }

                if (oneClock == 1) { oneReadFlag = true; }
                if (twoClock == 2) { twoReadFlag = true; }
                if (fourClock == 4) { fourReadFlag = true; }
                if (eightClock == 8) { eightReadFlag = true; }
                if (sixteenClock == 16) { sixteenReadFlag = true; }

                if (oneReadFlag)
                {
                    oneClock -= 2;
                    if (frame[i - 1] == '1') { numbersOfOnes[0]++; }
                }

                if (twoReadFlag)
                {
                    twoClock -= 2;
                    if(frame[i - 1] == '1') { numbersOfOnes[1]++; }
                }

                if (fourReadFlag)
                {
                    fourClock -= 2;
                    if(frame[i - 1] == '1') { numbersOfOnes[2]++; }
                }

                if (eightReadFlag)
                {
                    eightClock -= 2;
                    if(frame[i - 1] == '1') { numbersOfOnes[3]++; }
                }

                if (sixteenReadFlag)
                {
                    sixteenClock -= 2;
                    if(frame[i - 1] == '1') { numbersOfOnes[4]++; }
                }

                if (frame[i - 1] == '1') { numbersOfOnes[5]++; }
            }

            for (int i = 0; i < 6; i++)
            {
                numbersOfOnes[i] %= 2;
            }

            string controlCode = string.Join(string.Empty, numbersOfOnes);
            return frame + Reverse(controlCode);
        }

        private string BitStuffing(string message)
        {
            string result = sentMessagesBuffer + message;
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
                        result = result.Insert(index + 7, "0");
                        currentIndex = index + 6;
                    }
                }
            }

            result = result[sentMessagesBuffer.Length..];
            MessagesBufferUpdate(ref sentMessagesBuffer, ref result);
            return result;
        }

        private string DeBitStuffing(string message)
        {
            string result = receivedMessagesBuffer + message;
            if (result.Length >= 8)
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
            MessagesBufferUpdate(ref receivedMessagesBuffer, ref result);
            return result;
        }

        private void MessagesBufferUpdate(ref string buffer, ref string message)
        {
            buffer += message;
            if (buffer.Length >= 7)
            {
                buffer = buffer[^7..];
                if (buffer == bitStuffingFlag[..7])
                {
                    buffer = buffer[^6..];
                }
            }
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
