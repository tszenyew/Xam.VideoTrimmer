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
using Xamarin.Forms.Platform.Android;
using Android.Content;

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
                throw ex;
            }
        }

        public void GetVideoThumbnail(string videoPath, double videoDuration, List<TrackImageVM> imageVMs)
        {
            if (imageVMs.Count() <= 0)
                return;
            int numberOfThumnail = imageVMs.Count();
            double frameInterval = (videoDuration / numberOfThumnail) * 1000;

            for (int i = 0; i < imageVMs.Count(); i++)
            {
                int frameTimeMs = (int)(i * frameInterval);
                Bitmap thumbnail = CreateThumnails(videoPath, frameTimeMs);
                using (var stream = new MemoryStream())
                {
                    thumbnail.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                    byte[] bytes = stream.ToArray();
                    imageVMs[i].ImgSrc = ImageSource.FromStream(() => new MemoryStream(bytes));
                }
            }
        }


        public void TrimVideo(string videoPath, TimeSpan startTime, TimeSpan endTime, Action<bool, string> onTrimResulted)
        {
            try
            {
                FFmpegKitConfig.IgnoreSignal(Com.Arthenica.Ffmpegkit.Signal.Sigxcpu);
                string outputPath = GetOutputFilePath();
                string startT = startTime.ToString(@"hh\:mm\:ss\.fff");
                string endT = endTime.ToString(@"hh\:mm\:ss\.fff");
                //string cmd = $"-ss {startTime.ToString(@"hh\:mm\:ss")} -i \"{videoPath}\"  -to {endTime.ToString(@"hh\:mm\:ss")} -c copy \"{outputPath}\"";
                //string cmd = $"-i \"{videoPath}\"  -vf \"trim=start={startT}:end={endT}\" -af \"atrim=start={startT}:end={endT}\" -c:v libx264 -preset faster -c:a aac \"{outputPath}\"";
                //string cmd = $"-ss {startT} -i \"{videoPath}\" -to {endT} -filter:v \"scale=\'min(1280,iw):-2\'\" -c:v libx264 -preset faster -c:a aac \"{outputPath}\"";
                string cmd = $"-ss {startT} -i \"{videoPath}\" -to {endT} -filter:v \"scale=\'min(1280,iw):-2\'\" -c:v libx265 -crf 28 -preset faster \"{outputPath}\"";

                FFmpegSession ffmpegSession = FFmpegKit.ExecuteAsync(cmd, new FFmpegCallback(onTrimResulted, outputPath));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                onTrimResulted.Invoke(false, ex.Message);
            }
        }

        private ImageSource GetThumbnailImageSource(string videoPath, int timeMs)
        {
            Bitmap thumbnail = CreateThumnails(videoPath, timeMs);
            using (var stream = new MemoryStream())
            {
                thumbnail.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                byte[] bytes = stream.ToArray();
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
        }

        private string GetOutputFilePath()
        {
            string cacheDirc = Android.App.Application.Context.CacheDir.AbsolutePath;
            return System.IO.Path.Combine(cacheDirc, $"output_{DateTime.Now.ToString(@"MMddyy_hhmmss")}.mp4");
        }

        private static void setDataSource(string video, MediaMetadataRetriever retriever)
        {
            Java.IO.File videoFile = new Java.IO.File(video);
            Java.IO.FileInputStream inputStream = new Java.IO.FileInputStream(videoFile.AbsolutePath);
            retriever.SetDataSource(inputStream.FD);
        }

        private Bitmap CreateThumnails(string videoPath, int timeMs)
        {
            Bitmap bitmap = null;
            int targetH = 120, targetW = 120;
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
                throw ex;
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

    }


    public class FFmpegCallback : Java.Lang.Object, IFFmpegSessionCompleteCallback
    {
        Action<bool, string> OnTrimResulted;
        string OutputPath;

        public FFmpegCallback(Action<bool, string> onTrimResulted, string outputPath)
        {
            OnTrimResulted = onTrimResulted;
            OutputPath = outputPath;
        }

        public void Apply(FFmpegSession session)
        {
            if (session.ReturnCode.IsValueSuccess)
            {
                OnTrimResulted?.Invoke(true, OutputPath);
            }
            else
            {
                OnTrimResulted?.Invoke(false, null);
            }
        }
    }

    public class MediaInfoObject
    {
        public List<MediaStreamInfo> streams { get; set; }
    }
}

