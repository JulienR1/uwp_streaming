using Multiclient.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace Multiclient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoFeedVisualization : Page
    {
        public VideoFeedVisualization()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string)
            {
                string clientId = (string)e.Parameter;
                MainPage.videoFeeds.Add(clientId, OnDataReceived);
            }
            base.OnNavigatedTo(e);
        }        

        public void OnDataReceived(object data, string clientId)
        {
            if (data.GetType() == typeof(SoftwareBitmap))
            {
                SetImage((SoftwareBitmap)data);
            }
            SetTitle(clientId);
        }

        private async void SetImage(SoftwareBitmap image)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                SoftwareBitmapSource imgSource = new SoftwareBitmapSource();
                await imgSource.SetBitmapAsync(image);
                videoFeed.Source = imgSource;
            });
        }

        private async void SetTitle(string titleText)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                title.Text = titleText;
            });
        }
    }
}