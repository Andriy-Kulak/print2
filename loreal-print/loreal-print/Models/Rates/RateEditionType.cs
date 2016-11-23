using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.Rates
{
    public class RateEditionType
    {

        public RateEditionType()
        {
            Init();
        }

        private void Init()
        {
            EditionTypeName = string.Empty;
        }

        public int EditionTypeID { get; set; }
        public string EditionTypeName { get; set; }

        [Range(00.00, double.MaxValue, ErrorMessage = "Rate must be greater than 0.")]
        public decimal? Rate { get; set; }

        [Range(00.00, double.MaxValue, ErrorMessage = "CPM must be greater than 0.")]
        public decimal? CPM { get; set; }

        [Range(00.00, 99999.00, ErrorMessage = "Rate Base Circulation Guarantee must be between 0 and 99,999.")]
        public decimal? RateBaseCirculationGuarantee { get; set; }

        [Range(00.00, 99999.00, ErrorMessage = "Average Print Run must be between 0 between 0 and 99,999.")]
        public decimal? AveragePrintRun { get; set; }

        [Range(00, int.MaxValue, ErrorMessage = "Open Production Cost must be an integer greater than 0.")]
        public int? OpenProductionCost { get; set; }

        [Range(00, int.MaxValue, ErrorMessage = "Earned Production Cost must be an integer greater than 0.")]
        public int? EarnedProductionCost { get; set; }

        [Range(00.00, double.MaxValue, ErrorMessage = "Earned NEP must be greater than 0.")]
        public decimal? EarnedNEP { get; set; }
    }
}