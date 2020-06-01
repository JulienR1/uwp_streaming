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
    public sealed partial class Server : Page
    {
        TcpListener server = null;

        string ipStr = "";

        public Server()
        {
            this.InitializeComponent();            
        }

        private void StartServer()
        {
            try
            {
                Int32 port = 5050;
                GetIp();
                IPAddress ip = IPAddress.Parse(ipStr);

                print("Server initialization on " + ip.ToString() + ":" + port);
                server = new TcpListener(ip, port);
                server.Start();

                Byte[] bytes = new Byte[256];
                string data = null;

                // Listen
                while (true)
                {
                    print("Waiting for connection");
                    //Socket client = server.AcceptSocket(); //server.AcceptTcpClient();
                    TcpClient client = server.AcceptTcpClient();
                    print("Connected!");

                    data = null;

                    NetworkStream stream = client.GetStream();// new NetworkStream(client);// client.GetStream();

                    int i;
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        print("Received: " + data);
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