using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using loreal_print.Models;
using loreal_print.MEC_Common.Security.Principal;
using NLog;
using System.Web.Script.Serialization;
using System.Security.Principal;
using System.Collections.Generic;

namespace loreal_print.MEC_Common.Security
{
    internal static class CookieManagement
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        internal static void CreateCookie(HttpContext pContext, IPrincipal pUser, string pMail = "")
        {
            var lCheck = HttpContext.Current.User;
            var lUserId = pUser.Identity.GetUserId();
            var lEmail = (pMail != string.Empty ? pMail : pUser.Identity.Name);
            var lLogMsg = string.Empty;

            try
            {
                //Create Model for Cookie Data
                var serializeModel = new CustomPrincipalSerializerModel
                {
                    Id = lUserId,
                    Email = lEmail,
                    UserName = lEmail
                };

                //Serialize the Model
                var serializer = new JavaScriptSerializer();
                var userData = serializer.Serialize(serializeModel);

                //Create an Auth Ticket
                var authTicket = new FormsAuthenticationTicket(
                                1,
                                lEmail, //User
                                DateTime.Now, //When
                                DateTime.MaxValue, //Expires in
                                false,
                                // model.RememberMe, //Persistant? - we can add later
                                userData);

                //Encrypt the auth ticket to prevent Cookie Attacks
                var encTicket = FormsAuthentication.Encrypt(authTicket);

                //Add Cookie to the Context Reponse
                var faCookie = new HttpCookie(
                     "lorealPrint", encTicket);

                pContext.Response.Cookies.Add(faCookie);
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in CreateCookie for USER: {0} .", pUser.Identity.Name);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
        }

        internal static CustomPrincipalSerializerModel GetCookie(HttpContext pContext)
        {
            var lLogMsg = string.Empty;
            var lresult = new CustomPrincipalSerializerModel();
            var authCookie = pContext.Request.Cookies["lorealPrint"];

            try
            {
                // BUG FIX:  Handle empty cookie
                if (String.IsNullOrEmpty(authCookie.Value))
                {
                    var pUser = HttpContext.Current.User;
                    var lEmailAddress = pUser.Identity.Name;

                    // Repair Cookie
                    RepairCookie(HttpContext.Current, pUser);
                    //CreateCookie(HttpContext.Current, pUser);
                    authCookie = pContext.Request.Cookies["lorealPrint"];
                }
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                var serializer = new JavaScriptSerializer();
                var lCookieData = serializer.Deserialize<CustomPrincipalSerializerModel>(authTicket.UserData);

                lresult = lCookieData;
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in GetCookie.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            return lresult;
        }

        internal static bool RepairCookie(HttpContext pContext, IPrincipal pUser, CustomPrincipalSerializerModel pCookieData = null)
        {
            var lLogMsg = string.Empty;
            var lResult = false;
            var lUserId = pUser.Identity.GetUserId();
            var lEmail = pUser.Identity.Name;

            try
            {
                //Create Model for Cookie Data
                var serializeModel = new CustomPrincipalSerializerModel
                {
                    Id = lUserId,
                    Email = lEmail,
                    UserName = lEmail
                };

                // Add any additional information passed by pCookieData
                if (pContext != null)
                {
                    if (pCookieData.VersionID > 0)
                        serializeModel.VersionID = pCookieData.VersionID;
                    if (pCookieData.Book != string.Empty)
                        serializeModel.Book = pCookieData.Book;
                    if (pCookieData.BookID != null)
                        serializeModel.BookID = pCookieData.BookID;
                    if (pCookieData.Year != string.Empty)
                        serializeModel.Year = pCookieData.Year;
                    if (pCookieData.Publisher != string.Empty)
                        serializeModel.Publisher = pCookieData.Publisher;
                    if (pCookieData.PublisherID != null)
                        serializeModel.PublisherID = pCookieData.PublisherID;
                }

                var serializer = new JavaScriptSerializer();
                var userData = serializer.Serialize(serializeModel);

                //Repeair an Auth Ticket
                var authTicket = new FormsAuthenticationTicket(
                                1,
                                lEmail, //User
                                DateTime.Now, //When
                                DateTime.MaxValue, //Expires in
                                false,
                                // model.RememberMe, //Persistant? - we can add later
                                userData);

                //Encrypt the auth ticket to prevent Cookie Attacks
                var encTicket = FormsAuthentication.Encrypt(authTicket);

                //Set Cookie to the Context Reponse
                var faCookie = new HttpCookie(
                     "lorealPrint", encTicket);
                pContext.Response.Cookies.Set(faCookie);
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in RepairCookie for USER: {0}.", pUser.Identity.Name);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
            return lResult;
        }

        internal static bool UpdateCookie(HttpContext pContext, IPrincipal pUser, CustomPrincipalSerializerModel pCookieData)
        {
            var lResult = false;
            var lLogMsg = string.Empty;
            var authCookie = pContext.Request.Cookies["lorealPrint"];

            try
            {
                // BUG FIX:  Handle empty cookie
                if (String.IsNullOrEmpty(authCookie.Value))
                {
                    var isAuthenticated = pUser.Identity.IsAuthenticated;

                    if (isAuthenticated)
                    {
                        var res = RepairCookie(pContext, pUser, pCookieData);
                        authCookie = pContext.Request.Cookies["lorealPrint"];
                    }
                    else // Unrecoverable error - logoff and start over again.
                    {
                        return lResult;
                    }
                }

                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                var serializer = new JavaScriptSerializer();
                var lCookieData = serializer.Deserialize<CustomPrincipalSerializerModel>(authTicket.UserData);
                var lRememberMe = authTicket.IsPersistent;

                // Handle empty VersionID value
                var lVersionID = pCookieData.VersionID >= lCookieData.VersionID ? pCookieData.VersionID : lCookieData.VersionID;
                if (lVersionID < 1)
                    lVersionID = 1;

                // Create Model for Cookie Data
                var serializeModel = new CustomPrincipalSerializerModel
                {
                    Id = pCookieData.Id,
                    Email = pCookieData.Email != string.Empty ? pCookieData.Email : lCookieData.Email,
                    UserName = pCookieData.UserName != string.Empty ? pCookieData.UserName : lCookieData.UserName,
                    VersionID = lVersionID,
                    Book = pCookieData.Book != string.Empty ? pCookieData.Book : lCookieData.Book,
                    BookID = pCookieData.BookID != null ? pCookieData.BookID : lCookieData.BookID,
                    Year = pCookieData.Year != string.Empty ? pCookieData.Year : lCookieData.Year,
                    Publisher = pCookieData.Publisher != string.Empty ? pCookieData.Publisher : lCookieData.Publisher,
                    PublisherID = pCookieData.PublisherID != null ? pCookieData.PublisherID : lCookieData.PublisherID
                };

                // Serialize the Model
                serializer = new JavaScriptSerializer();
                var userData = serializer.Serialize(serializeModel);

                // Create an Auth Ticket
                authTicket = new FormsAuthenticationTicket(
                                authTicket.Version,
                                serializeModel.UserName, // User
                                authTicket.IssueDate, // When
                                authTicket.Expiration, // Expires in
                                lRememberMe, // Persistant?
                                userData);

                // Encrypt the auth ticket to prevent Cookie Attacks
                var encTicket = FormsAuthentication.Encrypt(authTicket);
                var faCookie = new HttpCookie(
                     "lorealPrint", encTicket);

                // TODO
                //If "Remember Me" has been selected, once user clicks the 'Log in' button, system shall store email/password. Selection shall be stored till user logs out
                if (authTicket.IsPersistent)
                {
                    faCookie.Expires = DateTime.MaxValue;
                }
                pContext.Response.Cookies.Set(faCookie);
                return true;
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in UpdateCookie for USER: {0}.", pUser.Identity.Name);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                return false;
            }
        }

        internal static bool DeleteCookie(HttpContext pContext)
        {
            var lResult = true;
            var lLogMsg = string.Empty;

            try
            {
                var authCookie = pContext.Request.Cookies["lorealPrint"];

                // Remove cookie when logging out.
                if (authCookie != null)
                {
                    //HttpCookie myCookie = new HttpCookie("lorealPrint");
                    //myCookie.Expires = DateTime.Now.AddDays(-1d);
                    //HttpContext.Current.Response.Cookies.Add(myCookie);

                    // Try this.
                    HttpContext.Current.Response.Cookies["lorealPrint"].Expires = DateTime.Now.AddDays(-1);
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in DeleteCookie.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = false;
            }
            return lResult;
        }

        internal static string GetPublisher(int? pPublisherID)
        {
            var lLogMsg = string.Empty;
            var lPublisher = string.Empty;

            try
            {
                using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
                {
                    lPublisher = db.Publishers
                        .Where(p => p.PublisherID == pPublisherID)
                        .Select(p => p.Publisher1)
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in GetPublisher for PublisherID: {0}.", pPublisherID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
            return lPublisher;
        }

        internal static VersionInfoModel SaveBookInfoToCookie(int pBookID, IPrincipal pUser)
        {
            // Get BookID, VersionID & Year from Cookie
            var lLogMsg = string.Empty;
            var pContext = HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);
            var lVersionInfo = new VersionInfoModel();
            var lYear = lCookieData.Year;
            var cookieData = new CustomPrincipalSerializerModel();

            try
            {
                using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
                {
                    var lBook = db.Books
                        .Where(b => b.BookID == pBookID && b.Year == lYear).SingleOrDefault();

                    // Get Version and Status information; Update cookie with this and pass back to client.
                    var lVersions = GetVersionInfo(pBookID, lYear);
                    if (lVersions.Any())
                        lVersions = lVersions.OrderByDescending(v => v.VersionID).ToList();
                    var lLatestVersion = lVersions.FirstOrDefault();

                    // Update Cookie
                    cookieData.BookID = pBookID;
                    cookieData.Book = lBook.Book1;
                    cookieData.VersionID = lLatestVersion.VersionID;
                    cookieData.Status = lLatestVersion.Status;

                    var lbool = CookieManagement.UpdateCookie(HttpContext.Current, pUser, cookieData);

                    // Populate VersionInfoModel
                    lVersionInfo.Status = lLatestVersion.Status;
                    lVersionInfo.VersionID = lLatestVersion.VersionID;
                    lVersionInfo.VersionList = lVersions;
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in SaveBookInfoToCookie.  BookID: {0}, User: {1}.", pBookID, pUser.Identity.Name);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
            return lVersionInfo;
        }

        internal static List<BookToVersion> GetVersionInfo(int pBookID, string pYear)
        {
            var lLogMsg = string.Empty;
            var lBookVersion = new List<BookToVersion>();
            const string STATUS = "In Progress";

            try
            {
                using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
                {
                    lBookVersion = db.BookToVersions
                        .Where(a => a.BookID == pBookID && a.Year == pYear)
                        .ToList();

                    if (!lBookVersion.Any())
                    {
                        // Call CreateNewVersion SP
                        var lVersionNum = db.CreateNewVersion(pYear, pBookID);
                        var lBook = new BookToVersion
                        {
                            BookID = pBookID,
                            VersionID = lVersionNum,
                            Year = pYear,
                            Status = STATUS
                        };
                        lBookVersion.Add(lBook);

                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in GetVersionInfo.  BookID: {0}, Year: {1}.", pBookID, pYear);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
            return lBookVersion;
        }
    }
}