using System;
using System.Globalization;
using Xamarin.Forms;

namespace Global.VideoPlayer.Converters
{
    public class MediaPositionToTimeStripPositionConverter : IValueConverter
    {
        double TrackWidth, MediaDuration;
        public MediaPositionToTimeStripPositionConverter(double trackWidth, double mediaDuration)
        {
            TrackWidth = trackWidth;
            MediaDuration = mediaDuration;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan timeLeft = (TimeSpan)value;
            double xPosition = timeLeft.TotalSeconds / MediaDuration * TrackWidth;
            return xPosition;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double xPos = (double)value;
            double totalSec = xPos / TrackWidth * MediaDuration;
            return TimeSpan.FromSeconds(totalSec);
        }
    }
}

