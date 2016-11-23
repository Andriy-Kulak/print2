using System;
using System.Collections.Generic;
using System.Linq;
using loreal_print.Models;
using loreal_print.Models.EditorialCalendarOutput;
using NLog;
using System.Web;
using loreal_print.MEC_Common.Security;

namespace loreal_print.Repository
{    
    public class EditorialCalendar
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public List<EditorialCalendarModel> GetCalendar()
        {
            var lResult = new List<EditorialCalendarModel>();
            var lLogMsg = string.Empty;
            var lEditorialCalendarOutput = new List<EditorialCalendarModel>();
            var lstEditorialRecords = new List<loreal_print.Models.EditorialCalendar>();

            var pContext = HttpContext.Current;

            // Get BookID, VersionID & Year from Cookie
            var lCookieData = CookieManagement.GetCookie(pContext);

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    lstEditorialRecords = db.EditorialCalendars
                        .Where(e => e.BookID == lCookieData.BookID && e.Year == lCookieData.Year && e.VersionID == lCookieData.VersionID)
                        .ToList();

                    if (lstEditorialRecords.Any())
                    {
                        foreach (var calItem in lstEditorialRecords)
                        {
                            //recordCount++;

                            var newEditorialCalendarOutput = new EditorialCalendarModel
                            {
                                EditorialCalendarID = calItem.EditorialCalendarID,
                                BookID = Convert.ToInt32(lCookieData.BookID),
                                Year = lCookieData.Year,
                                VersionID = lCookieData.VersionID,
                                IssueDate = calItem.IssueDate,
                                EditorialTheme = calItem.EditorialTheme,
                                OnSaleDate = calItem.OnSaleDate.ToString("MM/dd/yyyy"),
                                MailToSubscribersDate = calItem.MailToSubscribersDate.ToString("MM/dd/yyyy"),
                                SpaceClosingDateROB = calItem.SpaceClosingDate_ROB.ToString("MM/dd/yyyy"),
                                SpaceClosingDateCovers = calItem.SpaceClosingDate_Covers.ToString("MM/dd/yyyy"),
                                SpaceClosingDateScentStrips = calItem.SpaceClosingDate_ScentStrips.ToString("MM/dd/yyyy"),
                                MaterialsClosingDateROB = calItem.MaterialsClosingDate_ROB.ToString("MM/dd/yyyy"),
                                MaterialsClosingDateCovers = calItem.MaterialsClosingDate_Covers.ToString("MM/dd/yyyy"),
                                MaterialsClosingDateScentStrips = calItem.MaterialsClosingDate_ScentStrips.ToString("MM/dd/yyyy")
                            };
                            lEditorialCalendarOutput.Add(newEditorialCalendarOutput);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in Repository EditorialCalendar GetCalendar method.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                throw new Exception(lReturnMessage);
            }
            if (lEditorialCalendarOutput.Any())
            {
                lResult = lEditorialCalendarOutput;
            }

            return lResult;
        }
    }
}