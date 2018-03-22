using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace VmsPlusNTC
{
	public partial class App : Application
	{
        public static string Token { get; set; }
        public const string NotificationReceivedKey = "NotificationReceived";

		public App ()
		{
			InitializeComponent();

            MainPage = new VmsPlusNTC.LoginPage();
        }

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
