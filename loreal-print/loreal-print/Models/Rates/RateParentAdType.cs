using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace loreal_print.Models.Rates
{
    public class RateParentAdType
    {
        public RateParentAdType()
        {
            RateRateTypes = new List<RateRateType>();
        }
        public int ParentAdTypeID { get; set; }
        public string ParentAdType { get; set; }

        [DisplayName("Advertorial Earned Percent Discount")]
        [Range(00.00, 99.99, ErrorMessage = "Advertorial Earned Percent Discount must be between 0.00% and 99.99%.")]
        public decimal? AdvertorialEarnedPercentDiscount { get; set; }

        [DisplayName("Bleed Open Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Bleed Open Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? BleedOpenPercentPremium { get; set; }

        [DisplayName("Bleed Earned Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Bleed Earned Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? BleedEarnedPercentPremium { get; set; }

        [DisplayName("Cover2 Open Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Cover2 Open Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? Cover2OpenPercentPremium { get; set; }

        [DisplayName("Cover2 Earned Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Cover2 Earned Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? Cover2EarnedPercentPremium { get; set; }

        [DisplayName("Cover3 Open Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Cover3 Open Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? Cover3OpenPercentPremium { get; set; }

        [DisplayName("Cover3 Earned Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Cover3 Earned Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? Cover3EarnedPercentPremium { get; set; }

        [DisplayName("Cover4 Open Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Cover4 Open Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? Cover4OpenPercentPremium { get; set; }

        [DisplayName("Cover4 Earned Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Cover4 Earned Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? Cover4EarnedPercentPremium { get; set; }

        [DisplayName("Frac Half Page Open Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Frac Half Page Open Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? FracHalfPageOpenPercentPremium { get; set; }

        [DisplayName("Frac Half Page Earned Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Frac Half Page Earned Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? FracHalfPageEarnedPercentPremium { get; set; }

        [DisplayName("Frac Third Page Open Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Frac Third Page Open Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? FracThirdPageOpenPercentPremium { get; set; }

        [DisplayName("Frac Third Page Earned Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Frac Third Page Earned Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? FracThirdPageEarnedPercentPremium { get; set; }

        [DisplayName("Frac Third Run Opp FBP Open Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Frac Third Run Opp FBP Open Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? FracThirdRunOppFBPOpenPercentPremium { get; set; }

        [DisplayName("Frac Third Run Opp FBP Earned Percent Premium")]
        [Range(00.00, 99.99, ErrorMessage = "Frac Third Run Opp FBP Earned Percent Premium must be between 0.00% and 99.99%.")]
        public decimal? FracThirdRunOppFBPEarnedPercentPremium { get; set; }

        [DisplayName("Spread C2P1 Earned Percent Discount")]
        [Range(00.00, 99.99, ErrorMessage = "Spread C2P1 Earned Percent Discount must be between 0.00% and 99.99%.")]
        public decimal? SpreadC2P1EarnedPercentDiscount { get; set; }

        [DisplayName("Spread ROB Earned Percent Discount")]
        [Range(00.00, 99.99, ErrorMessage = "Spread ROB Earned Percent Discount must be between 0.00% and 99.99%.")]
        public decimal? SpreadROBEarnedPercentDiscount { get; set; }

        [DisplayName("Fifth Color Metallic Open Dollar Premium")]
        [Range(00.00, 2000000000.00, ErrorMessage = "Fifth Color Metallic Open Dollar Premium must be $2,000,000,000.00 or less.")]
        public int? FifthColorMetallicOpenDollarPremium { get; set; }

        [DisplayName("Fifth Color Metallic Earned Dollar Premium")]
        [Range(00.00, 2000000000.00, ErrorMessage = "Fifth Color Metallic Earned Dollar Premium must be $2,000,000,000.00 or less.")]
        public int? FifthColorMetallicEarnedDollarPremium { get; set; }

        [DisplayName("Fifth Color Match Open Dollar Premium")]
        [Range(00.00, 2000000000.00, ErrorMessage = "Fifth Color Match Open Dollar Premium must be $2,000,000,000.00 or less.")]
        public int? FifthColorMatchOpenDollarPremium { get; set; }

        [DisplayName("Fifth Color Match Earned Dollar Premium")]
        [Range(00.00, 2000000000.00, ErrorMessage = "Fifth Color Match Earned Dollar Premium must be $2,000,000,000.00 or less.")]
        public int? FifthColorMatchEarnedDollarPremium { get; set; }

        [DisplayName("Fifth Color PMS Open Dollar Premium")]
        [Range(00.00, 2000000000.00, ErrorMessage = "Fifth Color PMS Open Dollar Premium must be $2,000,000,000.00 or less.")]
        public int? FifthColorPMSOpenDollarPremium { get; set; }

        [DisplayName("Fifth Color PMS Earned Dollar Premium")]
        [Range(00.00, 2000000000.00, ErrorMessage = "Fifth Color PMS Earned Dollar Premium must be $2,000,000,000.00 or less.")]
        public int? FifthColorPMSEarnedDollarPremium { get; set; }
        public int? RateBaseCirculationGuarantee { get; set; }
        public int? AveragePrintRun { get; set; }

        public List<RateRateType> RateRateTypes { get; set; }
    }
}