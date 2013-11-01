using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HosUI.Models
{
    public class Workstate
    {
        public long DriverWorkStateId { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public int WorkStateId { get; set; }
        public string Timestamp { get; set; }
    }
}