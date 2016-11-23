using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.Rates
{
    public class RateRateType
    {
        public RateRateType()
        {
            Init();
        }

        private void Init()
        {
            RateTypeName = string.Empty;
            RateAdTypes = new List<RateAdType>();
        }

        // Retail & General
        public int RateTypeID { get; set; }
        public string RateTypeName { get; set; }

        public List<RateAdType> RateAdTypes { get; set; }
    }
}