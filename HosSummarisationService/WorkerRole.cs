﻿using System;
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
		QueueClient Client;
		ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        private HosRepository _hosRepository = new HosRepository(CloudConfigurationManager.GetSetting("SqlDbConnectionString"));

		public override void Run()
		{
			Trace.WriteLine("Starting processing of messages");

			// Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
			Client.OnMessage((receivedMessage) =>
				 {
					 try
					 {
						 // Process the message
						 Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());
                         var driverWorkstate = JsonConvert.DeserializeObject<DriverWorkstate>(receivedMessage.GetBody<string>());

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

                         receivedMessage.Complete();
					 }
                     catch (Exception exception)
                     {
                         receivedMessage.DeadLetter();
                     }
				 });

			CompletedEvent.WaitOne();
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
			Client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
			return base.OnStart();
		}

		public override void OnStop()
		{
			// Close the connection to Service Bus Queue
			Client.Close();
			CompletedEvent.Set();
			base.OnStop();
		}
	}
}
