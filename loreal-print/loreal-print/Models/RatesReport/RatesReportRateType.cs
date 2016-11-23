using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.RatesReport
{
    public class RatesReportRateType
    {
        public RatesReportRateType()
        {
            Init();
        }

        private void Init()
        {
            RateType = string.Empty;
            AdTypes = new List<RatesReportAdType>();
        }

        public int RateTypeID { get; set; }
        public string RateType { get; set; }
        public List<RatesReportAdType> AdTypes { get; set; }
    }
}