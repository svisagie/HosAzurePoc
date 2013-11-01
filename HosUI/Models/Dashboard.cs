using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HosUI.Models
{
    public class Dashboard
    {
        public IEnumerable<Workstate> Workstates { get; set; }
        public IEnumerable<Summary> Summaries { get; set; }
    }
}