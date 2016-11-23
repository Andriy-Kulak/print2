using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace loreal_print.Models.Rates
{
    public class RateOpen
    {
        public RateOpen()
        {
            Init();
        }

        private void Init()
        {
            Type = string.Empty;
        }
        public decimal Rate { get; set; }
        public decimal CPM { get; set; }
        public string Type { get; set; }
    }
}