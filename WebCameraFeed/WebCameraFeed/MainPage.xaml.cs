using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WebCameraFeed
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture captureManager;
        private bool isCapturingFrames;

        public MainPage()
        {
            this.InitializeComponent();
        }

        async private void InitCamera(object sender, RoutedEventArgs e)
        {
            captureManager = new MediaCapture();
            await captureManager.InitializeAsync();
        }

        async private void StartPreview(object sender, RoutedEventArgs e)
        {
            capturePreview.Source = captureManager;
            await captureManager.StartPreviewAsync();
        }

        private void StartPreviewFrame(object sender, RoutedEventArgs e)
        {
            isCapturingFrames = true;
            Thread t = new Thread(new ThreadStart(PreviewFrame));
            t.Start();            
        }
        
        private void StopPreviewFrame(object sender, RoutedEventArgs e)
        {
            isCapturingFrames = false;
            imagePreview.Source = null;
        }

        private async void PreviewFrame()
        {
            while (isCapturingFrames)
            {
                VideoEncodingProperties properties = captureManager.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
                VideoFrame videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)properties.Width, (int)properties.Height);
                // OVERFLOW problem
                VideoFrame frame = await captureManager.GetPreviewFrameAsync(videoFrame);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                 {
                     SoftwareBitmapSource bmpSource = new SoftwareBitmapSource();
                     await bmpSource.SetBitmapAsync(frame.SoftwareBitmap);
                     imagePreview.Source = bmpSource;
                 });                

                App.previewVideoFrames.Enqueue(frame.SoftwareBitmap);
                frame.Dispose();
                videoFrame.Dispose();
                Thread.Sleep(75);
            }
        }

        async private void StopPreview(object sender, RoutedEventArgs e)
        {
            StopPreviewFrame(sender, e);
            await captureManager.StopPreviewAsync();
        }

        private void OpenServer(object sender, RoutedEventArgs e) => OpenNewWindow(typeof(Server));
        private void OpenClient(object sender, RoutedEventArgs e) => OpenNewWindow(typeof(Client));

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
