using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Global.VideoPlayer.Sample
{	
	public partial class ViewVideoPage : ContentPage
	{
		string videoPath;
		public ViewVideoPage (string videoPath)
		{
			this.videoPath = videoPath;
			InitializeComponent ();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();
			videoPlayer.Source = new FileVideoSource { File = videoPath };
        }
    }
}

