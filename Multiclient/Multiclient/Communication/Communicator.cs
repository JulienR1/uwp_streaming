using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Multiclient.Communication
{
    public abstract class Communicator
    {
        protected Action<object> callback;

        public Communicator(Action<object> callback)
        {
            this.callback = callback;
        }

        protected void StartCommunication(CommunicationState communicationState, object data)
        {
            if (communicationState == CommunicationState.Reading || communicationState == CommunicationState.Both)
            {
                Process process = new Process() { process = new Action<object>(ReadData), data = data };
                new Thread(new ParameterizedThreadStart(BeginProcess)).Start(process);
            }
            if (communicationState == CommunicationState.Writing || communicationState == CommunicationState.Both)
            {
                Process process = new Process() { process = new Action<object>(WriteData), data = data };
                new Thread(new ParameterizedThreadStart(BeginProcess)).Start(process);
            }
        }

        private void BeginProcess(object _processData)
        {
            Process processData = (Process)_processData;
            while (true)
            {
                try
                {
                    processData.process.Invoke(processData.data);
                }
                catch (SocketException) { return; }
            }
        }

        protected abstract void ReadData(object data);
        protected abstract void WriteData(object data);

        protected int ReadIntFromStream(NetworkStream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, buffer.Length);
            return BitConverter.ToInt32(buffer, 0);
        }

        protected void WriteWithHeader(NetworkStream stream, byte[] data)
        {
            byte[] dataBytesCount = BitConverter.GetBytes(data.Length);
            byte[] dataWithHeader = new byte[data.Length + dataBytesCount.Length];
            dataBytesCount.CopyTo(dataWithHeader, 0);
            data.CopyTo(dataWithHeader, dataBytesCount.Length);

            stream.Write(dataWithHeader, 0, dataWithHeader.Length);
        }

        protected byte[] ReadBytesWithHeader(NetworkStream stream)
        {
            int byteCount = ReadIntFromStream(stream);
            byte[] buffer = new byte[byteCount];

            int totalBytesRead = 0;
            while (totalBytesRead < byteCount)
            {
                int bytesRead = stream.Read(buffer, totalBytesRead, byteCount - totalBytesRead);
                totalBytesRead += bytesRead;
            }
            return buffer;
        }

        private struct Process
        {
            public Action<object> process;
            public object data;
        }
    }
}
