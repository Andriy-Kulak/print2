using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.RatesReport
{
    public class RatesReportModel
    {
        public RatesReportModel()
        {
            Init();
        }

        private void Init()
        {
            Year = string.Empty;

            RateTypes = new List<RatesReportRateType>();
        }

        public int BookID { get; set; }
        public int VersionID { get; set; }
        public string Year { get; set; }
        
        public List<RatesReportRateType> RateTypes { get; set; }
    }
}