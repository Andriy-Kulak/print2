using loreal_print.MEC_Common.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace loreal_print.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var isauthCookie = false;
            var isResponseCookie = false;

            // If user is already logged in (They have a cookie) take them straight to the Book page.
            var returnUrl = "";
            var authCookie = HttpContext.Request.Cookies["lorealPrint"];
            if (authCookie == null)
            {
                returnUrl = "/Account/Login";
            }
            if (authCookie != null)
            {
                isauthCookie = !String.IsNullOrEmpty(authCookie.Value);
                isResponseCookie = (HttpContext.Response.Cookies["lorealPrint"] != null);

                if (isauthCookie)
                {
                    returnUrl = "/print/books";
                    //return Redirect(returnUrl);
                }
                else if (!isauthCookie && isResponseCookie)
                {
                    /*  We're in a zombie state.  First loggoff user (which will delete cookie).
                     *  This, in turn, will then redirect the user to the login page. */
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                    var lResult = CookieManagement.DeleteCookie(System.Web.HttpContext.Current);

                    return RedirectToAction("Login", "Account");
                }
            }
            return Redirect(returnUrl);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
    }
}