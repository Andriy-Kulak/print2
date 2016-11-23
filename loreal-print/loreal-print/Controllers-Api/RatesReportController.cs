using loreal_print.MEC_Common.Security;
using loreal_print.Models;
using loreal_print.Models.RatesReport;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace loreal_print.Controllers_Api
{
    public class RatesReportController : ApiController
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

        [Route("api/RatesReport/GetRatesReport")]
        [HttpGet]
        public IHttpActionResult GetRatesReport()
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var newRatesReportOutput = new RatesReportModel();

            try
            {
                var ReposRatesReport = new Repository.RatesReport();
                newRatesReportOutput = ReposRatesReport.GetRatesReport();
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in GetRatesReport.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }
            if (newRatesReportOutput.RateTypes.Any())
            {
                lResult = Ok(newRatesReportOutput);
            }
            else
            {
                lResult = Ok();
            }
            return lResult;
        }

        [Route("api/RatesReport/savePublisherAgreements/answers")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult savePublisherAgreements(List<PublisherAgreement> answers)
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lRateID = int.MinValue;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);
            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    foreach (var answer in answers)
                    {
                        var lRates = db.Rates.Where(r => (r.Year == lCookieData.Year
                            && r.BookID == lCookieData.BookID
                            && r.VersionID == lCookieData.VersionID
                            && r.RateTypeID == answer.RateTypeID
                            && r.AdTypeID == answer.AdTypeID
                            && r.EditionTypeID == answer.EditionTypeID)).ToList();

                        if (lRates.Any())
                        {
                            // Update
                            foreach (var lRate in lRates)
                            {
                                if (answer.Answer != string.Empty)
                                    lRate.PublisherAgreement = answer.Answer;
                            }                            
                        }
                    }
                    lRateID = db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in savePublisherAgreements.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }

            if (lRateID > int.MinValue)
            {
                lResult = Ok(lRateID);
            }
            else
            {
                lResult = Ok();
            }
            return lResult;
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
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