using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HosCommsApi.Models;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using System.Web.Http;
using Microsoft.WindowsAzure.ServiceRuntime;
using Newtonsoft.Json;


namespace HosCommsApi.Controllers
{
	public class HosCommsController : ApiController
	{
		static readonly QueueClient _client;

		static HosCommsController()
		{
			var queueNamespace = CloudConfigurationManager.GetSetting("HosPocQueueNamespace");
			//if (!RoleEnvironment.IsAvailable || RoleEnvironment.IsEmulated)
			//{
			//	ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;
			//}
			_client = QueueClient.CreateFromConnectionString(queueNamespace, CloudConfigurationManager.GetSetting("IncommingQueueName"));
		}

		[Route("api/driver/{driverId}/workstate"), HttpPost]
		public async Task<string> WorkstatePost(int driverId, [FromBody] DriverWorkStateChange data)
		{
			return await SendMessage(driverId, data);
		}

		[Route("api/driver/{driverId}/workstate/{workstate}"), HttpGet]
		public async Task<string> WorkstateGet(int driverId, int workstate)
		{
			var data = new DriverWorkStateChange
			{
				DriverId = driverId,
				WorkStateId = (WorkStates)workstate,
				Timestamp = DateTime.UtcNow
			};

			return await SendMessage(driverId, data);
		}

		private static async Task<string> SendMessage(int driverId, DriverWorkStateChange data)
		{
			try
			{
				var jsonData = JsonConvert.SerializeObject(data);
				var message = new BrokeredMessage(jsonData);
				message.Properties["DriverId"] = driverId;
				await _client.SendAsync(message);
			}
			catch (Exception ex)
			{
				return string.Format("Exception: {0}, \nStackTrace: {1}", ex.Message, ex.StackTrace);
			}
			return string.Empty;
		}
	}
}
