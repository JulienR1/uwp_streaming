using Multiclient.Communication;
using Multiclient.VideoFeed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments.DataProvider;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.System;
using Windows.UI;
using Windows.UI.WindowManagement;
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
        public static Dictionary<UIContext, AppWindow> AppWindows { get; set; } = new Dictionary<UIContext, AppWindow>();
        public static Dictionary<string, Action<object, string>> videoFeeds = new Dictionary<string, Action<object, string>>();
        
        private Server server;

        private static MainPage instance;

        public MainPage()
        {
            this.InitializeComponent();
            instance = this;

            try
            {
                server = new Server(OnReceivedData, OpenVisualizationPage);
                server.StartServer();
                Window.Current.Closed += delegate
                {
                    if (server != null)
                        server.StopServer();
                };
            }
            catch (SocketException) { outputField.Text = "You are not a server"; }
        }

        private void ClientClick(object sender, RoutedEventArgs e) => App.OpenNewWindow(typeof(pgClient), (CommunicationState)clientState.SelectedIndex);
        private void inputKeyDown(object sender, KeyRoutedEventArgs e) { if (e.Key == VirtualKey.Enter) SendMessage(); }

        public static async Task StartWebcam()
        {
            await MainPage.instance.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
             {
                 await Webcam.Start();
             });
        }

        private void SendMessage()
        {
            string message = inputField.Text;
            string clientIdStr = clientIdInputField.Text;

            int targetClientId;
            if (!int.TryParse(clientIdStr, out targetClientId))
            {
                outputField.Text += "Select a client to communicate with";
                return;
            }

            inputField.Text = "";
            clientIdInputField.Text = "";

            server.WriteToClient(message, targetClientId);
        }

        private async Task OpenVisualizationPage(string clientID)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await App.OpenNewWindow(typeof(VideoFeedVisualization), clientID);
            });            
        }

        public async void OnReceivedData(object data)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (data.GetType() == typeof(string))
                {
                    string dataStr = (string)data;
                    outputField.Text += dataStr + Environment.NewLine;
                }
                else if (data.GetType() == typeof(SoftwareBitmap))
                {
                    SoftwareBitmapSource src = new SoftwareBitmapSource();
                    await src.SetBitmapAsync((SoftwareBitmap)data);
                    receptionImage.Source = src;
                }
            });
        }
    }
}