using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.ClosedCaptioning;

namespace Multiclient.Communication
{
    public class Client : Communicator
    {
        private NetworkStream stream;
        private string clientID;

        private Queue<string> messagesToSend;

        public Client(Action<Object> callback) : base(callback) { }

        public void StartClient(CommunicationState clientCommunicationState)
        {
            messagesToSend = new Queue<string>();
            Connect();
            SendServerCommunicationState(clientCommunicationState);
            SendClientID();
            StartCommunication(clientCommunicationState, null);
        }

        public void CloseClient()
        {
            stream?.Dispose();
            stream = null;
        }

        private void Connect()
        {
            try
            {
                TcpClient client = new TcpClient(App.ip.ToString(), App.port);
                stream = client.GetStream();
            }
            catch (SocketException)
            {
                CloseClient();
                callback(false);
                return;
            }
        }

        private void SendServerCommunicationState(CommunicationState clientCommunicationState)
        {
            CommunicationState serverCommunicationState;
            if (clientCommunicationState == CommunicationState.Both)
                serverCommunicationState = CommunicationState.Both;
            else
                serverCommunicationState = (CommunicationState)(1 - (int)clientCommunicationState);

            stream.Write(BitConverter.GetBytes((int)serverCommunicationState), 0, 4);
        }      
        
        private void SendClientID()
        {
            clientID = (from nic in NetworkInterface.GetAllNetworkInterfaces()
                        where nic.OperationalStatus == OperationalStatus.Up
                        select nic.GetPhysicalAddress().ToString()).FirstOrDefault();

            WriteWithHeader(stream, Encoding.ASCII.GetBytes(clientID));
        }

        public void SendData(string data)
        {
            messagesToSend.Enqueue(data);
        }

        protected override void ReadData(object data)
        {
            byte[] messageBytes = ReadBytesWithHeader(stream);
            callback("Serveur: " + Encoding.ASCII.GetString(messageBytes));
        }

        protected override void WriteData(object data)
        {
            if (messagesToSend == null || messagesToSend.Count == 0)
                return;

            string message = messagesToSend.Dequeue();
            WriteWithHeader(stream, Encoding.ASCII.GetBytes(message));
        }
    }
}