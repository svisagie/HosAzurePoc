using System;
using System.Linq.Expressions;
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
			if (!RoleEnvironment.IsAvailable || RoleEnvironment.IsEmulated)
			{
				ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;
			}
			_client = QueueClient.CreateFromConnectionString(queueNamespace, CloudConfigurationManager.GetSetting("IncommingQueueName"));
		}

		[Route("api/driver/{driverId}/workstate"), HttpPost]
		public string WorkstatePost(int driverId, [FromBody] DriverWorkStateChange data)
		{
			try
			{
				var jsonData = JsonConvert.SerializeObject(data);
				var message = new BrokeredMessage(jsonData);
				message.Properties["DriverId"] = driverId;
				_client.Send(message);
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return string.Empty;
		}
	}
}
