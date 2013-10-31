using System;
using System.Collections.Generic;

namespace SqlRepository.Models
{
    public partial class DriverSummary
    {
        public int DriverId { get; set; }
        public int WorkStateId { get; set; }
        public long TotalSeconds { get; set; }
    }
}
