using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Global.VideoPlayer;
using Global.VideoPlayer.Converters;
using Xamarin.Forms;

namespace Global.VideoPlayer
{
    public class TrimmerView : AbsoluteLayout
    {
        bool IsChildAdded = false, IsThumnailLoading = false;
        const double thumbPadding = 18;
        public double maxValue { get; private set; } = 120;
        public double mediaDuration { get; private set; } = 30;
        public int ImgCount { get; private set; } = -1;
        private string videoPath;
        int targetH, targetW;
        internal List<TrackImageVM> TrackImgVM { get; private set; } = new List<TrackImageVM>();
        MediaStreams mediaInfo;
        internal VideoPlayer mediaPlayer;

        internal TrimmerThumb UpperThumb;
        internal TrimmerThumb LowerThumb;
        internal TrimmerPositionIndicator positionThumb;
        public TimeSpan TrimStartTime { get; private set; } = TimeSpan.Zero;
        public TimeSpan TrimEndTime { get; private set; } = TimeSpan.MaxValue;
        internal const string defaultThumbColor = "#FF5733";


        public event EventHandler OnUpperThumbPanned;
        public event EventHandler OnLowerThumbPanned;

        public static readonly BindableProperty UpperThumbColorProperty =
            BindableProperty.Create(nameof(UpperThumbColor), typeof(Color), typeof(TrimmerView), Color.FromHex(defaultThumbColor),
                BindingMode.TwoWay);

        public Color UpperThumbColor
        {
            get => (Color)GetValue(UpperThumbColorProperty);
            set => SetValue(UpperThumbColorProperty, value);
        }

        public static readonly BindableProperty LowerThumbColorProperty =
            BindableProperty.Create(nameof(LowerThumbColor), typeof(Color), typeof(TrimmerView), Color.FromHex(defaultThumbColor),
                BindingMode.TwoWay);

        public Color LowerThumbColor
        {
            get => (Color)GetValue(LowerThumbColorProperty);
            set => SetValue(LowerThumbColorProperty, value);
        }

        public static readonly BindableProperty ExcludeAudioProperty =
            BindableProperty.Create(nameof(ExcludeAudio), typeof(bool), typeof(VideoPlayer), false);

        public bool ExcludeAudio
        {
            get => (bool)GetValue(ExcludeAudioProperty);
            set => SetValue(ExcludeAudioProperty, value);
        }

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
                return TimeSpan.FromSeconds(Math.Round(TrimStartTime.TotalSeconds)).ToString(@"mm\:ss");
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
                return TimeSpan.FromSeconds(Math.Round(TrimEndTime.TotalSeconds)).ToString(@"mm\:ss");
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

        public TrimmerView()
        {
            BackgroundColor = Color.Transparent;
            HorizontalOptions = LayoutOptions.Fill;
            HeightRequest = 40;
            Padding = 0;
        }

        public void SetMediaInfo(string videoPath)
        {
            this.videoPath = videoPath;
            mediaInfo = DependencyService.Get<IVideoService>().GetMediaStreamInfo(videoPath);
            mediaDuration = mediaInfo[0].duration;
            TrimEndTime = TimeSpan.FromSeconds(mediaDuration);
            OnPropertyChanged(nameof(DisplayUpperValue));
            UpdateMediaSize(mediaDuration);
            LoadThumnail();
            if(mediaPlayer != null)
            {
                Console.WriteLine($"SetMediaInfo PosThumb = {positionThumb != null} , mediaInfo = {mediaInfo != null} , player = {mediaPlayer != null}");
                SetPlayerSource();
            }
        }


        public void SetMediaPlayer(VideoPlayer mediaPlayer)
        {
            if (this.mediaPlayer != null)
            {
                this.mediaPlayer.PropertyChanged -= MediaPlayer_PropertyChanged;
                this.mediaPlayer.RemoveBinding(VideoPlayer.MuteProperty);
            }
            this.mediaPlayer = mediaPlayer;
            mediaPlayer.SetBinding(VideoPlayer.MuteProperty, new Binding(nameof(ExcludeAudio), source: this));
            mediaPlayer.PropertyChanged += MediaPlayer_PropertyChanged;
            if (mediaInfo != null)
            {
                Console.WriteLine($"Player Set PosThumb = {positionThumb != null} , mediaInfo = {mediaInfo != null} , player = {mediaPlayer != null}");
                SetPlayerSource();
            }
        }

        public void TrimCurrentVideo(Action<bool,string> OnTrimResult)
        {
            mediaPlayer.Pause();
            //Dont trim video less than 1 sec
            if ((TrimEndTime - TrimStartTime).TotalSeconds < 1)
            {
                OnTrimResult?.Invoke(true, videoPath);
                return;
            }
            else if (TrimStartTime.TotalMilliseconds > 1000)
            {
                TrimStartTime = TrimEndTime.Add(new TimeSpan(0, 0, -1));
            }
            else
            {
                TrimEndTime = TrimStartTime.Add(new TimeSpan(0, 0, 1));
            }
            Task.Run(() =>
            {
                try
                {
                    DependencyService.Get<IVideoService>().TrimVideo(videoPath, TrimStartTime, TrimEndTime, OnTrimResult);
                }
                catch (Exception ex)
                {
                    OnTrimResult?.Invoke(false, ex.Message);
                }
            });
        }

        private void SetPlayerSource()
        {
            SetPosThumbBinding();
            mediaPlayer.Source = new FileVideoSource { File = videoPath };
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            if (width > 0 && Parent is VisualElement parent)
            {
                ImgCount = (int)((parent.Width - 40) / 40);
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
        }



        private void AddBackground()
        {
            BoxView bg = new BoxView { BackgroundColor = Color.Black, HeightRequest = 40, HorizontalOptions = LayoutOptions.FillAndExpand };
            AbsoluteLayout.SetLayoutFlags(bg, AbsoluteLayoutFlags.WidthProportional);
            AbsoluteLayout.SetLayoutBounds(bg, new Rect(0, 0, 1, 40));
            this.Children.Add(bg);
            LowerChild(bg);

            for (int i = 0; i < ImgCount; i++)
            {
                TrackImageVM imgVM = new TrackImageVM();
                Image image = new Image
                {
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
            UpperThumb.SetupThumb(this);
            LowerThumb.SetupThumb(this);
            positionThumb.SetupThumb(this);
            Console.WriteLine($"Add Thumb PosThumb = {positionThumb != null} , mediaInfo = {mediaInfo != null} , player = {mediaPlayer != null}");
            SetPosThumbBinding();

            if (Device.RuntimePlatform == Device.Android)
            {
                LowerThumb.panGesture.PanUpdated += PanGesture_PanUpdated;
                UpperThumb.panGesture.PanUpdated += PanGesture_PanUpdated;
            }
            else
            {
                LowerThumb.panGesture.PanUpdated += PanGesture_IOSPanUpdated;
                UpperThumb.panGesture.PanUpdated += PanGesture_IOSPanUpdated;
            }

            this.Children.Add(LowerThumb);
            this.Children.Add(UpperThumb);
            this.Children.Add(positionThumb);
            RaiseChild(positionThumb);
            RaiseChild(LowerThumb);
            RaiseChild(UpperThumb);

        }

        private void SetPosThumbBinding()
        {
            if (positionThumb != null && mediaPlayer != null && mediaInfo != null)
            {
                Console.WriteLine($"********####### POSTHUMB BINDING {maxValue} {mediaDuration}");
                positionThumb.SetBinding(BoxView.TranslationXProperty, new Binding { Mode = BindingMode.OneWay, Path = nameof(mediaPlayer.Position), Source = mediaPlayer, Converter = new MediaPositionToTimeStripPositionConverter(maxValue, mediaDuration) });
                positionThumb.SetBinding(VisualElement.IsVisibleProperty, new Binding { Mode = BindingMode.OneWay, Path = nameof(mediaPlayer.Status), Source = mediaPlayer, Converter = new MediaStateToTimeStripVisibilityConverter() });
            }
        }


        //Set position to trim start position when playing complete
        private void MediaPlayer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals(nameof(mediaPlayer.Position)))
                return;
            Console.WriteLine("Pos Ended" + mediaPlayer.Position.TotalSeconds + " / " + mediaPlayer.TimeToEnd.TotalSeconds);
            if(mediaPlayer.Status == VideoStatus.Paused && mediaPlayer.TimeToEnd.TotalMilliseconds < 200)
            {
                mediaPlayer.Position = TrimStartTime;
            }
            else if (mediaPlayer.Position >= TrimEndTime)
            {
                mediaPlayer.Pause();
                mediaPlayer.Position = TrimStartTime;
            }

        }

        private void PanGesture_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            bool IsUpperThumb = sender == UpperThumb;
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    if (mediaPlayer.Status == VideoStatus.Playing)
                    {
                        mediaPlayer.Pause();
                    }

                    if (IsUpperThumb)
                    {
                        double newPos = UpperThumbX + e.TotalX;
                        newPos = Math.Min(maxValue - thumbPadding, newPos);
                        double newUpperThumbPosX = Math.Max(newPos, LowerThumbX + 2);
                        UpperThumbX = newUpperThumbPosX;

                    }
                    else 
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
                    if (IsUpperThumb)
                    {
                        OnUpperThumbPanned?.Invoke(this, e);
                    }
                    else
                    {
                        OnLowerThumbPanned?.Invoke(this, e);
                    }
                    break;
            }
            
        }

        double initialPos = 0;
        private void PanGesture_IOSPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            bool IsUpperThumb = sender == UpperThumb;
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    if (sender == UpperThumb)
                    {
                        initialPos = UpperThumbX;
                    }
                    else if (sender == LowerThumb)
                    {
                        initialPos = LowerThumbX;
                    }
                    break;

                case GestureStatus.Running:
                    if (mediaPlayer.Status == VideoStatus.Playing)
                    {
                        mediaPlayer.Pause();
                    }

                    if (IsUpperThumb)
                    {

                        double newPos = initialPos + e.TotalX;
                        newPos = Math.Min(maxValue - thumbPadding, newPos);
                        double newUpperThumbPosX = Math.Max(newPos, LowerThumbX + 2);
                        UpperThumbX = newUpperThumbPosX;

                    }
                    else
                    {
                        double newPos = initialPos + e.TotalX;
                        newPos = Math.Max(-thumbPadding, newPos);
                        double newLowerThumbPosX = Math.Min(newPos, UpperThumbX - 2);
                        LowerThumbX = newLowerThumbPosX;
                    }
                    break;

                case GestureStatus.Completed:
                    initialPos = 0;
                    UpdateMediaSize();
                    UpdateTrimMedia();
                    if (IsUpperThumb)
                    {
                        OnUpperThumbPanned?.Invoke(this, e);
                    }
                    else
                    {
                        OnLowerThumbPanned?.Invoke(this, e);
                    }
                    break;
            }
        }

        private void CalculateTrimStartDuration()
        {
            if (mediaInfo == null)
                return;

            double second = _LowerThumbValue / maxValue * mediaDuration;
            TrimStartTime = TimeSpan.FromSeconds(second);
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
            Task.Run(() =>
            {
                DependencyService.Get<IVideoService>().GetVideoThumbnail(videoPath, mediaDuration, TrackImgVM);
            });
        }

        private void UpdateTrimMedia()
        {
            double xPosition = TrimStartTime.TotalSeconds / mediaDuration * maxValue;
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

