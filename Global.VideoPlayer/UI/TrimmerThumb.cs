using System;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;

namespace Global.VideoPlayer
{
	internal class TrimmerThumb : Grid
	{
		public bool IsUpperThumb { get; private set; }
		public PanGestureRecognizer panGesture { get; private set; }
        TrimmerView trimmerView;
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
            Children.Add(indicator);
            SetupIndicatorColor();

        }


        private Polygon GetIndicator()
        {
            if (!IsUpperThumb)
            {
                var polygon = new Polygon
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
                    StrokeThickness = 0,
                };
                return polygon;
            }
            else
            {
                var polygon = new Polygon
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
                    StrokeThickness = 0,
                };
                return polygon;
            }
        }

        internal void SetupThumb(TrimmerView trimmerView)
        {
            this.trimmerView = trimmerView;
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

                if (indicator != null)
                {
                    indicator.SetBinding(Polygon.FillProperty, new Binding(nameof(trimmerView.LowerThumbColor), source: trimmerView));
                }
            }
            else
            {
                AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(this, new Xamarin.Forms.Rectangle(0, -3, 36, 46));
                SetBinding(VisualElement.TranslationXProperty, new Binding
                {
                    Source = trimmerView,
                    Path = nameof(TrimmerView.LowerThumbX),
                    Mode = BindingMode.OneWay
                });
            }
            SetupIndicatorColor();
        }

        internal void SetupIndicatorColor()
        {
            if (indicator == null || trimmerView == null)
                return;

            if (IsUpperThumb)
            {
                indicator.SetBinding(Polygon.FillProperty, new Binding(nameof(trimmerView.UpperThumbColor), source: trimmerView));
            }
            else
            {
                indicator.SetBinding(Polygon.FillProperty, new Binding(nameof(trimmerView.LowerThumbColor), source: trimmerView));
            }
        }
    }
}

