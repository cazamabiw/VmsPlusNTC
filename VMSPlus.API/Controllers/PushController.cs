using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.NotificationHubs;
using Newtonsoft.Json.Linq;

namespace VMSPlus.API.Controllers
{
    [Route("vmsplus/[action]")]
    public class PushController : Controller
    {
        async Task<NotificationOutcome> SendNotification(string message, string installationId)
        {
            // Get the settings for the server project.
            //HttpConfiguration config = this.Configuration;

            // The name of the Notification Hub from the overview page.
            string notificationHubName = "VmsPlus";
            // Use "DefaultFullSharedAccessSignature" from the portal's Access Policies.
            string notificationHubConnection = "Endpoint=sb://vmsplus.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=3SWWzpHBge4CMEV/mCxi1g9uJLaWUJm+dXuG8o33G1g=";

            // Create a new Notification Hub client.
            var hub = NotificationHubClient.CreateClientFromConnectionString(
                notificationHubConnection,
                notificationHubName,
                // Don't use this in RELEASE builds. The number of devices is limited.
                // If TRUE, the send method will return the devices a message was
                // delivered to.
                enableTestSend: true);

            // Sending the message so that all template registrations that contain "messageParam"
            // will receive the notifications. This includes APNS, GCM, WNS, and MPNS template registrations.
            //byte[] bytes = Encoding.Default.GetBytes(message);
            //message = Encoding.UTF8.GetString(bytes);
            var templateParams = new Dictionary<string, string>
            {
                ["messageParam"] = message
            };

            // Send the push notification and log the results.

            NotificationOutcome result = null;
            if (string.IsNullOrWhiteSpace(installationId))
            {
                result = await hub.SendTemplateNotificationAsync(templateParams).ConfigureAwait(false);
            }
            else
            {
                result = await hub.SendTemplateNotificationAsync(templateParams, "$InstallationId:{" + installationId + "}").ConfigureAwait(false);
            }


            // Write the success result to the logs.
            //config.Services.GetTraceWriter().Info(result.State.ToString());
            return result;
        }


        [HttpPost]
        public async Task<IActionResult> SendAll(string message)
        {
            // Get the settings for the server project.
            //HttpConfiguration config = this.Configuration;
            try
            {
                await SendNotification(message, null);
            }
            catch (Exception ex)
            {
                // Write the failure result to the logs.
                //config.Services.GetTraceWriter().Error(ex.Message, null, "Push.SendAsync Error");
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendJson()
        {
            // Get the settings for the server project.
            //HttpConfiguration config = this.Configuration;
            try
            {
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var json = await reader.ReadToEndAsync();
                    var request = JObject.Parse(json).ToObject<PushRequest>();
                    await SendNotification(request.message, request.installationId);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                // Write the failure result to the logs.
                //config.Services.GetTraceWriter().Error(ex.Message, null, "Push.SendAsync Error");
                return BadRequest(ex.Message);
            }
 
        }

        [HttpPost]
        public async Task<IActionResult> Send(string installationId, string message)
        {
            // Get the settings for the server project.
            //HttpConfiguration config = this.Configuration;
            try
            {
                await SendNotification(message, installationId);
                return Ok();
            }
            catch (Exception ex)
            {
                // Write the failure result to the logs.
                //config.Services.GetTraceWriter().Error(ex.Message, null, "Push.SendAsync Error");
                return BadRequest(ex.Message);
            }

        }
    }

    public class PushRequest
    {
        public string installationId  { get; set; }
        public string message { get; set; }
    }
}