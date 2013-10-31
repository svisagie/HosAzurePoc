using System;
using System.Configuration;
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

			SetTurboMode();

			Console.WriteLine("How many posts?");
			var x = Console.ReadLine();
			var count = 0;

			if (int.TryParse(x, out count))
			{
				_itemsLeft = count;
				var actions = new Action[count];
				for (var i = 0; i < count; i++)
				{
					var actionId = i;
					actions[i] = () => DoPost(actionId);
				}

				Console.WriteLine("Starting...");
				var options = new ParallelOptions { MaxDegreeOfParallelism = 200 };
				Parallel.Invoke(options, actions);

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

		private static void SetTurboMode()
		{
			int t, io;
			ThreadPool.GetMaxThreads(out t, out io);
			Debug("Default Max {0}, I/O: {1}", t, io);

			var success = ThreadPool.SetMinThreads(t, io);
			Debug("Successfully set Min {0}, I/O: {1}", t, io);
		}

		private static void Debug(string format, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine(
				 string.Format("{0} - Thead#{1} - {2}",
					  DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.fff"),
					  Thread.CurrentThread.ManagedThreadId.ToString(),
					  string.Format(format, args)));
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
