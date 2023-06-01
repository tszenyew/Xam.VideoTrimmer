using System;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;

namespace Global.VideoPlayer
{
	public class TrimmerThumb : Grid
	{
		public bool IsUpperThumb { get; private set; }
		public Color IndicatorColor { get; private set; }
		public PanGestureRecognizer panGesture { get; private set; }
        const string defaultThumbColor = "#FF5733";
        Polygon indicator;


        public TrimmerThumb(bool isUpperThumb , string indicatorColorHex = defaultThumbColor)
        {
            this.IsUpperThumb = isUpperThumb;
            this.IndicatorColor = Color.FromHex(indicatorColorHex);
			Padding = new Thickness(10, 0);
			HeightRequest = 46;
			this.RowDefinitions = new RowDefinitionCollection{ new RowDefinition { Height = new GridLength(46) }};
            indicator = GetIndicator();
            panGesture = new PanGestureRecognizer();
			this.GestureRecognizers.Add(panGesture);
            Children.Add(indicator) ;
		}

		public void SetIndicatorColor(Color color)
		{
			indicator.Fill = new SolidColorBrush(color);
		}

        private Polygon GetIndicator()
        {
            if (!IsUpperThumb)
            {
                return new Polygon
                {
                    Points = new PointCollection
                {
                    new Point(0, 0),
                    new Point(6, 0),
                    new Point(6, 3),
                    new Point(3, 3),
                    new Point(3, 43),
                    new Point(6, 43),
                    new Point(6, 46),
                    new Point(0, 46),
                },
                    Fill = new SolidColorBrush(Color.FromHex(defaultThumbColor)),
                    StrokeThickness = 0,
                };
            }
            else
            {
                return new Polygon
                {
                    Points = new PointCollection
                {
                    new Point(0, 0),
                    new Point(6, 0),
                    new Point(6, 46),
                    new Point(0, 46),
                    new Point(0, 43),
                    new Point(3, 43),
                    new Point(3, 3),
                    new Point(0, 3),
                },
                    Fill = new SolidColorBrush(Color.FromHex(defaultThumbColor)),
                    StrokeThickness = 0,
                };
            }
        }
	}
}

