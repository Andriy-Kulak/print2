using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.RatesReport
{
    public class RatesReportAdType
    {
        public RatesReportAdType()
        {
            Init();
        }

        private void Init()
        {
            PublisherAgreement = string.Empty;
            ParentAdType = string.Empty;
            EditionType = string.Empty;
            AdType = string.Empty;

            Tiers = new List<RatesReportTier>();
        }

        public string PublisherAgreement { get; set; }

        public int ParentAdTypeID { get; set; }
        public string ParentAdType { get; set; }
        public int EditionTypeID { get; set; }
        public string EditionType { get; set; }

        public int AdTypeID { get; set; }
        public string AdType { get; set; }

        public List<RatesReportTier> Tiers { get; set; }
    }
}