using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
    public sealed partial class SocketClient : Page
    {
        public SocketClient()
        {
            this.InitializeComponent();

            Thread t = new Thread(new ThreadStart(Connect));
            t.Start();
        }

        private void Connect()
        {
            print("HostName: " + Dns.GetHostName());

            string ip = "127.0.0.1";
            int port = 5050;

            string request = "GET / HTTP/1.1\r\nHost: " + ip + "\r\nConnection: Close\r\n\r\n";
            Byte[] bytesSent = Encoding.ASCII.GetBytes("Hello world!");

            print("Connecting");
            using (Socket s = GetSocket(ip, port))
            {
                if (s == null)
                {
                    print("Connection failed");
                    return;
                }

                print("Data sending..");
                s.Send(bytesSent, bytesSent.Length, 0);
                print("Data sent");
            }
        }

        private Socket GetSocket(string ip, int port)
        {
            Socket s = null;
            IPHostEntry hostEntry = null;

            hostEntry = Dns.GetHostEntry(ip);

            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, port);
                Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                tempSocket.Connect(ipe);

                if (tempSocket.Connected)
                {
                    s = tempSocket;
                    break;
                }
                else
                {
                    continue;
                }
            }
            return s;
        }

        private async void print(string s)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                feedback.Text = feedback.Text + Environment.NewLine + s;
            });
        }
    }
}