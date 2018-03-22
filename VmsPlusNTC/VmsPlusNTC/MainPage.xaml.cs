using Newtonsoft.Json;
using Plugin.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace VmsPlusNTC
{
	public partial class MainPage : ContentPage
	{
        public MainPage()
		{
			InitializeComponent();
            LogoutBtn.Clicked += LogoutBtn_Clicked;
            //user.Text = Helpers.Settings.UserName;

            //user.Text = Helpers.Settings.Token + "aaaaa" + Helpers.Settings.InstallationId+"bbbbb" + Helpers.Settings.Platform;
            //user.Text = "test";
            //gen deviceinfoid เพื่อเอาไปใส่ lastsync 
            // var deviceid = Plugin.DeviceInfo.CrossDeviceInfo.Current.Id;

            // myWebView.Source = "https://vms.nauticomm.com/loginbytoken?token=" + Helpers.Settings.tokenWebview;
            //myWebView.Source = "www.google.co.th";

            //navigted ทำ sourceที่setค่าไว้ก่อน แล้วค่อยไปทำ  method Browser_Navigated
            //isLoadded ไว้setค่าว่า ทำเสร็จแล้ว จะได้ไม่มีการโหลดซ้ำ
            Browser.Navigated += Browser_Navigated;
            Browser.Source = "https://vms.nauticomm.com/loginbytoken?token=" + Helpers.Settings.tokenWebview;
            isLoadded = false;

        }

        bool isLoadded;

        private void Browser_Navigated(object sender, WebNavigatedEventArgs e)
        {
            if (!isLoadded)
            {
                Browser.Source = "https://vms.nauticomm.com/";
                isLoadded = true;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<object, string>(this, App.NotificationReceivedKey, OnMessageReceived);
        }
        private void OnMessageReceived(object sender, string msg)
        {
            Device.BeginInvokeOnMainThread(() => {
               // user.Text = msg;
            });
        }
        
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<object>(this, App.NotificationReceivedKey);
        }

        private async void LogoutBtn_Clicked(object sender, EventArgs e)
        {
            var client = new HttpClient();
            string username = Helpers.Settings.UserName;
            string password = Helpers.Settings.Password;
            var deviceid = Plugin.DeviceInfo.CrossDeviceInfo.Current.Id;

           // bool remember = false;
            //logout ให้setค่า remember เป็น false
         
            var json1 = await client.GetStringAsync($"https://servicevmsformobile1.azurewebsites.net/api/vms/ClearCustomerDataPushNoti?MobilePhoneId={deviceid}");

            Helpers.Settings.UserName = "";
            Helpers.Settings.Password = "";
            //Helpers.Settings.InstallationId = "";
            Helpers.Settings.tokenWebview = "";
            //Helpers.Settings.Platform = "";
            //Helpers.Settings.Token = "";

            await Navigation.PopModalAsync();
        }
    }
}
