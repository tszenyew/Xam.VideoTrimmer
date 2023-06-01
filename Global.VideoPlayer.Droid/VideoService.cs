using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Nfc;
using Com.Arthenica.Ffmpegkit;
using Java.Nio.Channels;
using Java.Security;
using Xamarin.Forms;
using static Android.Provider.MediaStore;
using Xamarin.Forms.PlatformConfiguration;
using Android.Media;
using Android.OS;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using Android.Provider;
using Java.IO;
using Global.VideoPlayer.Droid;
using Newtonsoft.Json;
using Java.Nio.FileNio;

[assembly: Dependency(typeof(VideoService))]
namespace Global.VideoPlayer.Droid
{
    public class VideoService : IVideoService
    {
        public MediaStreams GetMediaStreamInfo(string videoPath)
        {
            TaskCompletionSource<double> videoDurationTCS = new TaskCompletionSource<double>();
            try
            {
                MediaStreams mediaStreamInfo = null;

                string cmd = $"-v error -show_entries stream=duration,width,height,r_frame_rate,bit_rate,codec_type -of json \"{videoPath}\"";
                FFprobeSession fprobeSession = FFprobeKit.Execute(cmd);
                if (fprobeSession.ReturnCode.IsValueSuccess)
                {
                    string mediaInfoString = fprobeSession.GetAllLogsAsString(300);
                    System.Console.WriteLine(mediaInfoString);
                    MediaInfoObject mediaInfo = JsonConvert.DeserializeObject<MediaInfoObject>(mediaInfoString);
                    mediaStreamInfo = new MediaStreams(mediaInfo.streams);
                }
                return mediaStreamInfo;

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return null;
            }
        }

        public ImageSource GetVideoThumbnail(string videoPath, int targetH, int targetW, int timeMs)
        {
            Bitmap thumbnail = CreateThumnails(videoPath, targetH, targetW, timeMs);
            using (var stream = new MemoryStream())
            {
                thumbnail.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                byte[] bytes = stream.ToArray();
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
        }

        public void TrimVideo(string videoPath, TimeSpan startTime, TimeSpan endTime, string outputPath, Action<bool> onTrimResulted)
        {
            Task.Run(() =>
            {

                try
                {
                    FFmpegKitConfig.IgnoreSignal(Com.Arthenica.Ffmpegkit.Signal.Sigxcpu);
                    string cmd = $"-i \"{videoPath}\" -ss {startTime.ToString(@"hh\:mm\:ss")} -to {endTime.ToString(@"hh\:mm\:ss")} -c:v libx264 -c:a aac \"{outputPath}\"";
                    FFmpegSession ffmpegSession = FFmpegKit.ExecuteAsync(cmd, new FFmpegCallback(onTrimResulted));
                    if (ffmpegSession.ReturnCode.IsValueSuccess)
                    {
                        string mediaInfoString = ffmpegSession.GetAllLogsAsString(300);
                        System.Console.WriteLine(mediaInfoString);
                    }

                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    onTrimResulted.Invoke(false);
                }
            });
        }

        private static void setDataSource(string video, MediaMetadataRetriever retriever)
        {
            Java.IO.File videoFile = new Java.IO.File(video);
            Java.IO.FileInputStream inputStream = new Java.IO.FileInputStream(videoFile.AbsolutePath);
            retriever.SetDataSource(inputStream.FD);
        }

        private Bitmap CreateThumnails(string videoPath, int targetH, int targetW, int timeMs)
        {
            Bitmap bitmap = null;
            MediaMetadataRetriever retriever = new MediaMetadataRetriever();
            try
            {
                if (videoPath.StartsWith("/"))
                {
                    setDataSource(videoPath, retriever);
                }
                else if (videoPath.StartsWith("file://"))
                {
                    setDataSource(videoPath.Substring(7), retriever);
                }
                else
                {
                    retriever.SetDataSource(videoPath, new Dictionary<string, string>());
                }

                if (targetH != 0 || targetW != 0)
                {
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.OMr1 && targetH != 0 && targetW != 0)
                    {
                        // API Level 27
                        bitmap = retriever.GetScaledFrameAtTime(timeMs * 1000, Android.Media.Option.Closest,
                                targetW, targetH);
                    }
                    else
                    {
                        bitmap = retriever.GetFrameAtTime(timeMs * 1000, Android.Media.Option.Closest);
                        if (bitmap != null)
                        {
                            int width = bitmap.Width;
                            int height = bitmap.Height;
                            if (targetW == 0)
                            {
                                targetW = Java.Lang.Math.Round(((float)targetH / height) * width);
                            }
                            if (targetH == 0)
                            {
                                targetH = Java.Lang.Math.Round(((float)targetW / width) * height);
                            }
                            System.Console.WriteLine(string.Format("original w:%d, h:%d => %d, %d", width, height, targetW, targetH));
                            bitmap = Bitmap.CreateScaledBitmap(bitmap, targetW, targetH, true);
                        }
                    }
                }
                else
                {
                    bitmap = retriever.GetFrameAtTime(timeMs * 1000, Android.Media.Option.Closest);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            finally
            {
                try
                {
                    retriever.Release();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            }

            return bitmap;
        }


        public void GetVideoThumbnail(string videoPath, double videoDuration, int targetH, int targetW, List<TrackImageVM> imageVMs)
        {
            if (imageVMs.Count() <= 0)
                return;
            int numberOfThumnail = imageVMs.Count();
            double frameInterval = (videoDuration / numberOfThumnail) * 1000;

            for (int i = 0 ; i < imageVMs.Count(); i++ )
            {
                int frameTimeMs = (int)(i * frameInterval);
                Bitmap thumbnail = CreateThumnails(videoPath, targetH, targetW, frameTimeMs);
                using (var stream = new MemoryStream())
                {
                    thumbnail.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                    byte[] bytes = stream.ToArray();
                    imageVMs[i].ImgSrc = ImageSource.FromStream(() => new MemoryStream(bytes));
                }
            }
        }
    }



    public class FFmpegStatistic : Java.Lang.Object, IStatisticsCallback
    {
        public void Apply(Statistics stat)
        {
            System.Console.WriteLine($"Stat size = {stat.Size} , quality = {stat.VideoQuality}");
        }
    }

    public class FFmpegLog : Java.Lang.Object, ILogCallback
    {
        public void Apply(Log p0)
        {
            System.Console.WriteLine($"{p0.SessionId} {p0.Message}");
        }
    }

    public class FFmpegCallback : Java.Lang.Object, IFFmpegSessionCompleteCallback
    {
        Action<bool> OnTrimResulted;
        public FFmpegCallback(Action<bool> onTrimResulted)
        {
            OnTrimResulted = onTrimResulted;
        }

        public void Apply(FFmpegSession session)
        {
            if (session.ReturnCode.IsValueSuccess)
            {
                OnTrimResulted?.Invoke(true);
            }
            else
            {
                OnTrimResulted?.Invoke(false);
            }
        }
    }

    public class FFProbeCallback : Java.Lang.Object, IFFprobeSessionCompleteCallback
    {
        private TaskCompletionSource<double> videoDurationTCS;
        public FFProbeCallback(TaskCompletionSource<double> videoDurationTCS)
        {
            this.videoDurationTCS = videoDurationTCS;
        }

        public void Apply(FFprobeSession session)
        {
            if (session.ReturnCode.IsValueSuccess)
            {
                string durationStr = session.GetAllLogsAsString(1000);
                System.Console.WriteLine(durationStr);
                double duration = Double.Parse(durationStr);
                videoDurationTCS.SetResult(duration);
            }
            else
            {
                videoDurationTCS.SetResult(-1);
            }
        }
    }

    public class MediaInfoObject
    {
        public List<MediaStreamInfo> streams { get; set; }
    }
}

