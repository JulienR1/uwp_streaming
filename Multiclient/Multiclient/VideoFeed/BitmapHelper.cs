using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Multiclient.VideoFeed
{
    public static class BitmapHelper
    {
        public static async Task<byte[]> BitmapToEncodedBytesAsync(SoftwareBitmap bmp)
        {
            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ms);
                encoder.SetSoftwareBitmap(bmp);

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception) { return new byte[0]; }

                byte[] bmpBytes = new byte[ms.Size];
                await ms.ReadAsync(bmpBytes.AsBuffer(), (uint)ms.Size, InputStreamOptions.None);
                return bmpBytes;
            }
        }

        public static async Task<SoftwareBitmap> EncodedBytesToBitmapAsync(byte[] bmpBytes)
        {
            SoftwareBitmap result = null;
            using (IRandomAccessStream ms = bmpBytes.AsBuffer().AsStream().AsRandomAccessStream())
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.JpegDecoderId, ms);
                result = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            return result;
        }
    }
}