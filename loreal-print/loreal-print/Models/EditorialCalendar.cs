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
    
    public partial class EditorialCalendar
    {
        public int EditorialCalendarID { get; set; }
        public string Year { get; set; }
        public int BookID { get; set; }
        public string IssueDate { get; set; }
        public string EditorialTheme { get; set; }
        public System.DateTime OnSaleDate { get; set; }
        public System.DateTime MailToSubscribersDate { get; set; }
        public System.DateTime SpaceClosingDate_ROB { get; set; }
        public System.DateTime SpaceClosingDate_Covers { get; set; }
        public System.DateTime SpaceClosingDate_ScentStrips { get; set; }
        public System.DateTime MaterialsClosingDate_ROB { get; set; }
        public System.DateTime MaterialsClosingDate_Covers { get; set; }
        public System.DateTime MaterialsClosingDate_ScentStrips { get; set; }
        public int VersionID { get; set; }
        public System.DateTime Load_Date { get; set; }
    }
}