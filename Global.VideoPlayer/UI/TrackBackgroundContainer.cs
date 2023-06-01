using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Global.VideoPlayer
{
	public class TrackBackgroundContainer : Grid
	{
        public TrackBackgroundContainer()
        {
            this.Margin = 0;
            this.Padding = 0;
            this.HorizontalOptions = LayoutOptions.FillAndExpand;
            this.ColumnSpacing = 0;
            this.HeightRequest = 40;
            this.BackgroundColor = Color.LightBlue;
        }

        bool IsSetuped = false;
        public int ImgCount { get; private set; } = -1;
        public List<TrackImageVM> TrackImgVM { get; private set; } = new List<TrackImageVM>();
        private string videoPath;
        double videoDuration;
        int targetH, targetW;

        protected override void OnSizeAllocated(double width, double height)
        {
            //if(!IsSetuped && this.Parent is TrimmerContainer parentView)
            //{
            //    IsSetuped = true;
            //    ImgCount = ((int)parentView.Width / 40);
            //    base.OnSizeAllocated(parentView.Width, height);
            //    AddChild();
            //    if(videoPath != null)
            //    {
            //        LoadThumnail();
            //    }
            //    return;
            //}
            base.OnSizeAllocated(width, height);

        }

        private void AddChild()
        {
            for(int i = 0; i < ImgCount; i++)
            {
                TrackImageVM imgVM = new TrackImageVM();
                Image image = new Image { Margin = 0, Aspect = Aspect.AspectFill, HeightRequest = 40, WidthRequest = 40, HorizontalOptions = LayoutOptions.FillAndExpand };
                image.SetBinding(Image.SourceProperty, new Binding(path: nameof(imgVM.ImgSrc), source: imgVM));
                this.Children.AddHorizontal(image);
                TrackImgVM.Add(imgVM);
            }
        }

        public void SetupThumnail(string videoPath , double videoDuration , int targetH, int targetW)
        {
            this.videoPath = videoPath;
            this.videoDuration = videoDuration;
            this.targetH = targetH;
            this.targetW = targetW;
            if(ImgCount > 0)
            {
                LoadThumnail();
            }
        }

        private void LoadThumnail()
        {
            Task.Run(() =>
            {
                DependencyService.Get<IVideoService>().GetVideoThumbnail(videoPath, videoDuration, targetH, targetH, TrackImgVM);
            });
        }
    }
}

