﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WebcamPhotosStream.Pages
{
    public sealed partial class pgServer : Page
    {
        private Server server;

        public static pgServer instance;

        public pgServer()
        {
            instance = this;
            this.InitializeComponent();
            server = new Server();
        }

        public async void SetImage(SoftwareBitmap bmp)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                SoftwareBitmapSource imgSrc = new SoftwareBitmapSource();
                await imgSrc.SetBitmapAsync(bmp);
                entryVideo.Source = imgSrc;
            });           

            Debug.WriteLine("Image drawn");
        }
    }
}
