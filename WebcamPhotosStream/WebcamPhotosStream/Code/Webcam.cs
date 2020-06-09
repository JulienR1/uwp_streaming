using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Printers;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace WebcamPhotosStream
{
    public class Webcam
    {
        private CoreDispatcher dispatcher;

        private MediaCapture camera;
        private bool isCapturingFrames;
        private int fps = 15;

        private VideoEncodingProperties properties;
        private SoftwareBitmap currentBmp;
        private CaptureElement preview;

        public int Width { get => (int)properties.Width; }
        public int Height { get => (int)properties.Height; }

        public async Task Start(CoreDispatcher dispatcher, int fps = 15)
        {
            if (isCapturingFrames && camera.CameraStreamState != CameraStreamState.NotStreaming)
            {
                Debug.WriteLine("Camera already started, cannot complete.");
                return;
            }

            this.fps = fps;
            this.dispatcher = dispatcher;
            await InitializeCamera();
            await StartPreview();
            StartCapturingFrames();
        }

        public async void Stop()
        {
            if (!isCapturingFrames || camera.CameraStreamState != CameraStreamState.Streaming)
            {
                Debug.WriteLine("Cannot stop camera if it is not running");
                return;
            }

            isCapturingFrames = false;
            await camera.StopPreviewAsync();
        }

        private async Task InitializeCamera()
        {
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            string cameraId = devices[devices.Count - 1].Id;

            camera = new MediaCapture();
            await camera.InitializeAsync(new MediaCaptureInitializationSettings() { VideoDeviceId = cameraId });
            properties = camera.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
        }

        private async Task StartPreview()
        {
            // Necessaire pour que le preview se dirige à un endroitescalade 
            preview = new CaptureElement();
            preview.Source = camera;
            await camera.StartPreviewAsync();
        }

        private void StartCapturingFrames()
        {
            isCapturingFrames = true;
            Thread t = new Thread(new ThreadStart(PreviewFrame));
            t.Start();
        }

        private async void PreviewFrame()
        {
            while (isCapturingFrames)
            {
                VideoFrame videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)properties.Width, (int)properties.Height);
                VideoFrame frame = await camera.GetPreviewFrameAsync(videoFrame);
                currentBmp = frame.SoftwareBitmap;
                Thread.Sleep(1000 / fps);
            }
        }

        public async Task<byte[]> GetFrameBytesAsync()
        {
            if (currentBmp == null || !isCapturingFrames)
                return null;

            byte[] frameBytes = null;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                using (currentBmp)
                {
                    WriteableBitmap bmpBuffer = new WriteableBitmap(currentBmp.PixelWidth, currentBmp.PixelHeight);
                    currentBmp.CopyToBuffer(bmpBuffer.PixelBuffer);
                    frameBytes = bmpBuffer.PixelBuffer.ToArray();
                }
            });
            currentBmp = null;
            return frameBytes;
        }
    }
}