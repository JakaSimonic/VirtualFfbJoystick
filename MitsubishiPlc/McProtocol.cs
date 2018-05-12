using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MitsubishiPlc
{
    public class McProtocol : IDisposable
    {
        #region Property

        public bool TcpConnected { get; private set; }
        public bool UdpConnected { get; private set; }
        public string SendText { get; private set; }
        public int ReturnLenght { get; private set; }
        public string ReturnText { get; private set; }

        #endregion Property

        #region Variables

        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private UdpClient udpClient;
        private IPEndPoint udpEp;
        private bool disposed = false;

        private bool IsTcp;

        //values copied from Mitzibushi sample code
        private string monitoringTimer = "1000";

        private string networkNo = "00";
        private string pcNo = "FF";
        private string RequDestIONo = "FF03";
        private string RequDestStNo = "00";
        //end of values copied from Mitzibushi sample code

        // Define Help Strings
        private string Commandtext = "";

        private string Subcom = "";
        private string deviceOperand = "";

        #endregion Variables

        #region Constructors

        public McProtocol()
        {
            TcpConnected = false;
            UdpConnected = false;
        }

        public McProtocol(string monitoringTimer, string networkNo, string pcNo, string RequDestIONo, string RequDestStNo)
        {
            this.monitoringTimer = monitoringTimer;
            this.networkNo = networkNo;
            this.pcNo = pcNo;
            this.RequDestIONo = RequDestIONo;
            this.RequDestStNo = RequDestStNo;

            TcpConnected = false;
            UdpConnected = false;
        }

        #endregion Constructors

        #region CommandConstructors

        public void WriteWordWise(int[] values, string device, int startAddress)
        {
            Commandtext = "0114"; // Command for write
            Subcom = "0000";
            SetDeviceOpernad(device);
            string Values = "";
            for (int i = 0; i < values.Length; i++)
            {
                int commandlength1 = values[i] % 256;
                int commandlength2 = (values[i] - commandlength1) / 256;
                Values = Values + HEX2((byte)commandlength1) + HEX2((byte)commandlength2);
            }

            byte[] bytes = SendWriteCommand(Values, startAddress, values.Length);

            ReturnLenght = (bytes[7] + bytes[8] * 256) / 2 - 1;
            int returnValue;
            ReturnText = "";
            for (int i = 0; i <= (bytes[7] + bytes[8] * 256) - 3; i += 2)
            {
                returnValue = bytes[11 + i] + bytes[12 + i] * 256;
                ReturnText = ReturnText + returnValue + Environment.NewLine;
            }
        }

        public int[] ReadWordWise(string device, int startAddress, int readLength)
        {
            Commandtext = "0104"; // Command for read
            Subcom = "0000"; //word wise transfer
            SetDeviceOpernad(device);

            byte[] bytes = SendReadCommand(startAddress, readLength);

            ReturnLenght = (bytes[7] + bytes[8] * 256) / 2 - 1;
            int[] returnValues = new int[ReturnLenght];
            ReturnText = "";
            for (int i = 0; i <= (bytes[7] + bytes[8] * 256) - 3; i += 2)
            {
                int low = bytes[11 + i];
                int high = bytes[12 + i] * 256;
                returnValues[i / 2] = low + high;
                ReturnText = ReturnText + returnValues[i / 2] + Environment.NewLine;
            }
            return returnValues;
        }

        public void WriteBitWise(string[] values, string device, int startAddress)
        {
            Commandtext = "0114"; // Command for write
            Subcom = "0100";
            SetDeviceOpernad(device);
            string Values = "";
            for (int i = 0; i < values.Length; i++)
            {
                Values = Values + Int32.Parse(values[i]);
            }

            byte[] bytes = SendWriteCommand(Values, startAddress, values.Length);
            ReturnLenght = ((bytes[7] + bytes[8] * 255) - 2) * 2;

            string bytesSub = BitConverter.ToString(bytes).Substring(0, 256);
            byte returnByte = 0;
            if (bytesSub.Substring(33, 1) == "1")
                returnByte |= 1;
            if (bytesSub.Substring(34, 1) == "1")
                returnByte |= 2;
            if (bytesSub.Substring(36, 1) == "1")
                returnByte |= 4;
            if (bytesSub.Substring(37, 1) == "1")
                returnByte |= 8;
            if (bytesSub.Substring(39, 1) == "1")
                returnByte |= 16;
            if (bytesSub.Substring(40, 1) == "1")
                returnByte |= 32;
            if (bytesSub.Substring(42, 1) == "1")
                returnByte |= 64;

            ReturnText = returnByte.ToString();
        }

        public byte ReadBitWise(string device, int startAddress, int readLength)
        {
            Commandtext = "0104"; // Command for read
            Subcom = "0100"; //word wise transfer
            SetDeviceOpernad(device);

            byte[] bytes = SendReadCommand(startAddress, readLength);

            string bytesSub = BitConverter.ToString(bytes).Substring(0, 256);
            byte returnByte = 0;
            if (bytesSub.Substring(33, 1) == "1")
                returnByte |= 1;
            if (bytesSub.Substring(34, 1) == "1")
                returnByte |= 2;
            if (bytesSub.Substring(36, 1) == "1")
                returnByte |= 4;
            if (bytesSub.Substring(37, 1) == "1")
                returnByte |= 8;
            if (bytesSub.Substring(39, 1) == "1")
                returnByte |= 16;
            if (bytesSub.Substring(40, 1) == "1")
                returnByte |= 32;
            if (bytesSub.Substring(42, 1) == "1")
                returnByte |= 64;

            ReturnText = returnByte.ToString();

            return returnByte;
        }

        #endregion CommandConstructors

        #region HelperFunc

        private void SetDeviceOpernad(string device)
        {
            switch (device)
            {
                case "X":
                    deviceOperand = "9C";
                    break;

                case "Y":
                    deviceOperand = "9D";
                    break;

                case "M":
                    deviceOperand = "90";
                    break;

                case "D":
                    deviceOperand = "A8";
                    break;

                case "R":
                    deviceOperand = "AF";
                    break;

                case "L":
                    deviceOperand = "92";
                    break;

                case "F":
                    deviceOperand = "93";
                    break;

                case "B":
                    deviceOperand = "A0";
                    break;

                case "SB":
                    deviceOperand = "A1";
                    break;

                case "SW":
                    deviceOperand = "B5";
                    break;

                case "T Contact":
                    deviceOperand = "C1";
                    break;

                case "T Coil":
                    deviceOperand = "C0";
                    break;

                case "T Value":
                    deviceOperand = "C2";
                    break;

                case "C Contact":
                    deviceOperand = "C4";
                    break;

                case "C Coil":
                    deviceOperand = "C3";
                    break;

                case "C Value":
                    deviceOperand = "C5";
                    break;

                case "ZR":
                    deviceOperand = "B0";
                    break;

                default:
                    throw new Exception("Device not recognized");
            }
        }

        private string HEX2(byte value)
        {
            string result = null;
            result = string.Format("{0,2:X}", System.Convert.ToString(value, 16));
            if (result.Substring(0, 1) == " ")
            {
                result = "0" + result.Substring(1, 1);
            }
            return result.ToUpper();
        }

        #endregion HelperFunc

        #region SendFunc

        private byte[] SendReadCommand(int startAddress, int readLength)
        {
            int commandlength1 = startAddress % 256;
            int deviceStart = (startAddress - commandlength1) / 256;
            int commandlength2 = deviceStart % 256;
            int commandlength3 = (deviceStart - commandlength2) / 65536;
            string Devicelength = HEX2((byte)commandlength1) + HEX2((byte)commandlength2) + HEX2((byte)commandlength3);

            // callculating of the data ammount
            commandlength1 = readLength % 256;
            commandlength2 = (readLength - commandlength1) / 256;
            string Datacount = HEX2((byte)commandlength1) + HEX2((byte)commandlength2);

            // callculating of the data request length has to be done with all callculated figures before
            string request = monitoringTimer + Commandtext + Subcom + Devicelength + deviceOperand + Datacount;
            int commandlength = request.Length / 2;
            commandlength1 = commandlength % 256;
            commandlength2 = (commandlength - commandlength1) / 256;
            string RequestedDataLength = HEX2((byte)commandlength1) + HEX2((byte)commandlength2);

            // Define the send string
            SendText = "5000" + networkNo + pcNo + RequDestIONo + RequDestStNo + RequestedDataLength + monitoringTimer + Commandtext + Subcom + Devicelength + deviceOperand + Datacount;

            int n = SendText.Length / 2;
            byte[] buffer = new byte[n];
            for (int i = 0; i <= SendText.Length - 1; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(SendText.Substring(i, 2), 16);
            }

            byte[] bytes = SendAndGetResponsePLC(buffer);

            if (bytes[9] + bytes[10] * 255 == 0)
            {
                return bytes;
            }
            else
            {
                throw new Exception("Communication NOT OK");
            }
        }

        private byte[] SendWriteCommand(string Values, int startAddress, int writeLenght)
        {
            //Callculating address Startaddress
            int commandlength1 = startAddress % 256;
            int deviceStart = (startAddress - commandlength1) / 256;
            int commandlength2 = deviceStart % 256;
            int commandlength3 = (deviceStart - commandlength2) / 65536;
            string Devicelength = HEX2((byte)commandlength1) + HEX2((byte)commandlength2) + HEX2((byte)commandlength3);

            // callculating of the data ammount
            commandlength1 = writeLenght % 256;
            commandlength2 = (writeLenght - commandlength1) / 256;
            string Datacount = HEX2((byte)commandlength1) + HEX2((byte)commandlength2);

            // callculating of the data request length has to be done with all callculated figures before
            string request = monitoringTimer + Commandtext + Subcom + Devicelength + deviceOperand + Datacount + Values;
            int commandlength = request.Length / 2;
            commandlength1 = commandlength % 256;
            commandlength2 = (commandlength - commandlength1) / 256;
            string RequestedDataLength = HEX2((byte)commandlength1) + HEX2((byte)commandlength2);

            // Define the send string
            SendText = "5000" + networkNo + pcNo + RequDestIONo + RequDestStNo + RequestedDataLength + monitoringTimer + Commandtext + Subcom + Devicelength + deviceOperand + Datacount + Values;

            int n = SendText.Length / 2;
            byte[] buffer = new byte[n];
            for (int i = 0; i <= SendText.Length - 1; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(SendText.Substring(i, 2), 16);
            }

            byte[] bytes = SendAndGetResponsePLC(buffer);

            if (bytes[9] + bytes[10] * 255 == 0)
            {
                return bytes;
            }
            else
            {
                throw new Exception("Communication NOT OK");
            }
        }

        #endregion SendFunc

        #region NetworkFunc

        public void TcpConnect(string ipAddress, int port)
        {
            IPEndPoint tcpIp = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            IsTcp = true;
            tcpClient = new TcpClient();
            tcpClient.Connect(tcpIp);
            networkStream = tcpClient.GetStream();
            TcpConnected = true;
        }

        public void TcpDisconnect()
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
                TcpConnected = false;
            }
        }

        public void UdpConnect(string ipAddress, int port)
        {
            IPEndPoint udpIp = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            IsTcp = false;
            udpClient = new UdpClient();
            udpEp = udpIp;
            udpClient.Connect(udpEp);
            UdpConnected = true;
        }

        public void UdpDisconnect()
        {
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
                UdpConnected = false;
            }
        }

        private byte[] SendAndGetResponsePLC(byte[] buffer)
        {
            Task<byte[]> task;
            try
            {
                if (IsTcp)
                {
                    task = Task.Run(() => TCPSendAndGetResponsePLC(buffer));
                    task.Wait(50);

                    if (task.IsCompleted != true)
                    {
                        TcpDisconnect();
                    }
                }
                else
                {
                    task = Task.Run(() => UDPSendAndGetResponsePLC(buffer));
                    task.Wait(50);

                    if (task.IsCompleted != true)
                    {
                        UdpDisconnect();
                    }
                }
                return task.Result;
            }
            catch (AggregateException)
            {
                throw;
            }
        }

        private async Task<byte[]> TCPSendAndGetResponsePLC(byte[] buffer)
        {
            networkStream.Write(buffer, 0, buffer.Length);

            byte[] bytes = new byte[tcpClient.ReceiveBufferSize + 1];
            await networkStream.ReadAsync(bytes, 0, tcpClient.ReceiveBufferSize);

            return bytes;
        }

        private async Task<byte[]> UDPSendAndGetResponsePLC(byte[] buffer)
        {
            // send data
            udpClient.Send(buffer, buffer.Length);

            // then receive data
            UdpReceiveResult task = await udpClient.ReceiveAsync();
            return task.Buffer;
        }

        #endregion NetworkFunc

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    TcpDisconnect();
                    UdpDisconnect();
                }

                disposed = true;
            }
        }
    }
}