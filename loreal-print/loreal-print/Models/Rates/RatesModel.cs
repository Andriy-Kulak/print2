using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.Rates
{
    public class RatesModel
    {
        public RatesModel()
        {
            Init();
        }

        private void Init()
        {
            Year = string.Empty;
            PublisherName = string.Empty;
            BookName = string.Empty;
            RateParentAdTypes = new List<RateParentAdType>();
        }

        public string BookName { get; set; }
        public string PublisherName { get; set; }
        public int ParentAdTypeID { get; set; }
        public int VersionID { get; set; }
        public string Year { get; set; }

        public List<RateParentAdType> RateParentAdTypes { get; set; }


    }
}