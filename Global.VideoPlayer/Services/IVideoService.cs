using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Global.VideoPlayer
{
	public interface IVideoService
	{
        MediaStreams GetMediaStreamInfo(string videoPath);
        void GetVideoThumbnail(string videoPath, double videoDuration, List<TrackImageVM> imageVMs);
        void TrimVideo(string videoPath, TimeSpan startTime, TimeSpan endTime, Action<bool,string> onTrimResulted);

    }
}

