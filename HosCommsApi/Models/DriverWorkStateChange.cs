using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HosCommsApi.Models
{
	public enum WorkStates : byte
	{
		Driving = 0,
		Work = 1,
		Rest = 2,
		OffDuty = 3
	}

	public class DriverWorkStateChange
	{
		public int DriverId { get; set; }
		public WorkStates WorkStateId { get; set; }
		public DateTime Timestamp { get; set; }
	}
}