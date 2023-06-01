﻿using System;
using System.Globalization;
using Global.VideoPlayer;
using Xamarin.Forms;

namespace Global.VideoPlayer.Converters
{
	public class MediaStateToPlayButtonVisibilityConverter : IValueConverter
    {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                VideoStatus state = (VideoStatus)value;
                return state == VideoStatus.Paused;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return VideoStatus.Paused;
            }
        
	}
}

