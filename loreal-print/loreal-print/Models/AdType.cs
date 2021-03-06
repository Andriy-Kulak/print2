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
    
    public partial class AdType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AdType()
        {
            this.ProductionCostNEPs = new HashSet<ProductionCostNEP>();
            this.Rates = new HashSet<Rate>();
        }
    
        public string Year { get; set; }
        public int ParentAdTypeID { get; set; }
        public int AdTypeID { get; set; }
        public string AdType1 { get; set; }
        public System.DateTime Load_Date { get; set; }
    
        public virtual ParentAdType ParentAdType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductionCostNEP> ProductionCostNEPs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Rate> Rates { get; set; }
    }
}
