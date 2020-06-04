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
        private TcpClient client;
        private Thread readingThread;

        public override void Connect(ConnectionType connectionType)
        {
            base.Connect(connectionType);
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

        public override void Start()
        {
            if (connectionType == ConnectionType.Read || connectionType == ConnectionType.Both)
            {
                readingThread = new Thread(new ThreadStart(Read));
                readingThread.Start();
            }
        }

        public override void Stop()
        {
            readingThread.Join();
            connection.Close();
            client.Close();
        }

        public override void Read()
        {
            throw new NotImplementedException();
        }

        public override void Write<String>(String text)
        {
            byte[] msg = Parser.EncodeText(text.ToString());
            byte[] msgByteCount = BitConverter.GetBytes(msg.Length);
            byte[] connectionTypeBytes = BitConverter.GetBytes((int)DataType.Text);

            connection.Write(msgByteCount, 0, msgByteCount.Length);
            connection.Write(connectionTypeBytes, 0, connectionTypeBytes.Length);
            connection.Write(msg, 0, msg.Length);
        }
    }
}
