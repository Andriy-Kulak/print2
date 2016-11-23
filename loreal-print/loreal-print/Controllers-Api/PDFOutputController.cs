using loreal_print.MEC_Common.PDFOutput;
using loreal_print.MEC_Common.Security;
using loreal_print.MEC_Common.Security.Principal;
using loreal_print.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace loreal_print.Controllers_Api
{
    public class PDFOutputController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("api/PDFOutput/ValidateAllInputs")]
        [Authorize]
        [HttpGet]
        public IHttpActionResult ValidateAllInputs()
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lInputs = new List<String>();
            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    lInputs = db.ValidateAllInputs(Convert.ToInt16(lCookieData.Year), lCookieData.BookID, lCookieData.VersionID)
                    .ToList();
                }

            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in ValidateAllInputs.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                return lResult = BadRequest(lReturnMessage);
            }

            if (lInputs.Any())
            {
                lResult = Ok(lInputs);
            }
            else
            {
                lResult = Ok();
            }
            return lResult;
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

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {

        }

        [HttpGet()]
        [Route("api/PDFOutput/PrintPDFOutput")]
        [Authorize]
        //public HttpResponseMessage PrintPDFOutput()
        //public System.Web.Mvc.ActionResult PrintPDFOutput()            
        public IHttpActionResult PrintPDFOutput()
        {
            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);
            var lLogMsg = string.Empty;
            var pdfService = new PDFService();

            try
            {
                // Print Output for given BookID
                var pdfModel = new PDFDocumentModel
                {
                    Book = lCookieData.Book,
                    BookID = Convert.ToInt32(lCookieData.BookID),
                    Publisher = lCookieData.Publisher,
                    PublisherID = Convert.ToInt32(lCookieData.PublisherID),
                    VersionID = lCookieData.VersionID,
                    Year = lCookieData.Year
                };

                var lResult = pdfService.BuildPDF(pdfModel, pContext);

                if (lResult.Any())
                {
                    // Set up the Response Stream for the File
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.ClearHeaders();
                    HttpContext.Current.Response.ClearContent();
                    HttpContext.Current.Response.Expires = -1000;
                    //HttpContext.Current.Response.BufferOutput = false;
                    HttpContext.Current.Response.ContentType = "application/pdf";
                    HttpContext.Current.Response.AddHeader("content-length", lResult.Length.ToString());
                    HttpContext.Current.Response.AddHeader("content-disposition", "inline; filename=" + "loreal_output.pdf");
                    HttpContext.Current.Response.BinaryWrite(lResult);
                    HttpContext.Current.Response.End();
                }

                //}
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in PrintPDFOutput.  BOOK: {0}, Publisher: {1}.", lCookieData.Book, lCookieData.Publisher);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            return Ok(HttpContext.Current.Response);
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}