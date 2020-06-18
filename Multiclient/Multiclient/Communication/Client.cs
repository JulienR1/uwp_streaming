using Multiclient.VideoFeed;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.ClosedCaptioning;

namespace Multiclient.Communication
{
    public class Client : Communicator
    {
        private NetworkStream stream;
        private string clientID;

        public Client(Action<Object> callback) : base(callback) { }

        public async void StartClient(CommunicationState clientCommunicationState)
        {
            Connect();
            SendServerCommunicationState(clientCommunicationState);
            SendClientID();
            StartCommunication(clientCommunicationState, null);
            if (clientCommunicationState != CommunicationState.Reading)
                await Webcam.Start();
        }

        public void CloseClient()
        {
            inCommunication = false;
            stream?.Dispose();
            stream = null;
            Webcam.Stop();
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
            CommunicationState serverCommunicationState = CommunicationState.Reading;
            if (clientCommunicationState == CommunicationState.Both)
                serverCommunicationState = CommunicationState.Both;
            else if (clientCommunicationState == CommunicationState.Writing)
                serverCommunicationState = CommunicationState.Reading;
            else if (clientCommunicationState == CommunicationState.Reading)
                serverCommunicationState = CommunicationState.Writing;                

            stream.Write(BitConverter.GetBytes((int)serverCommunicationState), 0, 4);
        }      
        
        // The client ID is its MAC address.
        // The ID is not the same thing as the client number.
        // The client number is only a simple way to select a particular device.
        private void SendClientID()
        {
            clientID = (from nic in NetworkInterface.GetAllNetworkInterfaces()
                        where nic.OperationalStatus == OperationalStatus.Up
                        select nic.GetPhysicalAddress().ToString()).FirstOrDefault();

            WriteWithHeader(stream, Encoding.ASCII.GetBytes(clientID));
        }

        protected async override void ReadData(object data)
        {
            byte[] imgBytes = ReadBytesWithHeader(stream);
            SoftwareBitmap bmp = await BitmapHelper.EncodedBytesToBitmapAsync(imgBytes);
            callback(bmp);
        }

        protected async override void WriteData(object data)
        {
            SoftwareBitmap bmp = Webcam.CurrentFrame;
            if (bmp == null)
                return;
            
            byte[] imgBytes = await BitmapHelper.BitmapToEncodedBytesAsync(bmp);           

            WriteWithHeader(stream, imgBytes);
        }
    }
}