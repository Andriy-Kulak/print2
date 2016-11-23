using loreal_print.MEC_Common.PDFOutput;
using loreal_print.MEC_Common.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using System.Web;
using System.Web.Mvc;

namespace loreal_print.Controllers
{
    public class PDFOutputController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET: PDFOutput
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet()]
        [Route("PDFOutput/GeneratePDFOutput")]
        [Authorize]
        public ActionResult GeneratePDFOutput()
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
                    // Set Up the Response Stream for the Spec Sheet Collection
                    System.Web.HttpContext.Current.Response.Clear();
                    System.Web.HttpContext.Current.Response.BufferOutput = false;
                    System.Web.HttpContext.Current.Response.ContentType = "application/pdf";
                    System.Web.HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + "loreal_output.pdf");
                    // Create a FileStream for the PDF and Send it to the Response's OutputStream
                    using (var outStream = new FileStream(pdfService.FilePath, FileMode.Open))
                    {
                        byte[] buffer = new byte[4096];
                        int count = 0;
                        while ((count = outStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            System.Web.HttpContext.Current.Response.OutputStream.Write(buffer, 0, count);
                            System.Web.HttpContext.Current.Response.Flush();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in PrintPDFOutput.  BOOK: {0}, Publisher: {1}.", lCookieData.Book, lCookieData.Publisher);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            return View();
        }
    }
}