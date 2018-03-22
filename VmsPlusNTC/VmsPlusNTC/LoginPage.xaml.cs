using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VmsPlusNTC
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
	{
		public LoginPage ()
		{
			InitializeComponent ();
            loginBtn.Clicked += LoginBtn_Clicked;
            //telBtn.Clicked += TelBtn_Clicked;


            if (Helpers.Settings.UserName != "")
            {
                var np = new NavigationPage(new MainPage());
                Navigation.PushModalAsync(np, false);
            }
        }

        //private void TelBtn_Clicked(object sender, EventArgs e)
        //{
        //    Device.OpenUri(new Uri("tel: 09212399939"));
        //}

        private async void LoginBtn_Clicked(object sender, EventArgs e)
        {
            loadIndicator.IsRunning = true;
            var client = new HttpClient();
            string user = userName.Text;
            string pw = password.Text;
            bool remember = true;

            var json = await client.GetStringAsync($"https://servicevmsformobile1.azurewebsites.net/api/vms/login?username={user}&password={pw}&remember={remember}");
            var login = JObject.Parse(json).ToObject<LoginResult>();

            if (login.Success == true)
            {
                Helpers.Settings.UserName = userName.Text;
                Helpers.Settings.Password = password.Text;

                var tokenid = Helpers.Settings.Token;
                var installationId = Helpers.Settings.InstallationId;
                var platform = Helpers.Settings.Platform;


                //gen deviceinfoid เพื่อเอาไปใส่ lastsync 
                var deviceid = Plugin.DeviceInfo.CrossDeviceInfo.Current.Id;
                //เอาค่า mobilephoneid ออกมาด้วย

                var json1 = await client.GetStringAsync($"https://servicevmsformobile1.azurewebsites.net/api/vms/createToken?username={user}&password={pw}&mobilePhoneId={deviceid}&platformToken={tokenid}&platform={platform}&installationId={installationId}");
                string token = JsonConvert.DeserializeObject(json1).ToString();

                Helpers.Settings.tokenWebview = token;

                loadIndicator.IsRunning = false;
                //go to navigation
                var np = new NavigationPage(new MainPage());
                await Navigation.PushModalAsync(np);
            }
            else
            {
                resultLogin.Text = "Invalid Username or Password";
                loadIndicator.IsRunning = false;
            }
        }
    }
}