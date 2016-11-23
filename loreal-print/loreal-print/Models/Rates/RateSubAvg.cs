using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.Rates
{
    public class RateSubAvg
    {
        public RateSubAvg()
        {
            Init();
        }

        private void Init()
        {
            EditionType = string.Empty;
        }

        public string EditionType { get; set; }
        public int RateBaseCirculationGuarantee { get; set; }
        public int AveragePrintRun { get; set; }
    }
}