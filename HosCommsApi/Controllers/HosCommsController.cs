using System;
using HosCommsApi.Models;
using Microsoft.WindowsAzure;
using System.Web.Http;


namespace HosCommsApi.Controllers
{
	public class HosCommsController : ApiController
	{
		static readonly AzureSBQ.AzureSBQSender SBQSender;

		static HosCommsController()
		{
			var queueNamespace = CloudConfigurationManager.GetSetting("HosPocQueueNamespace");
			//	ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;
			SBQSender = new AzureSBQ.AzureSBQSender(queueNamespace, CloudConfigurationManager.GetSetting("IncommingQueueName"), int.Parse(CloudConfigurationManager.GetSetting("NumberOfSenders")));
		}

		[Route("api/driver/{driverId}/workstate"), HttpPost]
		public void WorkstatePost(int driverId, [FromBody] DriverWorkStateChange data)
		{
			SBQSender.SendMessageAsync(data);
		}

		[Route("api/driver/{driverId}/workstate/{workstate}"), HttpGet]
		public void WorkstateGet(int driverId, int workstate)
		{
			var data = new DriverWorkStateChange
			{
				DriverId = driverId,
				WorkStateId = (WorkStates)workstate,
				Timestamp = DateTime.UtcNow
			};

			SBQSender.SendMessageAsync(data);
		}
	}
}
