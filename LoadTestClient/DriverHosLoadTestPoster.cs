using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Newtonsoft.Json;
using RestSharp;

namespace LoadTestClient
{
	public class DriverHosLoadTestPoster
	{
		private readonly int _numberOfThreads;
		private bool _stopping;
		Task[] _tasks;

		private static double _count = 0d;
		private static double _prevCount = 0d;
		private static double _duration = 0d;
		private static readonly object DurationLock = new object();
		private static readonly Stopwatch Stopwatch = new Stopwatch();
		private static string _baseUrl;
		private int _sendMessageCount;

		public DriverHosLoadTestPoster(int numberOfThreads, string baseUrl)
		{
			_numberOfThreads = numberOfThreads;
			_baseUrl = baseUrl;
		}

		public void Start()
		{
			_tasks = new Task[_numberOfThreads];
			for (var i = 0; i < _numberOfThreads; i++)
			{
				var task = new Task(() => DoContinuousPost(i));
				_tasks[i] = task;
			}
			_tasks.AsParallel().ForAll(t => t.Start());
		}

		public void Stop()
		{
			_stopping = true;
			Task.WaitAll(_tasks);
		}

		public static double GetStats()
		{
			if (Stopwatch.IsRunning)
			{
				Stopwatch.Stop();
				_duration = Stopwatch.Elapsed.TotalSeconds;
				Stopwatch.Reset();
			}
			var result = _duration > 0 ? (_count - _prevCount) / _duration : 0;
			_prevCount = _count;
			Stopwatch.Start();
			return result;
		}

		private void DoContinuousPost(int i)
		{
			PostDirectToQueue();
			//var restSharpClients = new RestClient[_numberOfThreads];
			//for (var j = 0; j < _numberOfThreads; j++)
			//{
			//	restSharpClients[j] = new RestClient(_baseUrl);
			//}

			//while (!_stopping)
			//{
			//	var senderIndex = Math.Abs(_sendMessageCount++ % _numberOfThreads);

			//	var randomWorkStateChange = DriverWorkStateChange.GetRandomWorkStateChange();
			//	PostUsingHttpClient(randomWorkStateChange);
			//	//PostUsingHttpWebRequest(randomWorkStateChange);
			//	//PostUsingRestSharp(randomWorkStateChange, restSharpClients[senderIndex]);
			//}
		}

		private void PostDirectToQueue()
		{
			var sbq = new AzureSBQ.AzureSBQSender("Endpoint=sb://hospoc.servicebus.windows.net/;SharedSecretIssuer=owner;SharedSecretValue=mEiPrhMsF+gtEuyAHXJvt9zwNH8OiFiuAI9j0W+qzbo=", "hosworkstateincomming", _numberOfThreads);
			while (!_stopping)
			{
				var randomWorkStateChange = DriverWorkStateChange.GetRandomWorkStateChange();
				sbq.SendMessage(randomWorkStateChange);
			}
		}

		private async void PostUsingHttpClient(DriverWorkStateChange randomWorkStateChange)
		{
			using (var client = new HttpClient())
			{
				var uri = new Uri(_baseUrl + "/api/driver/" + randomWorkStateChange.DriverId + "/workstate");
				try
				{
					var x = await client.PostAsJsonAsync(uri.ToString(), randomWorkStateChange);
					Trace.WriteLine(x);
				}
				catch (Exception ex)
				{
					Console.WriteLine("{0} /n {1}", ex.Message, ex.InnerException.Message);
				}
				_count++;
			}
		}

		private void PostUsingHttpWebRequest(DriverWorkStateChange randomWorkStateChange)
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(_baseUrl + "/api/driver/" + randomWorkStateChange.DriverId + "/workstate");
			httpWebRequest.ContentType = "text/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				var json = JsonConvert.SerializeObject(randomWorkStateChange);

				streamWriter.Write(json);
				streamWriter.Flush();
				streamWriter.Close();

				var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				if (httpResponse.StatusCode == HttpStatusCode.OK)
					_count++;

			}
		}

		private static void PostUsingRestSharp(DriverWorkStateChange randomWorkStateChange, RestClient client)
		{
			var request = new RestRequest("api/driver/{driverId}/workstate", Method.POST);
			request.AddUrlSegment("driverId", randomWorkStateChange.DriverId.ToString(CultureInfo.InvariantCulture));
			request.AddBody(JsonConvert.SerializeObject(randomWorkStateChange));
			try
			{
				client.ExecuteAsync(request, response =>
				{
					_count++;
				});
			}
			catch (Exception ex)
			{
				Trace.WriteLine(GetExceptionDetails(ex));
				throw;
			}
		}

		private static string GetExceptionDetails(Exception exception)
		{
			return "Exception: " + exception.GetType()
				 + "\r\nInnerException: " + exception.InnerException
				 + "\r\nMessage: " + exception.Message
				 + "\r\nStackTrace: " + exception.StackTrace;
		}
	}
}
