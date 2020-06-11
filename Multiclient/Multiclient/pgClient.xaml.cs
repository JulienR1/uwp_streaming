using Multiclient.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Multiclient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pgClient : Page
    {
        private Client client = null;
        private vmClient vm;

        private CommunicationState communicationState;

        public class vmClient : BindableBase
        {
            private Visibility inputVisible, reconnectVisible;

            public Visibility InputVisible { get { return inputVisible; } set { SetProperty(ref inputVisible, value); } }
            public Visibility ReconnectVisible { get { return reconnectVisible; } set { SetProperty(ref reconnectVisible, value); } }

            public void UpdateVisibility(object client)
            {
                InputVisible = client == null ? Visibility.Collapsed : Visibility.Visible;
                ReconnectVisible = client == null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public pgClient()
        {
            this.InitializeComponent();
            this.DataContext = vm = new vmClient();
            Window.Current.Closed += delegate
            {
                if (client != null)
                    client.CloseClient();
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is CommunicationState)
            {
                this.communicationState = (CommunicationState)e.Parameter;
            }
            base.OnNavigatedTo(e);
        }

        private void OnLoadComplete(object sender, RoutedEventArgs e) => CreateClient();
        private void Reconnect(object sender, RoutedEventArgs e) => CreateClient();
        private void inputKeyDown(object sender, KeyRoutedEventArgs e) { if (e.Key == VirtualKey.Enter) SendMessage(); }

        private void CreateClient()
        {
            if (client == null)
            {
                client = new Client(OnDataReceived);
                client.StartClient(communicationState);
            }
            vm.UpdateVisibility(client);
        }

        private void SendMessage()
        {
            client.SendData(inputField.Text);
            inputField.Text = "";
        }

        private async void OnDataReceived(object data)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (data.GetType() == typeof(bool) && (bool)data == false)
                {
                    outputField.Text += "Impossible de se connecter: le serveur est-il actif?";
                    client = null;
                    return;
                }
                else if (data.GetType() == typeof(string))
                {
                    outputField.Text += (string)data;
                    return;
                }
            });
        }
    }
}