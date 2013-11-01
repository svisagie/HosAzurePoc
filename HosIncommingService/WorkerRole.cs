using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
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
		QueueClient _client, _summarisationClient;
	    private bool _run = true;
        private HosRepository _hosRepository = new HosRepository(CloudConfigurationManager.GetSetting("SqlDbConnectionString"));

		public override void Run()
		{
			Trace.WriteLine("Starting processing of messages");

		    while (_run)
		    {
                BrokeredMessage brokeredMessage;
                try
                {
                    brokeredMessage = _client.Receive(TimeSpan.FromSeconds(10));
                    if (brokeredMessage == null)
                    {
                        continue;
                    }
                }
                catch (Exception exception)
                {
                    continue;
                }

                try
                {
                    // Process the message
                    Trace.WriteLine("Processing Service Bus message: " + brokeredMessage.SequenceNumber);
                    var driverWorkstate = JsonConvert.DeserializeObject<DriverWorkstate>(brokeredMessage.GetBody<string>());
                    try
                    {
                        driverWorkstate = _hosRepository.SaveDriverWorkstate(driverWorkstate);
                        _summarisationClient.Send(new BrokeredMessage(JsonConvert.SerializeObject(driverWorkstate)));
                    }
                    catch (Exception exception)
                    {
                        brokeredMessage.Abandon();
                    }
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
            if (!namespaceManager.QueueExists(SummarisationQueueName))
            {
                namespaceManager.CreateQueue(SummarisationQueueName);
            }

			// Initialize the connection to Service Bus Queue
			_client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
            _summarisationClient = QueueClient.CreateFromConnectionString(connectionString, SummarisationQueueName);
			return base.OnStart();
		}

		public override void OnStop()
		{
		    _run = false;
			// Close the connection to Service Bus Queue
			_client.Close();
            _summarisationClient.Close();
			base.OnStop();
		}
	}
}
