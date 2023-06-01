using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Global.VideoPlayer;
using Xamarin.CommunityToolkit.Core;
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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SetupPlayer();
        }

        private void GetMediaInfo()
        {
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

                mediaInfo = DependencyService.Get<IVideoService>().GetMediaStreamInfo(videoPath);
                if (mediaInfo == null || mediaInfo.Count == 0)
                {
                    return;
                }
                TrackContainer.SetMediaInfo(mediaInfo);
                TrackContainer.SetupThumnail(videoPath, 120, 120);
                Device.BeginInvokeOnMainThread(() =>
                {
                    TrackContainer.SetMediaPlayer(TrimmerPlayer.MediaPlayer);
                });
            });
        }



        private void SetupPlayer()
        {
            if(!IsVideoSetuped)
                TrimmerPlayer.SetVideoSource(VideoSource.FromFile(videoPath));
        }

        string outputFilename;
        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            TrimmerPlayer.MediaPlayer.Stop();
            outputFilename = Path.Combine(FileSystem.CacheDirectory, $"output_{DateTime.Now.ToString(@"MMddyy_hhmmss")}.mp4");
            DependencyService.Get<IVideoService>().TrimVideo(videoPath, TrackContainer.TrimStartTime, TrackContainer.TrimEndTime, outputFilename, OnTrimResulted);
        }

        private void OnTrimResulted(bool IsSuccess)
        {
            bool IsExist = File.Exists(outputFilename);
            Console.WriteLine(IsSuccess + "  " + IsExist);
            if (IsSuccess && IsExist)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PushAsync(new VideoTrimmingPage(outputFilename));
                });
            }
        }
    }
}

