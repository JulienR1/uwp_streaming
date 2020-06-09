using System;
using System.Net;
using WebcamPhotosStream.Pages;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace WebcamPhotosStream
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void StartServer(object sender, RoutedEventArgs e) => OpenNewWindow(typeof(pgServer));
        private void StartClient(object sender, RoutedEventArgs e) => OpenNewWindow(typeof(pgClient));        

        private void ToggleLaptop(object sender, RoutedEventArgs e) => App.ip = IPAddress.Parse("192.168.93.110");
        private void ToggleHolo(object sender, RoutedEventArgs e) => App.ip = IPAddress.Parse("192.168.93.116");
       
        private async void OpenNewWindow(Type pageToOpen)
        {
            AppWindow window = await AppWindow.TryCreateAsync();
            Frame frame = new Frame();
            frame.Navigate(pageToOpen);
            ElementCompositionPreview.SetAppWindowContent(window, frame);
            await window.TryShowAsync();

            window.Closed += delegate
            {
                frame.Content = null;
                window = null;
            };
        }
    }
}