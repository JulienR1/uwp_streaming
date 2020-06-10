using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace WebcamPhotosStream
{
    public class Client
    {
        private Webcam camera;

        private TcpClient client;
        private NetworkStream stream;

        public Client(CoreDispatcher dispatcher)
        {
            StartAsync(dispatcher);
        }

        private async void StartAsync(CoreDispatcher dispatcher)
        {
            camera = new Webcam();
            await camera.Start(dispatcher, 30);

            client = new TcpClient(App.ip.ToString(), App.port);
            stream = client.GetStream();
            SendGeneralInfos();
            ReceiveConfirmation();
        }

        private void SendGeneralInfos()
        {
            byte[] imgWidth = BitConverter.GetBytes(camera.Width);
            byte[] imgHeight = BitConverter.GetBytes(camera.Height);
            
            stream.Write(imgWidth, 0, imgWidth.Length);
            stream.Write(imgHeight, 0, imgHeight.Length);
        }
        
        private void ReceiveConfirmation()
        {
            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, 1);
            bool confirmationStatus = BitConverter.ToBoolean(buffer, 0);
            if (confirmationStatus)
            {
                new Thread(new ThreadStart(SendImages)).Start();
            }
            else
            {
                Debug.WriteLine("Could not connect properly to server.");
                return;
            }
        }

        private async void SendImages()
        {
            while (true)
            {                
                byte[] img = await camera.GetFrameBytesAsync();                
                if (img != null)
                {                    
                    byte[] imgSize = BitConverter.GetBytes(img.Length);
                    byte[] imgWithHeader = new byte[img.Length + imgSize.Length];

                //    Debug.WriteLine("length sent: " + img.Length + "(" + imgSize.Length + ")");

                    imgSize.CopyTo(imgWithHeader, 0);
                    img.CopyTo(imgWithHeader, imgSize.Length);
                    //stream.Write(img, 0, img.Length);
                    stream.Write(imgWithHeader, 0, imgWithHeader.Length);
                 //   Debug.WriteLine("Sent image | len: " + img.Length);
                }
            //    Thread.Sleep(30);
            }
        }
    }
}
