using System;
using System.Collections.Generic;

namespace SqlRepository.Models
{
    public partial class DriverWorkstate
    {
        public long DriverWorkstateId { get; set; }
        public int DriverId { get; set; }
        public int WorkStateId { get; set; }
        public System.DateTime Timestamp { get; set; }
    }
}
