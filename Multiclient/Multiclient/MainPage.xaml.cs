﻿using Multiclient.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.Appointments.DataProvider;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Multiclient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Server server;

        public MainPage()
        {
            this.InitializeComponent();

            cameraStuff();

            try
            {
                server = new Server(OnReceivedData);
                server.StartServer();
                Window.Current.Closed += delegate
                {
                    if (server != null)
                        server.StopServer();
                };
            }
            catch (SocketException) { outputField.Text = "You are not a server"; }            
        }

        #region INUTILE
        private async void cameraStuff()
        {
            await Webcam.Start();
            new Thread(new ThreadStart(async () =>
            {
                while (true)
                {
                    SoftwareBitmap newFrame = Webcam.CurrentFrame;
                    if (newFrame != null)
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                        {
                            SoftwareBitmapSource src = new SoftwareBitmapSource();
                            await src.SetBitmapAsync(newFrame);
                            videoPreview.Source = src;
                        });
                    }
                }
            })).Start();
        }
        #endregion

        private void ClientClick(object sender, RoutedEventArgs e) => App.OpenNewWindow(typeof(pgClient), (CommunicationState)clientState.SelectedIndex);
        private void inputKeyDown(object sender, KeyRoutedEventArgs e) { if (e.Key == VirtualKey.Enter) SendMessage(); }

        private void SendMessage()
        { 
            string message = inputField.Text;
            string clientIdStr = clientIdInputField.Text;

            int targetClientId;
            if(!int.TryParse(clientIdStr, out targetClientId))
            {
                outputField.Text += "Select a client to communicate with";
                return;
            }

            inputField.Text = "";
            clientIdInputField.Text = "";

            server.WriteToClient(message, targetClientId);
        }

        public async void OnReceivedData(object data)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                string dataStr = (string)data;
                outputField.Text += dataStr + Environment.NewLine;
            });
        }
    }
}