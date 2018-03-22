using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Common;
using Firebase.Iid;
using Firebase.Messaging;
using Xamarin.Forms;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.MobileServices;
using System.Diagnostics;
using Plugin.Settings;

namespace VmsPlusNTC.Droid
{
    [Activity(Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

            IsPlayServicesAvailable();

//#if DEBUG
            // Force refresh of the token. If we redeploy the app, no new token will be sent but the old one will
            // be invalid.
            //Task.Run(() =>
            //{
            //    // This may not be executed on the main thread.
            //    FirebaseInstanceId.Instance.DeleteInstanceId();
            //    Console.WriteLine("Forced token: " + FirebaseInstanceId.Instance.Token);
            //});
//#endif
        }

        //เช็คว่ามี google play service มั้ย
        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                {
                    // In a real project you can give the user a chance to fix the issue.
                    Console.WriteLine($"Error: {GoogleApiAvailability.Instance.GetErrorString(resultCode)}");
                }
                else
                {
                    Console.WriteLine("Error: Play services not supported!");
                    Finish();
                }
                return false;
            }
            else
            {
                Console.WriteLine("Play Services available.");
                return true;
            }
        }
    }

    // This service handles the device's registration with FCM.

    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseIIDService : FirebaseInstanceIdService
    {
        //เมื่อรันแล้วจะได้ tokenมา
        public override void OnTokenRefresh()
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            Console.WriteLine($"Token received: {refreshedToken}");
            VmsPlusNTC.Helpers.Settings.Token = refreshedToken;
            
            SendRegistrationToServerAsync(refreshedToken);
        }

        async void SendRegistrationToServerAsync(string token)
        {
            try
            {
                // Formats: https://firebase.google.com/docs/cloud-messaging/concept-options
                // The "notification" format will automatically displayed in the notification center if the 
                // app is not in the foreground.
                const string templateBodyFCM =
                 "{" +
                  "\"notification\" : {" +
                  "\"body\" : \"$(messageParam)\"," +
                    "\"title\" : \"Nauticomm\"" +
                    //," +
                  //"\"icon\" : \"myicon\" }" +
                 "}";
                   
                var templates = new JObject();
                templates["genericMessage"] = new JObject
    {
     {"body", templateBodyFCM}
    };

                
                var client = new MobileServiceClient("https://vmsplus.azurewebsites.net");
                
                var push = client.GetPush();

                await push.RegisterAsync(token, templates);
                VmsPlusNTC.Helpers.Settings.InstallationId = push.InstallationId.ToString();
                VmsPlusNTC.Helpers.Settings.Platform = "Android";
 
                // Push object contains installation ID afterwards.
                Console.WriteLine(push.InstallationId.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Debugger.Break();
            }
        }
    }


    // This service is used if app is in the foreground and a message is received.
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            Console.WriteLine("Received: " + message);

            // Android supports different message payloads. To use the code below it must be something like this (you can paste this into Azure test send window):
            // {
            //   "notification" : {
            //      "body" : "The body",
            //                 "title" : "The title",
            //                 "icon" : "myicon
            //   }
            // }
            try
            {
                var msg = message.GetNotification().Body;
                var notificationBuilder = new Notification.Builder(this)
            .SetSmallIcon(Resource.Drawable.ic_launcher)
            .SetContentTitle("Nauticomm")
            .SetContentText(msg)
            .SetAutoCancel(true);

                var notificationManager = NotificationManager.FromContext(this);
                notificationManager.Notify(0, notificationBuilder.Build());

                MessagingCenter.Send<object, string>(this, VmsPlusNTC.App.NotificationReceivedKey, msg); //set key ไว้ที่ app.cs
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error extracting message: " + ex);
            }
        }
    }

}

