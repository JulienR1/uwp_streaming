using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Services.Maps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace WebCameraFeed
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class Server : Page
    {
        TcpListener server;
        NetworkStream stream;
        byte[] buffer;

        public Server()
        {
            this.InitializeComponent();
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(new ThreadStart(sendData));
            t.Start();
        }

        private async void sendData()
        {
            server = new TcpListener(App.ip, App.port);
            server.Start();
            buffer = new byte[1024];

            TcpClient client = server.AcceptTcpClient();
            stream = client.GetStream();

            List<byte> imgBytes = new List<byte>();
            int imgSize = 0;
            int imgWidth = 0, imgHeight = 0;
            while (true)
            {
                if (imgSize == 0)
                {
                    stream.Read(buffer, 0, 4);
                    stream.Read(buffer, 4, 4);
                    stream.Read(buffer, 8, 4);
                    imgSize = BitConverter.ToInt32(buffer, 0);
                    imgWidth = BitConverter.ToInt32(buffer, 4);
                    imgHeight = BitConverter.ToInt32(buffer, 8);                    
                    buffer = new byte[1024];
                    imgBytes = new List<byte>();
                }

                while (imgSize != imgBytes.Count)
                {
                    stream.Read(buffer, 0, Math.Min(imgSize - imgBytes.Count, buffer.Length));
                    imgBytes.InsertRange(imgBytes.Count, buffer);
                }
                imgSize = 0;

                SoftwareBitmap bmp = new SoftwareBitmap(BitmapPixelFormat.Bgra8, imgWidth, imgHeight, BitmapAlphaMode.Premultiplied);
                bmp.CopyFromBuffer(imgBytes.ToArray().AsBuffer());

                await imagePreview.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    SoftwareBitmapSource imgSrc = new SoftwareBitmapSource();
                    await imgSrc.SetBitmapAsync(bmp);
                    imagePreview.Source = imgSrc;
                });
            }
        }
    }
}