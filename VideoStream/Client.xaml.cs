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
using Windows.Media.Playback;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using System.Security.Cryptography;
using Windows.Storage.Streams;
using System.Drawing;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoStream
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Client : Page
    {
        private string ip = "";

        byte[] bytes;
        SoftwareBitmap bmp;

        public static Queue<SoftwareBitmapSource> bmpToSend = new Queue<SoftwareBitmapSource>();

        public Client()
        {
            this.InitializeComponent();


  //          WriteableBitmap wbmp = new WriteableBitmap(bmp.PixelWidth, bmp.PixelHeight);
  //          bmp.CopyToBuffer(wbmp.PixelBuffer);

            //imgRender.Source = wbmp;

            //bytes = wbmp.PixelBuffer.ToArray();

           // print(bytes.Length.ToString());
        }

        async void go()
        {
           // await bmpToSend.Dequeue().SetBitmapAsync(bmp);
          //  bytes = null;

            //print(bmpToSend.Count.ToString());
           // print(bmp.PixelWidth + " x " + bmp.PixelHeight);
           // print(bmp.ToString());
        }

    private void StartClient()
        {
            try
            {
                GetIp();
                print("Attempting to connect");
                Int32 port = 5050;
                TcpClient client = new TcpClient();
                client.Connect(ip, port);
                NetworkStream stream = client.GetStream();

                Thread.Sleep(100000);
                
                /*    using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
                    {
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.JpegDecoderId, ms);

                        BitmapTransform transform = new BitmapTransform()
                        {
                            ScaledWidth = Convert.ToUInt32(bmp.PixelWidth),
                            ScaledHeight = Convert.ToUInt32(bmp.PixelHeight)
                        };

                        PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Straight,
                            transform,
                            ExifOrientationMode.IgnoreExifOrientation,
                            ColorManagementMode.DoNotColorManage);

                        bytes = pixelData.DetachPixelData();
                        stream.Write(bytes, 0, bytes.Length);
                    }*/



                //  stream.Write(bytes, 0, bytes.Length);

                /*      print("Encoding message");
                      Byte[] data = System.Text.Encoding.ASCII.GetBytes("Hello world");

                      NetworkStream stream = client.GetStream();
                      stream.Write(data, 0, data.Length);
                      print("Sent message");*/

                //     print("Encoding image");
                /*  FileStream imgStream = new FileStream("D:/imgToSend.jpg", FileMode.Open, FileAccess.Read);
                  MemoryStream ms = new MemoryStream();
                  ms.SetLength(imgStream.Length);
                  imgStream.Read(ms.GetBuffer(), 0, (int)imgStream.Length);

                  ms.Flush();
                  imgStream.Close();
                  ms.Close();

                  byte[] imgBytes = ms.ToArray();
                  ms.Flush();
                  imgStream.Close();    */

                /*     StorageFolder installationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                     StorageFile file = await installationFolder.GetFileAsync(@"Assets\imgToSend.jpg");
                     byte[] imgBytes = File.Exists(file.Path) ? File.ReadAllBytes(file.Path) : null;

                     print("Sending image");
                   //  stream.Write(BitConverter.GetBytes(imgBytes.Length), 0, 4);
                     stream.Write(imgBytes, 0, imgBytes.Length);
                     print("Image sent");*/


                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                print("ArgumentNullException: " + e);
            }
            catch (SocketException e)
            {
                print("Socket Exception: " + e);
            }
        }

        private async void print(string s)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                feedbackText.Text = feedbackText.Text + Environment.NewLine + s;
            });
        }

        private async void GetIp()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { ip = ipAddress.Text; });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(StartClient));
            thread.Start();
        }
    }
}