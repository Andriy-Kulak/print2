using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace loreal_print.Models.Tablet
{
    public class TabletModel
    {
        public int TabletFunctionalityID { get; set; }
        public string TabletParentFunctionality { get; set; }
        public string TabletFunctionality { get; set; }

        [Range(00, int.MaxValue, ErrorMessage = "Earned Rate must be an integer greater than 0.")]
        public int? EarnedRate { get; set; }

        [Range(00, int.MaxValue, ErrorMessage = "Open Rate must be an integer greater than 0.")]
        public int? OpenRate { get; set; }
    }
}