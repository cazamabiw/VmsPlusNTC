using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

namespace VmsPlusNTC.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            RequestPushPermissionAsync();

            _launchOptions = options;

            return base.FinishedLaunching(app, options);
        }

        NSDictionary _launchOptions; //xmlของ ios 

        //ขอ permission noti (allow user)
        async Task RequestPushPermissionAsync()
        {
            // iOS10 and later (https://developer.xamarin.com/guides/ios/platform_features/user-notifications/enhanced-user-notifications/#Preparing_for_Notification_Delivery)
            // Register for ANY type of notification (local or remote):
            var requestResult = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(
                UNAuthorizationOptions.Alert
                | UNAuthorizationOptions.Badge
                | UNAuthorizationOptions.Sound);


            // Item1 = approved boolean
            bool approved = requestResult.Item1;
            NSError error = requestResult.Item2;
            if (error == null)
            {
                // Handle approval
                if (!approved)
                {
                    Console.Write("Permission to receive notifications was not granted.");
                    return;
                }

                var currentSettings = await UNUserNotificationCenter.Current.GetNotificationSettingsAsync();
                if (currentSettings.AuthorizationStatus != UNAuthorizationStatus.Authorized)
                {
                    Console.WriteLine("Permissions were requested in the past but have been revoked (-> Settings app).");
                    return;
                }

                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
            else
            {
                Console.Write($"Error requesting permissions: {error}.");
            }
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);
            // If app was not running and we come from a notificatio badge, the notification is delivered via the options.
            if (_launchOptions != null && _launchOptions.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey))
            {
                var notification = _launchOptions[UIApplication.LaunchOptionsRemoteNotificationKey] as NSDictionary;
                PresentNotification(notification);
            }
            _launchOptions = null;
        }

        //ใช้ยิงไปที่ .form 
        void PresentNotification(NSDictionary dict)
        {
            // Extract some data from the notifiation and display it using an alert view.
            NSDictionary aps = dict.ObjectForKey(new NSString("aps")) as NSDictionary;

            var msg = string.Empty;
            if (aps.ContainsKey(new NSString("alert")))
            {
                msg = (aps[new NSString("alert")] as NSString).ToString();
            }

            if (string.IsNullOrEmpty(msg))
            {
                msg = "(unable to parse)";
            }

            MessagingCenter.Send<object, string>(this, App.NotificationReceivedKey, msg);

        }

        //สร้างtoken เพื่อส่งไปที่ server 
        public async override void RegisteredForRemoteNotifications(
            UIApplication application, NSData deviceToken)
        {
            if (deviceToken == null)
            {
                // Can happen in rare conditions e.g. after restoring a device.
                return;
            }

            Console.WriteLine($"Token received: {deviceToken}");
            await SendRegistrationToServerAsync(deviceToken);
        }
        async Task SendRegistrationToServerAsync(NSData deviceToken)
        {
            // This is the template/payload used by iOS. It contains the "messageParam"
            // that will be replaced by our service.
            const string templateBodyAPNS = "{\"aps\":{\"alert\":\"$(messageParam)\"}}";

            var templates = new JObject();
            templates["genericMessage"] = new JObject
   {
    {"body", templateBodyAPNS}
   };

            var client = new MobileServiceClient("https://vmsplus.azurewebsites.net");
            //await client.GetPush().RegisterAsync(deviceToken, templates);

            var push = client.GetPush();
            await push.RegisterAsync(deviceToken, templates);

            VmsPlusNTC.Helpers.Settings.Token = deviceToken.ToString();
            VmsPlusNTC.Helpers.Settings.InstallationId = push.InstallationId.ToString();
            VmsPlusNTC.Helpers.Settings.Platform = "IOS";
        }

        //กรณีที่ failed
        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            Console.WriteLine($"Failed to register for remote notifications: {error.Description}");
        }


        //กรณีได้รับ msg
        public override void DidReceiveRemoteNotification(
                    UIApplication application,
                    NSDictionary userInfo,
                    Action<UIBackgroundFetchResult> completionHandler)
        {
            // This will be called if the app is in the background/not running and if in the foreground.
            // However, it will not display a notification visually if the app is in the foreground.

            PresentNotification(userInfo);

            completionHandler(UIBackgroundFetchResult.NoData);
        }
    }
}
