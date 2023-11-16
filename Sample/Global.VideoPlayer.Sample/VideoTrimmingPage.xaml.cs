using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Global.VideoPlayer;
using Xamarin.CommunityToolkit.Core;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Global.VideoPlayer.Sample
{
    public partial class VideoTrimmingPage : ContentPage
    {
        FileResult videoFile;
        string videoPath;
        bool IsVideoSetuped = false;
        MediaStreams mediaInfo;

        public VideoTrimmingPage(FileResult video)
        {
            videoFile = video;
            videoPath = videoFile.FullPath;
            InitializeComponent();
            GetMediaInfo();
        }

        public VideoTrimmingPage(string video)
        {
            videoPath = video;
            InitializeComponent();
            GetMediaInfo();
        }

        private void GetMediaInfo()
        {
            TrackContainer.SetMediaPlayer(TrimmerPlayer.MediaPlayer);
            Task.Run(async () =>
            {
                if (string.IsNullOrEmpty(videoPath))
                {
                    string fileExt = MimeTypes.GetMimeTypeExtensions(videoFile.ContentType).FirstOrDefault();
                    string filePathName = Path.Combine(FileSystem.CacheDirectory, $"temp.{fileExt}");

                    using (var stream = await videoFile.OpenReadAsync())
                    using (var newStream = File.OpenWrite(filePathName))
                        await stream.CopyToAsync(newStream);
                    videoPath = filePathName;
                }
                TrackContainer.SetMediaInfo(videoPath);
            });
        }



        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            (sender as ImageButton).IsEnabled = false;
            LoadingView.IsVisible = true;
            TrackContainer.TrimCurrentVideo(OnTrimResulted);
            (sender as ImageButton).IsEnabled = true;

        }

        private void OnTrimResulted(bool IsSuccess, string outputPath)
        {
            bool IsExist = File.Exists(outputPath);
            Console.WriteLine(IsSuccess + "  " + IsExist);
            Device.BeginInvokeOnMainThread(async () =>
            {
                LoadingView.IsVisible = false;

                if (IsSuccess && IsExist)
                {
                    await Navigation.PushAsync(new VideoTrimmingPage(outputPath));
                }
            });
        }
    }
}

