using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using LoadTestClient;
using Microsoft.ServiceBus;

namespace ConsoleSender
{
	class Program
	{
		static void Main(string[] args)
		{
			//ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;
			//var sbq = new AzureSBQ.AzureSBQ("Endpoint=sb://hospoc.servicebus.windows.net/;SharedSecretIssuer=owner;SharedSecretValue=mEiPrhMsF+gtEuyAHXJvt9zwNH8OiFiuAI9j0W+qzbo=", "hosworkstateincomming", 50, 0, 50);
			//sbq.OnMessageRecevied += sbq_OnMessageRecevied;
			//sbq.StartRecevier();

			Console.WriteLine("How many threads?");
			var x = Console.ReadLine();
			var numberOfThreads = 0;
			if (int.TryParse(x, out numberOfThreads))
			{
				var loadTester = new DriverHosLoadTestPoster(numberOfThreads, ConfigurationManager.AppSettings["baseUri"]);
				loadTester.Start();
				var timer = new Timer(state => Console.WriteLine(DriverHosLoadTestPoster.GetStats()));
				timer.Change(0, 5000);
				Console.ReadLine();
				timer.Dispose();
				loadTester.Stop();
			}
			else
			{
				Console.WriteLine("That was not a number");
			}
		}

		static void sbq_OnMessageRecevied(string message)
		{
			Console.WriteLine(message);
		}
	}
}
