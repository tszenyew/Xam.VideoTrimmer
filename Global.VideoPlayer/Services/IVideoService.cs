using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Global.VideoPlayer
{
	public interface IVideoService
	{
        MediaStreams GetMediaStreamInfo(string videoPath);
        void GetVideoThumbnail(string videoPath, double videoDuration, int targetH, int targetW, List<TrackImageVM> imageVMs);
        void TrimVideo(string videoPath, TimeSpan startTime, TimeSpan endTime, string outputPath, Action<bool> onTrimResulted);

    }
}

