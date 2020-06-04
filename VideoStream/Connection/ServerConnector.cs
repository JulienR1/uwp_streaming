using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VideoStream
{
    public class ServerConnector : Connector
    {
        private TcpListener server;

        Thread clientThread;
        List<ClientInfo> clients;

        bool serverIsUp = false;

        public void Connect(ConnectionType connectionType, int bufferSize)
        {
            base.Connect(connectionType);
            buffer = new byte[bufferSize];
            clients = new List<ClientInfo>();
            server = new TcpListener(App.serverIP, App.port);
            server.Start();
        }

        public override void Start()
        {
            serverIsUp = true;
            clientThread = new Thread(new ThreadStart(ReceiveClient));
            clientThread.Start();

            if (connectionType == ConnectionType.Read || connectionType == ConnectionType.Both)
                Read();
        }

        public override void Stop()
        {
            serverIsUp = false;
            throw new NotImplementedException();
        }

        private void ReceiveClient()
        {
            while (serverIsUp)
            {
                TcpClient currentClient = server.AcceptTcpClient();
                NetworkStream currentStream = currentClient.GetStream();
                if (connectionType == ConnectionType.Write)
                {
                    clients.Add(new ClientInfo { thread = null, client = currentClient, stream = currentStream });
                }
                else
                {
                    // Ajouter le thread correpondant au client qui fait la lecture et l'ecriture
                    clients.Add(new ClientInfo { thread = null, client = currentClient, stream = currentStream });
                }
            }
        }

        public override void Read()
        {
            while (serverIsUp)
            {
                foreach(ClientInfo client in clients)
                {
                    int i = 0;
                    while ((i = client.stream.Read(buffer, 0, buffer.Length)) != 0)
                    {

                    }
                }
            }
        }

        public override void Write<T>(T data)
        {
            throw new NotImplementedException();
        }

        public struct ClientInfo
        {
            public Thread thread;
            public TcpClient client;
            public Stream stream;
        }
    }
}
