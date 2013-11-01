using System;

namespace LoadTestClient
{
	public class DriverWorkStateChange
	{
		static readonly Random Rnd = new Random();

		public int DriverId { get; set; }
		public int WorkStateId { get; set; }
		public DateTime Timestamp { get; set; }

		public static DriverWorkStateChange GetRandomWorkStateChange(int maxDriverId = 999, int maxWorkStateId = 3)
		{
			return new DriverWorkStateChange
			{
				DriverId = Rnd.Next(0, 999),
				WorkStateId = Rnd.Next(0, 3),
				Timestamp = DateTime.UtcNow
			};
		}
	}
}