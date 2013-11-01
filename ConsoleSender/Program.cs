using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleSender
{
	class Program
	{
		private static string _baseUri;
		static readonly Random Rnd = new Random();
		readonly static object ItemsLeftLock = new object();
		private static int _itemsLeft = 0;
		private static readonly Dictionary<string, long> Exceptions = new Dictionary<string, long>();

		static void Main(string[] args)
		{
			_baseUri = ConfigurationManager.AppSettings["BaseUri"];
			Console.WriteLine("How many posts?");
			var x = Console.ReadLine();
			var count = 0;
			var sw = new Stopwatch();
			sw.Start();
			if (int.TryParse(x, out count))
			{
				_itemsLeft = count;
				var actions = new Action[count];
				for (var i = 0; i < count; i++)
				{
					var actionId = i;
					DoPost(actionId);
				}

				//Console.WriteLine("Starting...");
				//var options = new ParallelOptions { MaxDegreeOfParallelism = 100 };
				//Parallel.Invoke(options, actions);

				while (_itemsLeft > 0) { }

				Console.WriteLine("Done...");
			}
			else
			{
				Console.WriteLine("That was not a number");
			}

			sw.Stop();
			Console.WriteLine("Took {0} seconds for {1} posts ({2}/sec)", sw.Elapsed.TotalSeconds, count, count / sw.Elapsed.TotalSeconds);
			foreach (var exception in Exceptions)
			{
				Console.WriteLine("{0} = {1}", exception.Key, exception.Value);
			}
			Console.ReadLine();
		}

		private static async void DoPost(int id)
		{
			using (var client = new HttpClient())
			{
				var stateChange = new DriverWorkStateChange
				{
					DriverId = Rnd.Next(0, 999),
					WorkStateId = Rnd.Next(0, 3),
					Timestamp = DateTime.UtcNow
				};

				client.BaseAddress = new Uri(_baseUri);
				var resource = string.Format("/api/driver/{0}/workstate", stateChange.DriverId);
				HttpResponseMessage result = null;
				try
				{
					result = await PostAsJsonAsync(client, resource, stateChange);
				}
				catch (Exception ex)
				{
					if (!Exceptions.ContainsKey(ex.Message))
						Exceptions[ex.Message] = 1;
					else
						Exceptions[ex.Message]++;
				}

				lock (ItemsLeftLock)
					_itemsLeft--;

				Console.WriteLine("{0}: DriverId {1} - {2}", id, stateChange.DriverId, (result != null ? result.StatusCode.ToString() : "Failed"));
			}
		}

		private static async Task<HttpResponseMessage> PostAsJsonAsync(HttpClient client, string resource, DriverWorkStateChange stateChange)
		{
			return await client.PostAsJsonAsync(resource, stateChange);
		}

		internal class DriverWorkStateChange
		{
			public int DriverId { get; set; }
			public int WorkStateId { get; set; }
			public DateTime Timestamp { get; set; }
		}
	}

	internal class PostContext
	{
		public int N;
		public ManualResetEvent DoneEvent;

		public PostContext(int n, ManualResetEvent doneEvent)
		{
			N = n;
			DoneEvent = doneEvent;
		}
	}
}
