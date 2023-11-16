using System;
using System.Collections.Generic;
using System.Dynamic;
using Global.VideoPlayer;
using Xamarin.Forms;

namespace Global.VideoPlayer
{
    public partial class TrimmerMediaPlayer : Grid
    {
        public TrimmerMediaPlayer()
        {
            InitializeComponent();
        }

        public void SetVideoSource(VideoSource mediaSource)
        {
            MediaPlayer.Source = mediaSource;
        }

        void Player_Tapped(System.Object sender, System.EventArgs e)
        {
            switch (MediaPlayer.Status)
            {
                case Global.VideoPlayer.VideoStatus.Playing:
                    MediaPlayer.Pause();
                    break;
                case Global.VideoPlayer.VideoStatus.Paused:
                    MediaPlayer.Play();
                    break;
            }
        }
    }
}

