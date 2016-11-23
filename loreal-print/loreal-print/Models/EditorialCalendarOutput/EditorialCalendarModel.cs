using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace loreal_print.Models.EditorialCalendarOutput
{
    public class EditorialCalendarModel
    {
        public EditorialCalendarModel()
        {
            Init();
        }

        private void Init()
        {
            Book = string.Empty;
            Publisher = string.Empty;
            Year = string.Empty;
            IssueDate = string.Empty;
            EditorialTheme = string.Empty;
            EditorialErrorMsg = string.Empty;
        }
        public int EditorialCalendarID { get; set; }
        public bool Delete { get; set; }

        public int BookID { get; set; }
        public string Book { get; set; }
        public string Publisher { get; set; }
        public string Year { get; set; }
        [Required]
        [DisplayName("Issue Date")]
        public string IssueDate { get; set; }

        [Required]
        [DisplayName("Editorial Theme")]
        public string EditorialTheme { get; set; }

        [Required]
        [DisplayName("On Sale Date")]
        [DataType(DataType.Date)]
        public string OnSaleDate { get; set; }

        [Required]
        [DisplayName("Mail To Subscribers Date")]
        [DataType(DataType.Date)]
        public string MailToSubscribersDate { get; set; }

        [Required]
        [DisplayName("Space Closing Date - ROB")]
        [DataType(DataType.Date)]
        public string SpaceClosingDateROB { get; set; }

        [Required]
        [DisplayName("Space Closing Date - Covers")]
        [DataType(DataType.Date)]
        public string SpaceClosingDateCovers { get; set; }

        [Required]
        [DisplayName("Space Closing Date - Scent Strips")]
        [DataType(DataType.Date)]
        public string SpaceClosingDateScentStrips { get; set; }

        [Required]
        [DisplayName("Materials Closing Date - ROB")]
        [DataType(DataType.Date)]
        public string MaterialsClosingDateROB { get; set; }

        [Required]
        [DisplayName("Materials Closing Date - Covers")]
        [DataType(DataType.Date)]
        public string MaterialsClosingDateCovers { get; set; }

        [Required]
        [DisplayName("Materials Closing Date - Scent Strips")]
        [DataType(DataType.Date)]
        public string MaterialsClosingDateScentStrips { get; set; }
        public int VersionID { get; set; }

        public string EditorialErrorMsg { get; set; }

        private DateTime _OnSaleDate;
        private DateTime _MailToSubscribersDate;
        private DateTime _SpaceClosingDateROB;
        private DateTime _SpaceClosingDateCovers;
        private DateTime _SpaceClosingDateScentStrips;
        private DateTime _MaterialsClosingDateROB;
        private DateTime _MaterialsClosingDateCovers;
        private DateTime _MaterialsClosingDateScentStrips;

        public bool ValidateDateFields(EditorialCalendarModel calendarRecord)
        {
            var lResult = false;
            var isValidDate = true;
            var TmpMsg = string.Empty;

            isValidDate = DateTime.TryParse(calendarRecord.OnSaleDate, out _OnSaleDate);
            if (!isValidDate)
            {
                TmpMsg = String.Format("OnSaleDate {0} is not a valid date.", calendarRecord.OnSaleDate);
                EditorialErrorMsg = EditorialErrorMsg.Length > 0 ? EditorialErrorMsg += "; " + TmpMsg : TmpMsg;
            }

            isValidDate = DateTime.TryParse(calendarRecord.MailToSubscribersDate, out _MailToSubscribersDate);
            if (!isValidDate)
            {
                TmpMsg = String.Format("MailToSubscribersDate {0} is not a valid date.", calendarRecord.MailToSubscribersDate);
                EditorialErrorMsg = EditorialErrorMsg.Length > 0 ? EditorialErrorMsg += "; " + TmpMsg : TmpMsg;
            }

            isValidDate = DateTime.TryParse(calendarRecord.SpaceClosingDateROB, out _SpaceClosingDateROB);
            if (!isValidDate)
            {
                TmpMsg = String.Format("SpaceClosingDateROB {0} is not a valid date.", calendarRecord.SpaceClosingDateROB);
                EditorialErrorMsg = EditorialErrorMsg.Length > 0 ? EditorialErrorMsg += "; " + TmpMsg : TmpMsg;
            }

            isValidDate = DateTime.TryParse(calendarRecord.SpaceClosingDateCovers, out _SpaceClosingDateCovers);
            if (!isValidDate)
            {
                TmpMsg = String.Format("SpaceClosingDateCovers {0} is not a valid date.", calendarRecord.SpaceClosingDateCovers);
                EditorialErrorMsg = EditorialErrorMsg.Length > 0 ? EditorialErrorMsg += "; " + TmpMsg : TmpMsg;
            }

            isValidDate = DateTime.TryParse(calendarRecord.SpaceClosingDateScentStrips, out _SpaceClosingDateScentStrips);
            if (!isValidDate)
            {
                TmpMsg = String.Format("SpaceClosingDateScentStrips {0} is not a valid date.", calendarRecord.SpaceClosingDateScentStrips);
                EditorialErrorMsg = EditorialErrorMsg.Length > 0 ? EditorialErrorMsg += "; " + TmpMsg : TmpMsg;
            }

            isValidDate = DateTime.TryParse(calendarRecord.MaterialsClosingDateROB, out _MaterialsClosingDateROB);
            if (!isValidDate)
            {
                TmpMsg = String.Format("MaterialsClosingDateROB {0} is not a valid date.", calendarRecord.MaterialsClosingDateROB);
                EditorialErrorMsg = EditorialErrorMsg.Length > 0 ? EditorialErrorMsg += "; " + TmpMsg : TmpMsg;
            }

            isValidDate = DateTime.TryParse(calendarRecord.MaterialsClosingDateCovers, out _MaterialsClosingDateCovers);
            if (!isValidDate)
            {
                TmpMsg = String.Format("MaterialsClosingDateCovers {0} is not a valid date.", calendarRecord.MaterialsClosingDateCovers);
                EditorialErrorMsg = EditorialErrorMsg.Length > 0 ? EditorialErrorMsg += "; " + TmpMsg : TmpMsg;
            }

            isValidDate = DateTime.TryParse(calendarRecord.MaterialsClosingDateScentStrips, out _MaterialsClosingDateScentStrips);
            if (!isValidDate)
            {
                TmpMsg = String.Format("MaterialsClosingDateScentStrips {0} is not a valid date.", calendarRecord.MaterialsClosingDateScentStrips);
                EditorialErrorMsg = EditorialErrorMsg.Length > 0 ? EditorialErrorMsg += "; " + TmpMsg : TmpMsg;
            }
            lResult = isValidDate;

            return lResult;
        }
    }

    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is bool && (bool)value;
        }
    }
}