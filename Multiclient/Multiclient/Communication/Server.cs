using Multiclient.VideoFeed;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
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

        private Func<string, Task> OpenVisualizationPage;

        public Server(Action<object> callback, Func<string, Task> OpenVisualizationPage) : base(callback)
        {
            this.OpenVisualizationPage = OpenVisualizationPage;
        }

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
            inCommunication = false;
            //TODO: close all other threads that were opened in between clients and server
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

        private async void ExecuteClient(object obj)
        {
            NetworkStream stream = (NetworkStream)obj;
            CommunicationState currentCommunicationState = (CommunicationState)ReadIntFromStream(stream);

            byte[] idBytes = ReadBytesWithHeader(stream);
            string clientId = Encoding.ASCII.GetString(idBytes);

            if (currentCommunicationState != CommunicationState.Writing)
                await OpenVisualizationPage(clientId);
            ClientData data = new ClientData()
            {
                stream = stream,
                clientID = clientId,
                clientNo = currentClientNo
            };
            allClients.Add(currentClientNo, data);
            currentClientNo++;

            if (currentCommunicationState == CommunicationState.Writing)
                await MainPage.StartWebcam();

            StartCommunication(currentCommunicationState, data);
        }

        protected async override void ReadData(object _data)
        {
            ClientData data = (ClientData)_data;
            byte[] message = ReadBytesWithHeader(data.stream);

            if (message != null && message.Length != 0)
            {
                SoftwareBitmap receivedBmp = await BitmapHelper.EncodedBytesToBitmapAsync(message);
                if (MainPage.videoFeeds.ContainsKey(data.clientID))
                {
                    MainPage.videoFeeds[data.clientID]?.Invoke(receivedBmp, data.clientID);
                }
            }
        }

        protected async override void WriteData(object data)
        {
            SoftwareBitmap bmp = Webcam.CurrentFrame;
            if (bmp == null)
                return;

            byte[] imgBytes = await BitmapHelper.BitmapToEncodedBytesAsync(bmp);
            ClientData cData = (ClientData)data;
            WriteWithHeader(cData.stream, imgBytes);
        }

        public struct ClientData
        {
            public NetworkStream stream;
            public string clientID;
            public string title;
            public int clientNo;
        }
    }
}