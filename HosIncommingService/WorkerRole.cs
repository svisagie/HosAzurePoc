using System;
using System.Diagnostics;
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

namespace HosIncommingService
{
	public class WorkerRole : RoleEntryPoint
	{
		// The name of your queue
		private const string QueueName = "hosworkstateincomming";
		private const string SummarisationQueueName = "hosworkstatesummarisation";

		// QueueClient is thread-safe. Recommended that you cache 
		// rather than recreating it on every request
		private AzureSBQRecevier _sbqReceiver;
		private AzureSBQSender _sbqSender;
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
			_sbqSender = new AzureSBQSender(connectionString, SummarisationQueueName, int.Parse(CloudConfigurationManager.GetSetting("NumberOfSenders")));
			_sbqReceiver = new AzureSBQRecevier(connectionString, QueueName, int.Parse(CloudConfigurationManager.GetSetting("NumberOfReceivers")), int.Parse(CloudConfigurationManager.GetSetting("QueuePrefetchCount")));
			_sbqReceiver.OnMessageRecevied += sbqReceiver_OnMessageRecevied;
			return base.OnStart();
		}

		void sbqReceiver_OnMessageRecevied(BrokeredMessage brokeredMessage)
		{
			try
			{
				// Process the message
				Trace.WriteLine("Processing Service Bus message: " + brokeredMessage.SequenceNumber);
				var driverWorkstate = JsonConvert.DeserializeObject<DriverWorkstate>(brokeredMessage.GetBody<string>());
				if (driverWorkstate == null)
				{
					brokeredMessage.Complete();
					return;
				}
				try
				{
					driverWorkstate = _hosRepository.SaveDriverWorkstate(driverWorkstate);
					_sbqSender.Send(new BrokeredMessage(JsonConvert.SerializeObject(driverWorkstate)));
					brokeredMessage.Complete();
				}
				catch
				{
					brokeredMessage.Abandon();
				}
			}
			catch
			{
				brokeredMessage.DeadLetter();
			}

		}

		public override void OnStop()
		{
			// Close the connection to Service Bus Queue
			_sbqReceiver.StopRecevier();
			_sbqReceiver.Dispose();
			_sbqSender.Dispose();
			_manualResetEvent.Set();
			base.OnStop();
		}
	}
}
