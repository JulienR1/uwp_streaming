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
    public class ClientConnector : Connector
    {
        private ConnectionType connectionType;
        private TcpClient client;

        private Thread readingThread;

        protected override void Connect(ConnectionType connectionType)
        {
            this.connectionType = connectionType;
            try
            {
                client = new TcpClient(App.serverIP.ToString(), App.port);
                connection = client.GetStream();
            }
            catch (SocketException)
            {
                client = null;
                Stop();
                return;
            }
        }

        protected override void Start()
        {
            if (connectionType == ConnectionType.Read || connectionType == ConnectionType.Both)
            {
                readingThread = new Thread(new ThreadStart(Read));
            }
        }

        protected override void Stop()
        {
            connection.Close();
            client.Close();
        }

        protected override void Read()
        {
            throw new NotImplementedException();
        }

        protected override void Write()
        {
            throw new NotImplementedException();
        }
    }
}
