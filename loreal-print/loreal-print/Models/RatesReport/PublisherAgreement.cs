using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.RatesReport
{
    public class PublisherAgreement
    {
        public PublisherAgreement()
        {
            Init();
        }

        private void Init()
        {
            Answer = string.Empty;
        }

        public int RateTypeID { get; set; }
        public int AdTypeID { get; set; }
        public int EditionTypeID { get; set; }
        public string Answer { get; set; }
    }
}