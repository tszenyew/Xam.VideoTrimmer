using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Global.VideoPlayer;
using Global.VideoPlayer.Converters;
using Xamarin.Forms;

namespace Global.VideoPlayer
{
	public class TrimmerContainer:AbsoluteLayout
	{
        bool IsChildAdded = false , IsThumnailLoading = false;
        const double thumbPadding = 13;
        public double maxValue { get; private set; } = 120;
        public double mediaDuration { get; private set; } = 30;
        public int ImgCount { get; private set; } = -1;
        private string videoPath;
        int targetH, targetW;
        public List<TrackImageVM> TrackImgVM { get; private set; } = new List<TrackImageVM>();
        MediaStreams mediaInfo;
        VideoPlayer mediaPlayer;

        public TrimmerThumb UpperThumb;
        public TrimmerThumb LowerThumb;
        public TrimmerPositionIndicator positionThumb;
        public TimeSpan TrimStartTime { get; private set; } = TimeSpan.Zero;
        public TimeSpan TrimEndTime { get; private set; } = TimeSpan.MaxValue;


        public event EventHandler OnDragCompleted;

        private double _LowerThumbX = -thumbPadding;
        private double _LowerThumbValue = 0;
        public double LowerThumbX
        {
            get => _LowerThumbX;
            set
            {
                _LowerThumbX = value;
                _LowerThumbValue = _LowerThumbX + thumbPadding;
                OnPropertyChanged(nameof(LowerThumbX));
                CalculateTrimStartDuration();
            }
        }

        public string DisplayLowerValue
        {
            get
            {
                return TrimStartTime.ToString(@"mm\:ss");
            }
        }

        private double _UpperThumbX = 200;
        private double _UpperThumbValue = 200 + thumbPadding;
        public double UpperThumbX
        {
            get => _UpperThumbX;
            set
            {
                _UpperThumbX = value;
                _UpperThumbValue = _UpperThumbX + thumbPadding;
                OnPropertyChanged(nameof(UpperThumbX));
                CalculateTrimEndDuration();
            }
        }

        public string DisplayUpperValue
        {
            get
            {
                return TrimEndTime.ToString(@"mm\:ss");
            }
        }

        private double _TrimmedSize = 0;
        public double TrimmedSize
        {
            get => _TrimmedSize;
            set
            {
                _TrimmedSize = value;
                OnPropertyChanged(nameof(TrimmedSize));
            }
        }

        private string _TrimmedSizeString = "";
        public string TrimmedSizeString
        {
            get => _TrimmedSizeString;
            set
            {
                _TrimmedSizeString = value;
                OnPropertyChanged(nameof(TrimmedSizeString));
            }
        }

        public TrimmerContainer()
		{
			BackgroundColor = Color.Transparent;
			HorizontalOptions = LayoutOptions.Fill;
			HeightRequest = 40;
			Padding = 0;
		}

        public void SetMediaInfo(MediaStreams mediaInfo)
        {
            this.mediaInfo = mediaInfo;
            mediaDuration = mediaInfo[0].duration;
            TrimEndTime = TimeSpan.FromSeconds(mediaDuration);
            OnPropertyChanged(nameof(DisplayUpperValue));
            UpdateMediaSize(mediaDuration);
        }

        public void SetupThumnail(string videoPath, int targetH, int targetW)
        {
            this.videoPath = videoPath;
            this.targetH = targetH;
            this.targetW = targetW;
            if (ImgCount > 0)
            {
                LoadThumnail();
            }
        }




        public void SetMediaPlayer(VideoPlayer mediaPlayer)
        {
            this.mediaPlayer = mediaPlayer;
            mediaPlayer.PlayRequested += MediaPlayer_PlayRequested;
            if (positionThumb != null)
            {
                positionThumb.SetBinding(BoxView.TranslationXProperty, new Binding { Mode = BindingMode.OneWay, Path = nameof(mediaPlayer.Position), Source = mediaPlayer, Converter = new MediaPositionToTimeStripPositionConverter(maxValue, mediaDuration) });
                positionThumb.SetBinding(VisualElement.IsVisibleProperty, new Binding { Mode = BindingMode.OneWay, Path = nameof(mediaPlayer.Status), Source = mediaPlayer, Converter = new MediaStateToTimeStripVisibilityConverter() });
            }
        }

        private void MediaPlayer_PlayRequested(object sender, EventArgs e)
        {
            mediaPlayer.PropertyChanged -= MediaPlayer_PropertyChanged;
            mediaPlayer.PropertyChanged += MediaPlayer_PropertyChanged;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            if(width > 0 && Parent is VisualElement parent)
            {
                ImgCount = (int)((parent.Width-40) / 40);
                maxValue = ImgCount * 40;
                double actualMargin = (parent.Width - maxValue) / 2;
                this.Padding = new Thickness(actualMargin, 0);
                SetupChild();

            }

            base.OnSizeAllocated(width, height);
        }

        private void SetupChild()
        {
            if (IsChildAdded)
                return;

            IsChildAdded = true;
            UpperThumbX = maxValue - thumbPadding;
            AddBackground();
            AddThumb();
            if(videoPath != null)
            {
                LoadThumnail();
            }
        }

        

        private void AddBackground()
        {
            BoxView bg = new BoxView { BackgroundColor = Color.Black, HeightRequest = 40, HorizontalOptions = LayoutOptions.FillAndExpand };
            AbsoluteLayout.SetLayoutFlags(bg , AbsoluteLayoutFlags.WidthProportional);
            AbsoluteLayout.SetLayoutBounds(bg, new Rect(0, 0, 1, 40));
            this.Children.Add(bg);
            LowerChild(bg);

            for (int i = 0; i < ImgCount; i++)
            {
                TrackImageVM imgVM = new TrackImageVM();
                Image image = new Image {
                    Margin = 0,
                    Aspect = Aspect.AspectFill,
                    HeightRequest = 40,
                    WidthRequest = 40,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                };
                AbsoluteLayout.SetLayoutBounds(image, new Rect(i * 40, 0, 40, 40));
                Grid a = new Grid();
                image.SetBinding(Image.SourceProperty, new Binding(path: nameof(imgVM.ImgSrc), source: imgVM));
                this.Children.Add(image);
                TrackImgVM.Add(imgVM);
            }
        }

        private void AddThumb()
        {
            UpperThumb = new TrimmerThumb(true);
            LowerThumb = new TrimmerThumb(false);
            positionThumb = new TrimmerPositionIndicator();
            if (mediaPlayer != null)
            {
                positionThumb.SetBinding(BoxView.TranslationXProperty, new Binding { Mode = BindingMode.OneWay, Path = nameof(mediaPlayer.Position), Source = mediaPlayer, Converter = new MediaPositionToTimeStripPositionConverter(maxValue, mediaDuration) });
                positionThumb.SetBinding(VisualElement.IsVisibleProperty, new Binding { Mode = BindingMode.OneWay, Path = nameof(mediaPlayer.Status), Source = mediaPlayer, Converter = new MediaStateToTimeStripVisibilityConverter() });
            }
            AbsoluteLayout.SetLayoutFlags(LowerThumb, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutFlags(UpperThumb, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutFlags(positionThumb, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(LowerThumb, new Rectangle(0,-3,26,46));
            AbsoluteLayout.SetLayoutBounds(UpperThumb, new Rectangle(0,-3,26,46));
            AbsoluteLayout.SetLayoutBounds(positionThumb, new Rectangle(0,0,1,40));
            LowerThumb.SetBinding(VisualElement.TranslationXProperty, new Binding
            {
                Source = this,
                Path = nameof(LowerThumbX),
                Mode = BindingMode.OneWay
            });
            UpperThumb.SetBinding(VisualElement.TranslationXProperty, new Binding
            {
                Source = this,
                Path = nameof(UpperThumbX),
                Mode = BindingMode.OneWay
            });
            LowerThumb.panGesture.PanUpdated += PanGesture_PanUpdated;
            UpperThumb.panGesture.PanUpdated += PanGesture_PanUpdated;

            this.Children.Add(LowerThumb);
            this.Children.Add(UpperThumb);
            this.Children.Add(positionThumb);
            RaiseChild(positionThumb);
            RaiseChild(LowerThumb);
            RaiseChild(UpperThumb);

        }

        private void MediaPlayer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals(nameof(mediaPlayer.Position)))
                return;

            if (mediaPlayer.Position >= TrimEndTime)
            {
                mediaPlayer.Pause();
            }
        }

        private void PanGesture_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    if(mediaPlayer.Status == VideoStatus.Playing)
                    {
                        mediaPlayer.Pause();
                    }

                    if (sender == UpperThumb)
                    {
                        double newPos = UpperThumbX + e.TotalX;
                        newPos = Math.Min(maxValue - thumbPadding, newPos);
                        double newUpperThumbPosX = Math.Max(newPos, LowerThumbX + 2);
                        UpperThumbX = newUpperThumbPosX;
                        
                    }
                    else if(sender == LowerThumb)
                    {
                        double newPos = LowerThumbX + e.TotalX;
                        newPos = Math.Max(-thumbPadding, newPos);
                        double newLowerThumbPosX = Math.Min(newPos, UpperThumbX - 2);
                        LowerThumbX = newLowerThumbPosX;
                    }
                    break;

                case GestureStatus.Completed:
                    UpdateMediaSize();
                    UpdateTrimMedia();
                    OnDragCompleted?.Invoke(sender, e);
                    break;
            }
        }

        private void CalculateTrimStartDuration()
        {
            if (mediaInfo == null)
                return;

            double second = _LowerThumbValue / maxValue * mediaDuration;
            TrimStartTime = TimeSpan.FromSeconds(second);
            Console.WriteLine("Updated TrimStartTime " + TrimStartTime.ToString());
            OnPropertyChanged(nameof(DisplayLowerValue));
        }

        private void CalculateTrimEndDuration()
        {
            if (mediaInfo == null)
                return;

            double second = _UpperThumbValue / maxValue * mediaDuration;
            TrimEndTime = TimeSpan.FromSeconds(second);
            OnPropertyChanged(nameof(DisplayUpperValue));
        }

        private void LoadThumnail()
        {
            if (IsThumnailLoading)
                return;

            IsThumnailLoading = true;
            Task.Run(() =>
            {
                DependencyService.Get<IVideoService>().GetVideoThumbnail(videoPath, mediaDuration, targetH, targetW, TrackImgVM);
            });
        }

        private void UpdateTrimMedia()
        {
            double xPosition = TrimStartTime.TotalSeconds / mediaDuration * maxValue;
            Console.WriteLine("Calculated Pos = " + xPosition + " ActualPos = " + LowerThumbX + thumbPadding);
            mediaPlayer.Position = TrimStartTime;
            mediaPlayer.Play();
        }

        private void UpdateMediaSize(double duration = -1)
        {
            if (mediaInfo == null)
                return;

            if (duration < 0)
            {
                TimeSpan durationSpan = TrimEndTime - TrimStartTime;
                duration = durationSpan.TotalSeconds;
            }
            double bytePerSec = mediaInfo.GetStreamsBytePerSecond();
            TrimmedSize = duration * bytePerSec;
            if (TrimmedSize > 999)
            {
                TrimmedSizeString = string.Format("{0:N2}", TrimmedSize / 1000) + " MB";
            }
            else
            {
                TrimmedSizeString = string.Format("{0:N2}", TrimmedSize) + "KB";
            }

        }
    }
}

