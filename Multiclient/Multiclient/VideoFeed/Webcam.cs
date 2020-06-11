using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Controls;

namespace Multiclient.Communication
{
    public static class Webcam
    {
        private static MediaCapture captureManager;

        private static bool isCapturingFrames = false;
        private static int fps, width, height;

        private static CaptureElement videoPreview;
        private static SoftwareBitmap currentPicture;

        public static async void Start(int fps = 30, int width = 1280, int height = 720)
        {
            if (isCapturingFrames && captureManager.CameraStreamState != CameraStreamState.NotStreaming)
            {
                Debug.WriteLine("Camera is already started. It cannot be started again.");
                return;
            }

            Webcam.fps = fps;
            Webcam.width = width;
            Webcam.height = height;
            await InitializeCamera();
            await SetCameraProperties();
            await StartPreview();
            StartPreviewingFrames();
        }

        public static async void Stop()
        {
            if (!isCapturingFrames || captureManager.CameraStreamState == CameraStreamState.NotStreaming)
            {
                Debug.WriteLine("Camera is already stopped. It cannot be stopped again.");
                return;
            }

            isCapturingFrames = false;
            await captureManager.StopPreviewAsync();
        }

        private static async Task InitializeCamera()
        {
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            string cameraId = devices.Last().Id;

            captureManager = new MediaCapture();
            await captureManager.InitializeAsync(new MediaCaptureInitializationSettings() { VideoDeviceId = cameraId });
        }

        private static async Task SetCameraProperties()
        {
            VideoEncodingProperties properties = captureManager.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview)
                .Where(element => element.Subtype == "MJPG")
                .Where(element => ((VideoEncodingProperties)element).Width == 1280 && ((VideoEncodingProperties)element).Height == 720)
                .Where(element => ((VideoEncodingProperties)element).FrameRate.Numerator == fps)
                .FirstOrDefault() as VideoEncodingProperties;

            if (properties == null)
                throw new Exception($"No camera found with resolution {width}x{height} at {fps}fps.");

            await captureManager.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, properties);
        }

        private static async Task StartPreview()
        {
            videoPreview = new CaptureElement();
            videoPreview.Source = captureManager;
            await captureManager.StartPreviewAsync();
        }

        private static void StartPreviewingFrames()
        {
            isCapturingFrames = true;
            new Thread(new ThreadStart(PreviewFrame)).Start();
        }

        private static async void PreviewFrame()
        {
            while (isCapturingFrames)
            {
                VideoFrame videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);
                VideoFrame frame = await captureManager.GetPreviewFrameAsync(videoFrame);
                currentPicture = frame.SoftwareBitmap;
                Thread.Sleep(1000 / fps);
            }
        }

        public static SoftwareBitmap CurrentFrame
        {
            get
            {
                if (!isCapturingFrames)
                    return null;

                SoftwareBitmap temp = currentPicture;
                currentPicture = null;
                return temp;
            }
        }
    }
}