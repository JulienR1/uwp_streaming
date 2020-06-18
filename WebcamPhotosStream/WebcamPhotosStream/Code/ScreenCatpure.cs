using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace WebcamPhotosStream.Code
{
    public class ScreenCatpure
    {
        private GraphicsCaptureItem item;
        private Direct3D11CaptureFramePool framePool;
        private IDirect3DDevice canvasDevice;
        private GraphicsCaptureSession session;

        public async Task Initialize()
        {
            if (!GraphicsCaptureSession.IsSupported())
            {
                Debug.WriteLine("Screen capturing not supported");
                return;
            }

            await StartCaptureAsync();
        }

        private async Task StartCaptureAsync()
        {
            GraphicsCapturePicker picker = new GraphicsCapturePicker();
            GraphicsCaptureItem item = await picker.PickSingleItemAsync();
            if(item != null)
            {
                StartCaptureInternal(item);
            }
        }

        private void StopCapture()
        {
            session?.Dispose();
            framePool?.Dispose();
            item = null;
            session = null;
            framePool = null;
        }

        private void StartCaptureInternal(GraphicsCaptureItem item)
        {
            StopCapture();

            this.item = item;
            framePool = Direct3D11CaptureFramePool.Create(canvasDevice, DirectXPixelFormat.B8G8R8A8UIntNormalized, 2, this.item.Size);            

            framePool.FrameArrived += (s, a) =>
            {
                using(Direct3D11CaptureFrame frame = framePool.TryGetNextFrame())
                {
                    ProcessFrame(frame);
                }
            };

            this.item.Closed += (s, a) =>
            {
                StopCapture();
            };

            session = framePool.CreateCaptureSession(this.item);
            session.StartCapture();
        }

        private void ProcessFrame(Direct3D11CaptureFrame frame)
        {
            
        }
    }
}
