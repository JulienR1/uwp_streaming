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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoStream
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Client : Page
    {
        private string ip = "";

        public Client()
        {
            this.InitializeComponent();            
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

                Byte[] data = System.Text.Encoding.ASCII.GetBytes("Hello world");

                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                print("Sent message");

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