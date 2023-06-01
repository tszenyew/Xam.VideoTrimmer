using System;
using Xamarin.Forms;

namespace Global.VideoPlayer
{
	public class TrimmerPositionIndicator :BoxView
	{
		public TrimmerPositionIndicator()
		{
			BackgroundColor = Color.White;
			HeightRequest = 40;
			WidthRequest = 1;
			HorizontalOptions = LayoutOptions.Center;
		}
	}
}

