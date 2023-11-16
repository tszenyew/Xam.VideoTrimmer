using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Global.VideoPlayer
{
    public class MediaStreams : List<MediaStreamInfo>
    {
        public MediaStreams(List<MediaStreamInfo> streams)
        {
            this.AddRange(streams);
        }

        public MediaStreamInfo GetVideoStream()
        {
            return this.FirstOrDefault(x => x.IsVideoStream);
        }

        public double GetStreamsBytePerSecond()
        {
            return this.Sum(x => x.bit_rate)/8000;
        }
    }

    public class MediaStreamInfo
    {
        public int width { get; set; }
        public int height { get; set; }
        public double duration { get; set; }
        public double bit_rate { get; set; }
        public string r_frame_rate { get; set; }
        public string codec_type { get; set; }

        public bool IsVideoStream => string.Compare("video" ,codec_type , true) > 0;

        private double _fps = -1;
        public double fps
        {
            get
            {
                if (_fps == -1)
                {
                    string[] fps_arr = r_frame_rate.Split('/');
                    if (fps_arr.Length != 2 || fps_arr[1] == "0")
                        return -1;

                    _fps = Convert.ToDouble(fps_arr[0]) / Convert.ToDouble(fps_arr[1]);
                }
                return _fps;
            }
        }
    }
}

