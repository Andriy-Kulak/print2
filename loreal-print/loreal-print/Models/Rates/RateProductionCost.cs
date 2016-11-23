using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.Rates
{
    public class RateProductionCost
    {
        public int AdTypeID { get; set; }
        public int EditionTypeID { get; set; }
        public int OpenProductionCost { get; set; }
        public int EarnedProductionCost { get; set; }
    }
}