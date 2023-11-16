using System;
using Global.VideoPlayer.Converters;
using Xamarin.Forms;

namespace Global.VideoPlayer
{
    internal class TrimmerPositionIndicator :BoxView
	{
		public TrimmerPositionIndicator()
		{
			BackgroundColor = Color.White;
			HeightRequest = 40;
			WidthRequest = 1;
			HorizontalOptions = LayoutOptions.Center;
		}

        internal void SetupThumb(TrimmerView trimmerView)
        {
            if (trimmerView.mediaPlayer != null)
            {
                SetBinding(BoxView.TranslationXProperty, new Binding { Mode = BindingMode.OneWay, Path = nameof(trimmerView.mediaPlayer.Position), Source = trimmerView.mediaPlayer, Converter = new MediaPositionToTimeStripPositionConverter(trimmerView.maxValue, trimmerView.mediaDuration) });
                SetBinding(VisualElement.IsVisibleProperty, new Binding { Mode = BindingMode.OneWay, Path = nameof(trimmerView.mediaPlayer.Status), Source = trimmerView.mediaPlayer, Converter = new MediaStateToTimeStripVisibilityConverter() });
            }
            AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(this, new Rectangle(0, 0, 1, 40));
        }
    }
}

