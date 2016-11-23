using loreal_print.Models.EditorialCalendarOutput;

using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using loreal_print.MEC_Common.Security;
using loreal_print.Models;

namespace loreal_print.Controllers_Api
{
    public class EditorialCalendarController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        [Route("api/EditorialCalendar/GetCalendar")]
        [HttpGet]
        public IHttpActionResult GetCalendar()
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lEditorialCalendarOutput = new List<EditorialCalendarModel>();

            try
            {
                var ReposCal = new Repository.EditorialCalendar();
                lEditorialCalendarOutput = ReposCal.GetCalendar();
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in EditorialCalendar GetCalendar method.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }
            if (lEditorialCalendarOutput.Any())
            {
                lResult = Ok(lEditorialCalendarOutput);
            }
            else
            {
                lResult = Ok();
            }
            return lResult;
        }

        [Route("api/EditorialCalendar/saveEditorialCalendar/calendarRecord")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult saveEditorialCalendar(EditorialCalendarModel calendarRecord)
        {
            /* There is no way to guarantee uniqueness.  Therefore, whenever changes/additions are made:
             * 1. All records are sent up
             * 2. All existing records are DELETED
             * 3. All records included in the calendarRecords collection are INSERTED */

            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lEditorialCalendarID = int.MinValue;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            // Verify Incoming Date Values passed as Strings
            var isValidDate = calendarRecord.ValidateDateFields(calendarRecord);

            if (ModelState.IsValid && isValidDate)
            {
                if (calendarRecord != null)
                {
                    try
                    {
                        using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                        {
                            if (calendarRecord.EditorialCalendarID > 0)
                            {
                                // Update Existing record
                                var lUpdateCalRecord = db.EditorialCalendars.Find(calendarRecord.EditorialCalendarID);

                                lUpdateCalRecord.Year = lCookieData.Year;
                                lUpdateCalRecord.BookID = Convert.ToInt32(lCookieData.BookID);
                                lUpdateCalRecord.VersionID = lCookieData.VersionID;
                                lUpdateCalRecord.IssueDate = calendarRecord.IssueDate;
                                lUpdateCalRecord.EditorialTheme = calendarRecord.EditorialTheme;
                                lUpdateCalRecord.OnSaleDate = Convert.ToDateTime(calendarRecord.OnSaleDate);
                                lUpdateCalRecord.MailToSubscribersDate = Convert.ToDateTime(calendarRecord.MailToSubscribersDate);
                                lUpdateCalRecord.SpaceClosingDate_ROB = Convert.ToDateTime(calendarRecord.SpaceClosingDateROB);
                                lUpdateCalRecord.SpaceClosingDate_Covers = Convert.ToDateTime(calendarRecord.SpaceClosingDateCovers);
                                lUpdateCalRecord.SpaceClosingDate_ScentStrips = Convert.ToDateTime(calendarRecord.SpaceClosingDateScentStrips);
                                lUpdateCalRecord.MaterialsClosingDate_ROB = Convert.ToDateTime(calendarRecord.MaterialsClosingDateROB);
                                lUpdateCalRecord.MaterialsClosingDate_Covers = Convert.ToDateTime(calendarRecord.MaterialsClosingDateCovers);
                                lUpdateCalRecord.MaterialsClosingDate_ScentStrips = Convert.ToDateTime(calendarRecord.MaterialsClosingDateScentStrips);
                            }
                            else
                            {
                                // Create
                                var lNewCalRecord = new EditorialCalendar
                                {
                                    Year = lCookieData.Year,
                                    BookID = Convert.ToInt32(lCookieData.BookID),
                                    VersionID = lCookieData.VersionID,
                                    IssueDate = calendarRecord.IssueDate,
                                    EditorialTheme = calendarRecord.EditorialTheme,
                                    OnSaleDate = Convert.ToDateTime(calendarRecord.OnSaleDate),
                                    MailToSubscribersDate = Convert.ToDateTime(calendarRecord.MailToSubscribersDate),
                                    SpaceClosingDate_ROB = Convert.ToDateTime(calendarRecord.SpaceClosingDateROB),
                                    SpaceClosingDate_Covers = Convert.ToDateTime(calendarRecord.SpaceClosingDateCovers),
                                    SpaceClosingDate_ScentStrips = Convert.ToDateTime(calendarRecord.SpaceClosingDateScentStrips),
                                    MaterialsClosingDate_ROB = Convert.ToDateTime(calendarRecord.MaterialsClosingDateROB),
                                    MaterialsClosingDate_Covers = Convert.ToDateTime(calendarRecord.MaterialsClosingDateCovers),
                                    MaterialsClosingDate_ScentStrips = Convert.ToDateTime(calendarRecord.MaterialsClosingDateScentStrips),

                                    Load_Date = DateTime.Now
                                };
                                db.EditorialCalendars.Add(lNewCalRecord);
                            }
                            //var lCalRecords = db.EditorialCalendars.Where(c => (c.Year == lCookieData.Year
                            //                    && c.BookID == lCookieData.BookID
                            //                    && c.VersionID == lCookieData.VersionID
                            //                   )).ToList();

                            //if (lCalRecords.Any())
                            //{
                            //    // Delete all existing records.
                            //    //obj.tblA.Where(x => x.fid == i).ToList().ForEach(obj.tblA.DeleteObject);
                            //    //obj.SaveChanges();
                            //    foreach (var calRecord in lCalRecords)
                            //    {
                            //        db.EditorialCalendars.Remove(calRecord);
                            //    }
                            //}
                            //lCircID = db.SaveChanges();

                            // Create All records sent
                            //foreach (var calItem in calendarRecords)
                            //{

                            //}
                            lEditorialCalendarID = db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        var lReturnMessage = String.Format("Error in saveEditorialCalendar.  Publisher: {0}, Book: {1}, BookID: {2}.",
                            lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                        lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                        logger.Error(lLogMsg);
                        lResult = BadRequest(lReturnMessage);
                    }
                }
            }
            else
            {
                string errorMessages = string.Empty;

                if (ModelState.Values.Any())
                {
                    errorMessages = string.Join("; ", ModelState.Values
                                                            .SelectMany(e => e.Errors)
                                                            .Select(e => e.ErrorMessage));
                }
                else
                {
                    errorMessages = calendarRecord.EditorialErrorMsg;
                }               
                return BadRequest(errorMessages);
            }

            if (lEditorialCalendarID > int.MinValue)
            {
                lResult = Ok(lEditorialCalendarID);
            }
            else
            {
                lResult = Ok();
            }
            return lResult;
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
            using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
            {
                //var lCalRecord = db.EditorialCalendars.Find(id);
                var lCalRecord = new EditorialCalendar();
                lCalRecord.EditorialCalendarID = id;

                db.EditorialCalendars.Attach(lCalRecord);
                db.EditorialCalendars.Remove(lCalRecord);
                db.SaveChanges();
            }
        }
    }
}