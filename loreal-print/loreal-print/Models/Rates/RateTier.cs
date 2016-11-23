using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.Rates
{
    public class RateTier
    {
        public RateTier()
        {
            Init();
        }

        private void Init()
        {
            Tier = string.Empty;
            TierRange = string.Empty;

            RateEditionTypes = new List<RateEditionType>();
        }

        public int TierID { get; set; }
        public string Tier { get; set; }
        public string TierRange { get; set; }
        public List<RateEditionType> RateEditionTypes { get; set; }
    }
}