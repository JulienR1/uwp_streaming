using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Perception.Spatial;
using Windows.Security.Cryptography.Core;

namespace Multiclient.Communication
{
    public class Server : Communicator
    {
        private TcpListener listener;
        private bool serverIsActive = false;

        private Dictionary<int, ClientData> allClients = new Dictionary<int, ClientData>();
        private int currentClientNo;

        public Server(Action<object> callback) : base(callback) { }

        public void StartServer()
        {
            listener = new TcpListener(App.ip, App.port);
            listener.Start();
            serverIsActive = true;
            new Thread(new ThreadStart(LookForConnection)).Start();
        }

        public void StopServer()
        {
            serverIsActive = false;
            listener.Stop();
            listener = null;
        }

        public void WriteToClient(object message, int targetClientID)
        {
            WriteWithHeader(allClients[targetClientID].stream, Encoding.ASCII.GetBytes((string)message));
        }

        private void LookForConnection()
        {
            while (serverIsActive)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    ExecuteClient(stream);
                }
                catch (SocketException) { return; }
            }
        }

        private void ExecuteClient(object obj)
        {
            NetworkStream stream = (NetworkStream)obj;
            CommunicationState currentCommunicationState = (CommunicationState)ReadIntFromStream(stream);

            byte[] idBytes = ReadBytesWithHeader(stream);
            string clientId = Encoding.ASCII.GetString(idBytes);
            
            ClientData data = new ClientData() { stream = stream, clientID = clientId, clientNo = currentClientNo };
            allClients.Add(currentClientNo++, data);

            StartCommunication(currentCommunicationState, data);
        }

        protected override void ReadData(object _data)
        {
            ClientData data = (ClientData)_data;
            while (serverIsActive)
            {
                byte[] message = ReadBytesWithHeader(data.stream);

                string entryMessage = Encoding.ASCII.GetString(message);
                callback($"Client {data.clientID}-{data.clientNo}: {entryMessage}");
            }
        }

        protected override void WriteData(object data)
        {
           // throw new NotImplementedException();
        }

        public struct ClientData {
            public NetworkStream stream;
            public string clientID;
            public int clientNo;
        }
    }
}