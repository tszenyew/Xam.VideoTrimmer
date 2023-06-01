using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;

namespace Global.VideoPlayer.Sample
{
    public partial class SelectVideoPage : ContentPage
    {
        public SelectVideoPage()
        {
            InitializeComponent();
        }


        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            bool isAllowed = await CheckPermission();

            if (!isAllowed)
                return;

            try
            {
                FileResult video = await MediaPicker.PickVideoAsync(new MediaPickerOptions { Title = "Pick Video" });
                if (video != null)
                {
                    await Navigation.PushAsync(new VideoTrimmingPage(video));
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {fnsEx.Message}");

                // Feature is not supported on the device
            }
            catch (PermissionException pEx)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {pEx.Message}");

                // Permissions not granted
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
            }
            
        }

        private async Task<bool> CheckPermission()
        {
            var camera = new Camera();
            var media = new StorageRead();
            var photo = new StorageWrite();
            if (await camera.CheckStatusAsync() != Xamarin.Essentials.PermissionStatus.Granted)
            {
                await camera.RequestAsync();
            }
            if (await media.CheckStatusAsync() != Xamarin.Essentials.PermissionStatus.Granted)
            {
                await media.RequestAsync();
            }
            if (await photo.CheckStatusAsync() != Xamarin.Essentials.PermissionStatus.Granted)
            {
                await photo.RequestAsync();
            }

            bool IsCameraAllow = await camera.CheckStatusAsync() == Xamarin.Essentials.PermissionStatus.Granted;
            bool IsMediaAllow = await media.CheckStatusAsync() == Xamarin.Essentials.PermissionStatus.Granted;
            bool IsPhotoAllow = await photo.CheckStatusAsync() == Xamarin.Essentials.PermissionStatus.Granted;
            bool isAllowed = IsCameraAllow && IsMediaAllow && IsPhotoAllow;

            if (!isAllowed)
            {
                await DisplayAlert("Permission", $"Camera : {IsCameraAllow} \n Media : {IsMediaAllow} \n Photo : {IsPhotoAllow}", "Dismiss");
            }
            return isAllowed;
        }
    }
}

