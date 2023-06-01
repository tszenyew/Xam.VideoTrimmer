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
            LoadingView.IsVisible = true;
            MediaPlayer.UpdateStatus += MediaPlayer_MediaOpened;
            MediaPlayer.Source = mediaSource;
        }


        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            if (MediaPlayer.Status != VideoStatus.NotReady)
            {
                LoadingView.IsVisible = false;
            }
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

