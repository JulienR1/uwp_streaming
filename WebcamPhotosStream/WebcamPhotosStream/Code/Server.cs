using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebcamPhotosStream.Pages;
using Windows.Devices.Printers;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace WebcamPhotosStream
{
    public class Server
    {
        private TcpListener listener;
        private TcpClient client;
        private NetworkStream stream;

        public Server()
        {
            listener = new TcpListener(App.ip, App.port);
            listener.Start();
            new Thread(new ThreadStart(ConnectToClient)).Start();
        }

        private void ConnectToClient()
        {
            client = listener.AcceptTcpClient();
            stream = client.GetStream();

            GeneralInfos infos = ReadGeneralInfos();
            SendConfirmation();
            ReadVideo(infos);
        }

        private GeneralInfos ReadGeneralInfos()
        {
            GeneralInfos infos = new GeneralInfos();
            infos.imgWidth = ReadInt();
            infos.imgHeight = ReadInt();
            return infos;
        }

        private void SendConfirmation()
        {
            stream.Write(BitConverter.GetBytes(true), 0, 1);            
        }

        private async void ReadVideo(GeneralInfos infos)
        {
            while (true)
            {
                int imgSize = ReadInt();                

                byte[] buffer = new byte[imgSize];
                //byte[] buffer = new byte[76800];
                //byte[] buffer = new byte[infos.imgSize];
                /*    while (imgBytes.Count < infos.imgSize)
                    {
                        int qtyToRead = Math.Min(infos.imgSize - imgBytes.Count, buffer.Length);
                        stream.Read(buffer, 0, qtyToRead);

                        byte[] pertinentData = new byte[qtyToRead];
                        Array.Copy(buffer, pertinentData, qtyToRead);
                        imgBytes.InsertRange(imgBytes.Count, pertinentData);
                  //      Thread.Sleep(16);
                    }*/
                //stream.Read(buffer, 0, infos.imgSize);
                List<byte> imgBytes = new List<byte>();
                int bytesRead = 0;
                int amountToRead = 0;
                while (bytesRead < imgSize)
                {
                    int insertionIndex = bytesRead;
                    amountToRead = Math.Min(buffer.Length, imgSize - bytesRead);
                    bytesRead += stream.Read(buffer, 0, amountToRead);                    
                    imgBytes.InsertRange(insertionIndex, buffer);
                    imgBytes.RemoveRange(bytesRead, imgBytes.Count - bytesRead);
                }
                imgBytes.RemoveRange(imgSize, imgBytes.Count - imgSize);

                SoftwareBitmap receivedBmp = null;// = new SoftwareBitmap(BitmapPixelFormat.Bgra8, infos.imgWidth, infos.imgHeight, BitmapAlphaMode.Premultiplied);
         //       receivedBmp.CopyFromBuffer(imgBytes.ToArray().AsBuffer());
                //receivedBmp.CopyFromBuffer(buffer.AsBuffer());

                using (IRandomAccessStream ms = imgBytes.ToArray().AsBuffer().AsStream().AsRandomAccessStream())
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.JpegDecoderId, ms);
                    receivedBmp = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                pgServer.instance.SetImage(receivedBmp);

                Debug.WriteLine("Image received | len: " + infos.imgSize + " | " + buffer.Length + " | " + infos.imgWidth + " | " + infos.imgHeight);
            }            
        }

        private int ReadInt()
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        private struct GeneralInfos
        {
            public int imgWidth;
            public int imgHeight;

            public int imgSize { get { return 4 * imgWidth * imgHeight; } }
        }
    }
}