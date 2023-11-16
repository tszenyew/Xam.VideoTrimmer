using System;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;

namespace Global.VideoPlayer
{
	internal class TrimmerThumb : Grid
	{
		public bool IsUpperThumb { get; private set; }
		public PanGestureRecognizer panGesture { get; private set; }
        Polygon indicator;


        public TrimmerThumb(bool isUpperThumb )
        {
            this.IsUpperThumb = isUpperThumb;
            Padding = new Thickness(15,0);
			HeightRequest = 46;
			RowDefinitions = new RowDefinitionCollection{ new RowDefinition { Height = new GridLength(46) }};
            indicator = GetIndicator();
            panGesture = new PanGestureRecognizer();
			GestureRecognizers.Add(panGesture);
            Children.Add(indicator) ;
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
                    Fill = new SolidColorBrush(Color.FromHex(TrimmerView.defaultThumbColor)),
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
                    Fill = new SolidColorBrush(Color.FromHex(TrimmerView.defaultThumbColor)),
                    StrokeThickness = 0,
                };
            }
        }

        internal void SetupThumb(TrimmerView trimmerView)
        {
            if (IsUpperThumb)
            {
                AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(this, new Xamarin.Forms.Rectangle(0, -3, 36, 46));
                indicator.SetBinding(Polygon.FillProperty, new Binding(nameof(trimmerView.UpperThumbColor), source: trimmerView));
                SetBinding(VisualElement.TranslationXProperty, new Binding
                {
                    Source = trimmerView,
                    Path = nameof(TrimmerView.UpperThumbX),
                    Mode = BindingMode.OneWay
                });
            }
            else
            {
                AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(this, new Xamarin.Forms.Rectangle(0, -3, 36, 46));
                indicator.SetBinding(Polygon.FillProperty, new Binding(nameof(trimmerView.LowerThumbColor), source: trimmerView));
                SetBinding(VisualElement.TranslationXProperty, new Binding
                {
                    Source = trimmerView,
                    Path = nameof(TrimmerView.LowerThumbX),
                    Mode = BindingMode.OneWay
                });
            }
        }
    }
}

