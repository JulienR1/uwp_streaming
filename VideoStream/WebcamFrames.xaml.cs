using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoStream
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebcamFrames : Page
    {
        MediaFrameSourceGroup selectedGroup = null;
        MediaFrameSourceInfo colorSourceInfo = null;

        MediaCapture mediaCapture;
        MediaFrameReader mediaFrameReader;

        MediaFrameSource colorFrameSource;

        private SoftwareBitmap backBuffer;
        private bool taskRunning = false;

        public WebcamFrames()
        {
            this.InitializeComponent();
        }

        private async void StartCamera()
        {
            await GetMediaFrameGroup();
            await CreateMediaCapture();
            await SetCameraFormat();
            await CreateFrameReader();
        }

        private async Task GetMediaFrameGroup()
        {
            var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();

            foreach (MediaFrameSourceGroup sourceGroup in frameSourceGroups)
            {
                foreach (MediaFrameSourceInfo sourceInfo in sourceGroup.SourceInfos)
                {
                    if (sourceInfo.MediaStreamType == MediaStreamType.VideoPreview && sourceInfo.SourceKind == MediaFrameSourceKind.Color)
                    {
                        colorSourceInfo = sourceInfo;
                        break;
                    }
                }
                if (colorSourceInfo != null)
                {
                    selectedGroup = sourceGroup;
                    break;
                }
            }
        }

        private async Task CreateMediaCapture()
        {
            mediaCapture = new MediaCapture();

            var settings = new MediaCaptureInitializationSettings()
             {
                 SourceGroup = selectedGroup,
                 SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                 MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                 StreamingCaptureMode = StreamingCaptureMode.Video,
             };            

            try
            {
                await mediaCapture.InitializeAsync(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MediaCapture initialization failed: " + ex.Message);
                return;
            }
        }

        private async Task SetCameraFormat()
        {
            colorFrameSource = mediaCapture.FrameSources[colorSourceInfo.Id];
            var preferredFormat = colorFrameSource.SupportedFormats.Where(format =>
            {
                return format.VideoFormat.Width >= 1080 && format.Subtype == MediaEncodingSubtypes.Argb32;
            }).FirstOrDefault();

            if (preferredFormat == null)
            {
                return;
            }

            await colorFrameSource.SetFormatAsync(preferredFormat);
        }

        private async Task CreateFrameReader()
        {
            mediaFrameReader = await mediaCapture.CreateFrameReaderAsync(colorFrameSource, MediaEncodingSubtypes.Argb32);
            mediaFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
            await mediaFrameReader.StartAsync();
        }

        private void ColorFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            var mediaFrameReference = sender.TryAcquireLatestFrame();
            var videoMediaFrame = mediaFrameReference?.VideoMediaFrame;
            var softwareBitmap = videoMediaFrame?.SoftwareBitmap;

            if (softwareBitmap != null)
            {
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                softwareBitmap = Interlocked.Exchange(ref backBuffer, softwareBitmap);
                softwareBitmap?.Dispose();

                ProcessImage();
            }
            mediaFrameReference?.Dispose();
        }

        private void ProcessImage()
        {
            var task = localImgRenderer.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (taskRunning)
                {
                    return;
                }
                taskRunning = true;

                SoftwareBitmap latestBitmap;
                while ((latestBitmap = Interlocked.Exchange(ref backBuffer, null)) != null)
                {                    
                    SoftwareBitmapSource src = new SoftwareBitmapSource();
                    await src.SetBitmapAsync(latestBitmap);
                    localImgRenderer.Source = src;

                    Client.bmpToSend.Enqueue(src);

                    latestBitmap.Dispose();
                }

                taskRunning = false;
            });
        }

        private async void StopCamera()
        {
            await mediaFrameReader.StopAsync();
            mediaFrameReader.FrameArrived -= ColorFrameReader_FrameArrived;
            mediaCapture.Dispose();
            mediaCapture = null;
        }

        private void OnStart(object sender, RoutedEventArgs e)
        {
            StartCamera();
        }

        private void OnStop(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }
    }
}