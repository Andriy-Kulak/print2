//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace loreal_print.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductionCostNEP
    {
        public int ProductionCostNEPID { get; set; }
        public string Year { get; set; }
        public int BookID { get; set; }
        public int AdTypeID { get; set; }
        public int EditionTypeID { get; set; }
        public Nullable<int> OpenProductionCost { get; set; }
        public Nullable<int> EarnedProductionCost { get; set; }
        public Nullable<decimal> EarnedNEP { get; set; }
        public int VersionID { get; set; }
        public System.DateTime Load_Date { get; set; }
    
        public virtual AdType AdType { get; set; }
        public virtual EditionType EditionType { get; set; }
    }
}
