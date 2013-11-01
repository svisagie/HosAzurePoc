using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
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

		// QueueClient is thread-safe. Recommended that you cache 
		// rather than recreating it on every request
		QueueClient _client;
        private bool _run = true;

        private HosRepository _hosRepository = new HosRepository(CloudConfigurationManager.GetSetting("SqlDbConnectionString"));

		public override void Run()
		{
			Trace.WriteLine("Starting processing of messages");

		    while (_run)
		    {
		        BrokeredMessage brokeredMessage = _client.Receive(TimeSpan.FromSeconds(10));
		        if (brokeredMessage == null)
		        {
		            continue;
		        }

                try
                {
                    // Process the message
                    Trace.WriteLine("Processing Service Bus message: " + brokeredMessage.SequenceNumber.ToString());
                    var driverWorkstate = JsonConvert.DeserializeObject<DriverWorkstate>(brokeredMessage.GetBody<string>());

                    var driverSummary = _hosRepository.FindDriverSummary(driverWorkstate.DriverId,
                        driverWorkstate.WorkStateId);

                    if (driverSummary == null)
                    {
                        driverSummary = new DriverSummary();
                        driverSummary.DriverId = driverWorkstate.DriverId;
                        driverSummary.WorkStateId = driverWorkstate.WorkStateId;
                        driverSummary.TotalSeconds = 0;
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

                    brokeredMessage.Complete();
                }
                catch (Exception exception)
                {
                    brokeredMessage.DeadLetter();
                }
		    }
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
			string connectionString = CloudConfigurationManager.GetSetting("HosPocQueueNamespace");
			var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
			if (!namespaceManager.QueueExists(QueueName))
			{
				namespaceManager.CreateQueue(QueueName);
			}

			// Initialize the connection to Service Bus Queue
			_client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
			return base.OnStart();
		}

		public override void OnStop()
		{
		    _run = false;
			// Close the connection to Service Bus Queue
			_client.Close();
			base.OnStop();
		}
	}
}
