using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoStream
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Server : Page
    {
        TcpListener server = null;

        string ipStr = "";

        public Server()
        {
            this.InitializeComponent();            
        }

        private async void StartServer()
        {
            try
            {
                Int32 port = 5050;
                GetIp();
                IPAddress ip = IPAddress.Parse(ipStr);

                print("Server initialization on " + ip.ToString() + ":" + port);
                server = new TcpListener(ip, port);
                server.Start();

                Byte[] buffer = new Byte[256];

                // Listen
                while (true)
                {
                    print("Waiting for connection");
                    TcpClient client = server.AcceptTcpClient();
                    print("Connected!");

                    NetworkStream stream = client.GetStream();

                    int i;
                    //   byte[] byteCountBuffer = new byte[64];
                    //  byte[] receivedImgByteCount = new byte[stream.Read(byteCountBuffer, 0, 4)];
                    //                    int receivedImgBytes = BitConverter.ToInt32(receivedImgByteCount, 0);

                    //     byte[] receivedImg = new byte[receivedImgBytes];
                    //                  int currentIndex = 0;

                    List<byte> img = new List<byte>();
                    int currentIndex = 0;

                    while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        img.InsertRange(currentIndex, buffer);
                        currentIndex += i;
                    }

                    using (InMemoryRandomAccessStream imgStream = new InMemoryRandomAccessStream())
                    {
                        using(DataWriter writer = new DataWriter(imgStream.GetOutputStreamAt(0)))
                        {
                            writer.WriteBytes(img.ToArray());
                            await writer.StoreAsync();
                        }
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                        {
                            BitmapImage bmpImg = new BitmapImage();
                            await bmpImg.SetSourceAsync(imgStream);
                            imgRender.Source = bmpImg;
                        });                        
                    }
                }
            }
            catch (SocketException e)
            {
                print(string.Format("Socket Exception: {0}", e));
            }
            finally
            {
                server.Stop();
            }
            print("Server closed");
        }       

        private async void GetIp()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { ipStr = ipAddress.Text; });
        }

        private async void print(string s)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                feedbackText.Text = feedbackText.Text + Environment.NewLine + s;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(StartServer));
            thread.Start();

        }
    }
}