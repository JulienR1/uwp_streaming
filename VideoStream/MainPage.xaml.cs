using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VideoStream
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture mediaCapture;
        bool isPreviewing;

        DisplayRequest displayRequest = new DisplayRequest();

        public MainPage()
        {
            this.InitializeComponent();

            Application.Current.Suspending += Application_Suspending;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartCamera();
        }

        private async void StartCamera()
        {
            await StartPreviewAsync();
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            await CleanupCameraAsync();
        }

        private async Task StartPreviewAsync()
        {
            try
            {
                mediaCapture = new MediaCapture();
                // SELECTION DE LA CAMERA:
                // BODGE x1000
                DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                await (Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    feedback.Text = devices.Count.ToString();
                }));

                string cameraId = devices[devices.Count == 2 ? 1 : 0].Id;
                await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings { VideoDeviceId = cameraId });

                displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            catch (UnauthorizedAccessException)
            {
                feedback.Text = "Droits insuffisants";
                return;
            }

            try
            {
                webcamPreview.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                isPreviewing = true;

                VideoEncodingProperties previewProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
                VideoFrame videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);
                VideoFrame previewFrame = await mediaCapture.GetPreviewFrameAsync(videoFrame);
            }
            catch (System.IO.FileLoadException)
            {
                mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
            }
        }

        private async Task CleanupCameraAsync()
        {
            if (mediaCapture != null)
            {
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    webcamPreview.Source = null;
                    if (displayRequest != null)
                    {
                        displayRequest.RequestRelease();
                    }

                    mediaCapture.Dispose();
                    mediaCapture = null;
                });
            }
        }

        private async void _mediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender, MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        {
            if (args.Status == MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable)
            {
                feedback.Text = "Caméra déjà en utilisation";
            }
            else if (args.Status == MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable && !isPreviewing)
            {
                await (Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                 {
                     await StartPreviewAsync();
                 }));
            }
        }

        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
                await CleanupCameraAsync();
                deferral.Complete();
            }
        }

        private void btnServer_Click(object sender, RoutedEventArgs e)
        {
            OpenNewWindow(typeof(Server));
        }

        private void btnClient_Click(object sender, RoutedEventArgs e)
        {
            OpenNewWindow(typeof(Client));
        }

        private void btnCameraFrames_Click(object sender, RoutedEventArgs e)
        {
            OpenNewWindow(typeof(WebcamFrames));
        }

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