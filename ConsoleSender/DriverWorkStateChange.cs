using System;

namespace ConsoleSender
{
	public class DriverWorkStateChange
	{
		public int DriverId { get; set; }
		public int WorkStateId { get; set; }
		public DateTime Timestamp { get; set; }
	}
}