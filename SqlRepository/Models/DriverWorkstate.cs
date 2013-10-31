using System;

namespace SqlRepository.Models
{
    public partial class DriverWorkstate
    {
        public long DriverWorkStateId { get; set; }
        public int DriverId { get; set; }
        public int WorkStateId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
