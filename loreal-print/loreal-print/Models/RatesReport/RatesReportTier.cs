using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.RatesReport
{
    public class RatesReportTier
    {
        public int TierID { get; set; }
        public string TierRange { get; set; }
        public decimal? Rate { get; set; }
    }
}