using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthKit;
using System.Drawing;
using Xamarin.Forms;
using Global.VideoPlayer.iOS;
using AVFoundation;
using Newtonsoft.Json;
using Foundation;
using CoreMedia;
using UIKit;
using System.IO;
using System.Diagnostics;
using CoreGraphics;

[assembly: Dependency(typeof(VideoService))]
namespace Global.VideoPlayer.iOS
{
    public class VideoService : IVideoService
    {
        public MediaStreams GetMediaStreamInfo(string videoPath)
        {
            if (string.IsNullOrWhiteSpace(videoPath))
                throw new ArgumentNullException(nameof(videoPath));

            try
            {
                MediaStreams mediaStreamInfo = null;
                AVAsset asset = null;

                asset = AVAsset.FromUrl(NSUrl.CreateFileUrl(videoPath, null));

                mediaStreamInfo = GetStreamsFromAsset(asset);

                return mediaStreamInfo;

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return null;
            }
        }

        public void GetVideoThumbnail(string videoPath, double videoDuration, List<TrackImageVM> imageVMs)
        {
            try
            {
                if (imageVMs.Count() <= 0)
                    return;
                int numberOfThumnail = imageVMs.Count();
                double frameInterval = videoDuration / numberOfThumnail;
                AVAsset asset = AVAsset.FromUrl(NSUrl.CreateFileUrl(videoPath, null));
                var imgGenerator = new AVAssetImageGenerator(asset: asset);
                imgGenerator.AppliesPreferredTrackTransform = true;

                for (int i = 0; i < imageVMs.Count(); i++)
                {
                    System.Console.WriteLine($"Getting VideoThumnail {i + 1}");
                    double frameTime = i * frameInterval;
                    CMTime actualTime;
                    NSError err;
                    CGImage cGImage = imgGenerator.CopyCGImageAtTime(CMTime.FromSeconds(frameTime, 600), out actualTime, out err);
                    if (err != null)
                    {
                        Console.WriteLine(err);
                        continue;
                    }
                    var thumbnail = new UIImage(cGImage);
                    imageVMs[i].ImgSrc = ImageSource.FromStream(() => thumbnail.AsPNG().AsStream());
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Failed To getVideoThumbnail ex: {ex.Message}");
            }
        }

        public async void TrimVideo(string videoPath, TimeSpan startTime, TimeSpan endTime, Action<bool, string> onTrimResulted)
        {
            try
            {
                NSUrl videoURL = NSUrl.CreateFileUrl(videoPath, null);
                CMTimeRange timeRange = new CMTimeRange { Start = CMTime.FromSeconds(startTime.TotalSeconds, 600), Duration = CMTime.FromSeconds((endTime - startTime).TotalSeconds, 600) };
                AVAsset asset = AVAsset.FromUrl(videoURL);
                AVUrlAsset aVUrlAsset = (AVUrlAsset)asset;
                AVAssetExportSession exporter = new AVAssetExportSession(asset, AVAssetExportSessionPreset.LowQuality);
                exporter.TimeRange = timeRange;
                string exportFileType = AVFileType.Mpeg4;
                NSUrl outputUrl = GetOutputURL("mp4");
                exporter.OutputFileType = exportFileType;
                exporter.OutputUrl = outputUrl;

                await exporter.ExportTaskAsync();
                if (exporter.Error != null)
                {
                    Console.WriteLine(exporter.Error);
                    Console.WriteLine(exporter.Error.Description);
                    onTrimResulted?.Invoke(false, exporter.Error.Description);
                }
                else
                {
                    onTrimResulted?.Invoke(true, outputUrl.Path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                onTrimResulted.Invoke(false, ex.Message);
            }
        }

        private MediaStreams GetStreamsFromAsset(AVAsset asset)
        {
            List<MediaStreamInfo> streams = new List<MediaStreamInfo>();
            foreach (AVAssetTrack track in asset.Tracks)
            {
                MediaStreamInfo videoStreamInfo = GetStreamInfoFromTrack(track);
                streams.Add(videoStreamInfo);
            }
            return new MediaStreams(streams);
        }

        private NSUrl GetOutputURL(string exportFileType)
        {
            var dirs = NSSearchPath.GetDirectories(NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User);
            if (dirs == null || dirs.Length == 0)
                return null;
            string outputFilename = Path.Combine(dirs[0], $"output_{DateTime.Now.ToString(@"MMddyy_hhmmss")}.{exportFileType}");
            return NSUrl.FromFilename(outputFilename);
        }


        private MediaStreamInfo GetStreamInfoFromTrack(AVAssetTrack track)
        {
            MediaStreamInfo streamInfo = new MediaStreamInfo();
            if ("vide".Equals(track.MediaType))
            {
                streamInfo.codec_type = "video";
                streamInfo.height = (int)track.NaturalSize.Height;
                streamInfo.width = (int)track.NaturalSize.Width;
                streamInfo.bit_rate = track.EstimatedDataRate;
                streamInfo.duration = track.TimeRange.Duration.Seconds;
                streamInfo.r_frame_rate = track.NominalFrameRate.ToString();
            }
            else if ("soun".Equals(track.MediaType))
            {
                streamInfo.codec_type = "audio";
                streamInfo.bit_rate = track.EstimatedDataRate;
                streamInfo.duration = track.TimeRange.Duration.Seconds;
            }
            return streamInfo;
        }


        private string GetDurationStringFromCMTime(CMTime duration)
        {
            double durationTime = duration.Seconds;
            double minutes = durationTime / 60;
            double seconds = durationTime % 60;
            string videoDuration = $"{minutes}:{seconds}";
            return videoDuration;
        }
    }



    public class MediaInfoObject
    {
        public List<MediaStreamInfo> streams { get; set; }
    }
}

