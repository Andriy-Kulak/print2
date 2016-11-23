using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.Rates
{
    public class RateAdType
    {
        public RateAdType()
        {
            Init();
        }

        private void Init()
        {
            AdType = string.Empty;
            RateTiers = new List<RateTier>();
        }

        public int AdTypeID { get; set; }
        public string AdType { get; set; }
        public string PublisherAgreement { get; set; }

        public List<RateTier> RateTiers { get; set; }

    }
}