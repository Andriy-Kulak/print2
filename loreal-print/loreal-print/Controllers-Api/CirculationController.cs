using loreal_print.MEC_Common.Security;
using loreal_print.Models;
using loreal_print.Models.CirculationOutput;
using NLog;
using loreal_print.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace loreal_print.Controllers_Api
{
    public class CirculationController : ApiController
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

        [Route("api/circulation/GetCirculation")]
        [HttpGet]
        public IHttpActionResult GetCirculation()
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var newCirculationOutput = new CirculationModel();

            try
            {
                var ReposCirc = new Repository.Circulation();
                newCirculationOutput = ReposCirc.GetCirculation();
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in GetCirculation.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }

            if (newCirculationOutput.CirculationGroups.Any())
                lResult = Ok(newCirculationOutput);
            else
                lResult = Ok();

            return lResult;
        }


        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        [Route("api/Circulation/saveCirculationRecords/CircContent")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult saveCirculationRecords(List<CirculationContent> CircContent)
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lCircID = int.MinValue;
            var lCirculationSubTypeID = int.MinValue;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            if (ModelState.IsValid)
            {
                try
                {
                    using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                    {
                        foreach (var circ in CircContent)
                        {
                            lCirculationSubTypeID = circ.CirculationSubTypeID; // For reference in case of error.
                            var lCircRecord = db.Circulations.Where(c => (c.Year == lCookieData.Year
                                && c.BookID == lCookieData.BookID
                                && c.VersionID == lCookieData.VersionID
                                && c.CirculationSubTypeID == lCirculationSubTypeID
                               )).FirstOrDefault();

                            if (lCircRecord != null)
                            {
                                // Update
                                lCircRecord.DigitalNonReplicaCirculation = circ.DigitalNonReplicaCirculation;
                                lCircRecord.DigitalReplicaCirculation = circ.DigitalReplicaCirculation;
                                lCircRecord.PrintCirculation = circ.PrintCirculation;
                                lCircRecord.VersionID = lCookieData.VersionID;
                            }
                            else
                            {
                                // Create
                                var lNewCircRecord = new Models.Circulation
                                {
                                    Year = lCookieData.Year,
                                    BookID = Convert.ToInt32(lCookieData.BookID),
                                    VersionID = lCookieData.VersionID,
                                    CirculationSubTypeID = lCirculationSubTypeID,
                                    Load_Date = DateTime.Now
                                };
                                if (circ.PrintCirculation != null)
                                    lNewCircRecord.PrintCirculation = circ.PrintCirculation;
                                if (circ.DigitalNonReplicaCirculation != null)
                                    lNewCircRecord.DigitalNonReplicaCirculation = circ.DigitalNonReplicaCirculation;
                                if (circ.DigitalReplicaCirculation != null)
                                    lNewCircRecord.DigitalReplicaCirculation = circ.DigitalReplicaCirculation;

                                db.Circulations.Add(lNewCircRecord);
                            }
                        }
                        lCircID = db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    var lReturnMessage = String.Format("Error in saveCirculationRecords.  CirculationSubTypeID: {0}, Publisher: {1}, Book: {2}, BookID: {3}.",
                        lCirculationSubTypeID, lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
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

            if (lCircID > int.MinValue)
            {
                lResult = Ok(lCircID);
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