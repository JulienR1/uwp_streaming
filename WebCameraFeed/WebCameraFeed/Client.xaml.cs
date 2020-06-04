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
    public sealed partial class Client : Page
    {
        TcpClient client;
        NetworkStream stream;

        public Client()
        {
            this.InitializeComponent();
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            client = new TcpClient(App.ip.ToString(), App.port);
            stream = client.GetStream();            

            SendImage();
       //     Thread thread = new Thread(new ThreadStart(SendImage));
       //     thread.Start();
        }

        private async void SendImage()
        {
            SoftwareBitmap bmp = App.previewVideoFrames.Dequeue();
            WriteableBitmap bmpBuffer = new WriteableBitmap(bmp.PixelWidth, bmp.PixelHeight);
            bmp.CopyToBuffer(bmpBuffer.PixelBuffer);

            await imagePreview.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => {
                SoftwareBitmapSource src = new SoftwareBitmapSource();
                await src.SetBitmapAsync(bmp);
                imagePreview.Source = src;
            });

            byte[] bytes = bmpBuffer.PixelBuffer.ToArray();
            byte[] widthBytes = BitConverter.GetBytes(bmp.PixelWidth);
            byte[] heightBytes = BitConverter.GetBytes(bmp.PixelHeight);
            byte[] imgSizeBytes = BitConverter.GetBytes(bytes.Length);

            stream.Write(imgSizeBytes, 0, imgSizeBytes.Length);
            stream.Write(widthBytes, 0, widthBytes.Length);
            stream.Write(heightBytes, 0, heightBytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
