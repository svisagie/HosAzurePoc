using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleSender
{
	class Program
	{
		private static string _baseUri;
		static readonly Random Rnd = new Random();
		readonly static object ItemsLeftLock = new object();
		private static int _itemsLeft = 0;

		static void Main(string[] args)
		{
			_baseUri = ConfigurationManager.AppSettings["BaseUri"];
			Console.WriteLine("How many posts?");
			var x = Console.ReadLine();
			var count = 0;

			if (int.TryParse(x, out count))
			{
				_itemsLeft = count;
				for (var i = 0; i < count; i++)
				{
					var actionId = i;
					var task = new Task(() => DoPost(actionId));
					task.Start();
				}

				Console.WriteLine("Starting...");

				while (_itemsLeft > 0) { }

				Console.WriteLine("Done...");
			}
			else
			{
				Console.WriteLine("That was not a number");
			}
			//Console.ReadLine();
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
				var result = await client.PostAsJsonAsync(resource, stateChange);

				lock (ItemsLeftLock)
					_itemsLeft--;

				Console.WriteLine("{0}: DriverId {1} - {2}", id, stateChange.DriverId, result.StatusCode);
			}
		}
	}
}
