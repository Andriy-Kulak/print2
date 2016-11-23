using loreal_print.MEC_Common.Security.Principal;
using loreal_print.Models;
using loreal_print.ViewModels;
using loreal_print.MEC_Common.Security;
using Microsoft.AspNet.Identity;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace loreal_print.Controllers_Api
{
    public class BooksController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [HttpGet()]
        [Route("api/Books/GetBookYears")]
        public IHttpActionResult GetBookYears()
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lstYears = new BooksViewModel().Years;

            try
            {
                using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
                {
                    var years = db.Books
                        .Select(b => b.Year)
                        .Distinct()
                        .ToList();

                    if (years.Any())
                    {
                        foreach (var item in years)
                        {
                            var bkYear = new BookYear();
                            bkYear.Year = item;
                            bkYear.YearId = Convert.ToInt16(item);
                            lstYears.Add(bkYear);
                        }

                        lResult = Ok(lstYears);
                    }
                    else
                    {
                        lResult = Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in GetBookYears.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }
            return lResult;
        }

        // PUT api/<controller>/5
        [HttpPut()]
        public void Put(int id, [FromBody]string value)
        {
            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lLogMsg = string.Empty;
            var lName = String.Empty;

            // Update Cookie
            var lCookieData = new CustomPrincipalSerializerModel();
            lCookieData.Book = lName;
            lCookieData.BookID = id;

            try
            {

                // Get Book Name
                using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
                {
                    lName = db.Books
                        .Where(b => b.BookID == id)
                        .Select(B => B.Book1).FirstOrDefault();
                }

                var lbool = CookieManagement.UpdateCookie(System.Web.HttpContext.Current, User, lCookieData);
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in saveBook (Book Put).  BOOK: {0}, Publisher: {1}.", lCookieData.Publisher, id);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
        }

        [HttpGet()]
        [Route("api/Books/GetBooks/{year}")]
        [Authorize]
        public IHttpActionResult GetBooks(string year)
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lstBooks = new List<BookViewModel>();
            var userId = User.Identity.GetUserId();
            int? publisherID = 0;
            var lCookieData = new CustomPrincipalSerializerModel();

            try
            {
                using (Loreal_DEVEntities5 db = new Loreal_DEVEntities5())
                {
                    publisherID = db.AspNetUsers
                        .Where(a => a.Id == userId)
                        .Select(a => a.PublisherID)
                        .SingleOrDefault();
                }

                // Update Cookie            
                lCookieData.Id = userId;
                lCookieData.Year = year;
                lCookieData.PublisherID = publisherID;
                lCookieData.Publisher = CookieManagement.GetPublisher(publisherID);
                var lbool = CookieManagement.UpdateCookie(System.Web.HttpContext.Current, User, lCookieData);

                // We're in a weird zombie cookie state.  Logoff.
                //if (!lbool)
                //{
                //    //return Redirect("/Account/LogOff");
                //    var newUrl = this.Url.Link("Default", new
                //    {
                //        Controller = "Account",
                //        Action = "LogOff"
                //    });
                //    return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, RedirectUrl = newUrl });
                //}

                using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
                {
                    var books = db.Books
                        .Where(b => b.Year == year && b.PublisherID == publisherID)
                        .OrderBy(b => b.Book1)
                        .ToList();

                    if (books.Any())
                    {
                        foreach (var book in books)
                        {
                            var lbook = new BookViewModel();
                            lbook.ID = book.BookID;
                            lbook.Name = book.Book1;

                            lstBooks.Add(lbook);
                        }
                        lResult = Ok(lstBooks);
                    }
                    else
                    {
                        lResult = Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in GetBooks. Year: {0} Publisher: {1}, Book: {2}, BookID: {3}.", year, lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }
            return lResult;
        }
    }
}
