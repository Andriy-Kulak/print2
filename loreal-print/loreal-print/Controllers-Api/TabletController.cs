using loreal_print.MEC_Common.Security;
using loreal_print.Models;
using loreal_print.Models.Tablet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using System.Net.Http;
using System.Web.Http;

namespace loreal_print.Controllers_Api
{
    public class TabletController : ApiController
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

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        [Route("api/Tablet/GetTabletRates")]
        [HttpGet]
        public IHttpActionResult GetTabletRates()
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lTableRatesOutput = new List<TabletModel>();

            try
            {
                var ReposTablet = new Repository.Tablet();
                lTableRatesOutput = ReposTablet.GetTabletRates();
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in Tablet GetTabletRates method.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }
            if (lTableRatesOutput.Any())
            {
                lResult = Ok(lTableRatesOutput);
            }
            else
            {
                lResult = Ok();
            }
            return lResult;
        }

        [Route("api/Circulation/saveTabletRates/rates")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult saveTabletRates(List<TabletModel> rates)
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lTabletRateID = int.MinValue;
            var lTabletFunctionalityID = int.MinValue;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            if (ModelState.IsValid)
            {
                try
                {
                    using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                    {
                        foreach (var rate in rates)
                        {
                            lTabletFunctionalityID = rate.TabletFunctionalityID; // For reference in case of error.
                            var lTabletRateRecord = db.TabletRates.Where(t => (t.Year == lCookieData.Year
                                && t.BookID == lCookieData.BookID
                                && t.VersionID == lCookieData.VersionID
                                && t.TabletFunctionalityID == lTabletFunctionalityID
                               )).FirstOrDefault();

                            if (lTabletRateRecord != null)
                            {
                                // Update
                                if (rate.EarnedRate >= 0)
                                    lTabletRateRecord.EarnedRate = rate.EarnedRate;
                                if (rate.OpenRate >= 0)
                                    lTabletRateRecord.OpenRate = rate.OpenRate;
                                lTabletRateRecord.VersionID = lCookieData.VersionID;
                            }
                            else
                            {
                                // Create
                                var lNewTabletRateRecord = new TabletRate
                                {
                                    Year = lCookieData.Year,
                                    BookID = Convert.ToInt32(lCookieData.BookID),
                                    VersionID = lCookieData.VersionID,
                                    TabletFunctionalityID = lTabletFunctionalityID,
                                    Load_Date = DateTime.Now
                                };
                                if (rate.EarnedRate >= 0)
                                    lNewTabletRateRecord.EarnedRate = rate.EarnedRate;
                                if (rate.OpenRate >= 0)
                                    lNewTabletRateRecord.OpenRate = rate.OpenRate;
                                lNewTabletRateRecord.VersionID = lCookieData.VersionID;

                                db.TabletRates.Add(lNewTabletRateRecord);
                            }
                        }
                        lTabletRateID = db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    var lReturnMessage = String.Format("Error in saveTabletRates.  TabletFunctionalityID: {0}, Publisher: {1}, Book: {2}, BookID: {3}.",
                        lTabletFunctionalityID, lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                    lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                    logger.Error(lLogMsg);
                    lResult = BadRequest(lReturnMessage);
                }
            }
            else
            {
                string errorMessages = string.Join("; ", ModelState.Values
                                                        .SelectMany(e => e.Errors)
                                                        .Select(e => e.ErrorMessage));

                return lResult = BadRequest(errorMessages);
            }

            // If we got to this point the operation was successful
            if (lTabletRateID > int.MinValue)
            {
                lResult = Ok(lTabletRateID);
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
        }
    }
}