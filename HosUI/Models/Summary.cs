using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HosUI.Models
{
    public class Summary
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public int WorkStateId { get; set; }
        public long TotalSeconds { get; set; }
    }
}