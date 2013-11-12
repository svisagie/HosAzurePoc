using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using AzureSBQ;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Newtonsoft.Json;
using SqlRepository;
using SqlRepository.Models;

namespace HosSummarisationService
{
	public class WorkerRole : RoleEntryPoint
	{
		// The name of your queue
		const string QueueName = "hosworkstatesummarisation";
		private AzureSBQRecevier _sbqReceiver;
		private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
		private readonly HosRepository _hosRepository = new HosRepository(CloudConfigurationManager.GetSetting("SqlDbConnectionString"));

		public override void Run()
		{
			Trace.WriteLine("Starting processing of messages");
			_sbqReceiver.StartRecevier();
			_manualResetEvent.WaitOne();
		}

		public override bool OnStart()
		{
			if (!RoleEnvironment.IsAvailable || RoleEnvironment.IsEmulated)
			{
				ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;
			}

			// Set the maximum number of concurrent connections 
			ServicePointManager.DefaultConnectionLimit = 12;

			// Create the queue if it does not exist already
			var connectionString = CloudConfigurationManager.GetSetting("HosPocQueueNamespace");
			_sbqReceiver = new AzureSBQRecevier(connectionString, QueueName, int.Parse(CloudConfigurationManager.GetSetting("NumberOfReceivers")), int.Parse(CloudConfigurationManager.GetSetting("QueuePrefetchCount")));
			_sbqReceiver.OnMessageRecevied += sbqReceiver_OnMessageRecevied;

			return base.OnStart();
		}

		private void sbqReceiver_OnMessageRecevied(BrokeredMessage brokeredMessage)
		{
			try
			{
				// Process the message
				Trace.WriteLine("Processing Service Bus message: " + brokeredMessage.SequenceNumber);
				var driverWorkstate = JsonConvert.DeserializeObject<DriverWorkstate>(brokeredMessage.GetBody<string>());

				var driverSummary = _hosRepository.FindDriverSummary(driverWorkstate.DriverId,
					 driverWorkstate.WorkStateId);

				if (driverSummary == null)
				{
					driverSummary = new DriverSummary
					{
						DriverId = driverWorkstate.DriverId,
						WorkStateId = driverWorkstate.WorkStateId,
						TotalSeconds = 0
					};
				}
				else
				{
					var lastDriverWorkstate =
						 _hosRepository.LastDriverWorkStateBefore(driverWorkstate.DriverId,
						 driverWorkstate.WorkStateId,
						 driverWorkstate.Timestamp);

					if (lastDriverWorkstate != null)
					{
						driverSummary.TotalSeconds += (long)(driverWorkstate.Timestamp - lastDriverWorkstate.Timestamp).TotalSeconds;
					}
				}

				_hosRepository.SaveDriverSummary(driverSummary);

				Thread.Sleep(int.Parse(CloudConfigurationManager.GetSetting("ProcessingDelay")));
				brokeredMessage.Complete();
			}
			catch (Exception exception)
			{
				brokeredMessage.DeadLetter();
			}
		}

		public override void OnStop()
		{
			// Close the connection to Service Bus Queue
			_sbqReceiver.StopRecevier();
			_sbqReceiver.Dispose();
			_manualResetEvent.Set();
			base.OnStop();
		}
	}
}
