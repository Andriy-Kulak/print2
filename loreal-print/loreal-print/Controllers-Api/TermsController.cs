using loreal_print.MEC_Common.Security;
using loreal_print.MEC_Common.Security.Principal;
using loreal_print.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace loreal_print.Controllers_Api
{
    public class TermsController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET api/<controller>
        public IHttpActionResult Get()
        {
            // Update Cookie wth bookID
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);
            CookieManagement.SaveBookInfoToCookie(Convert.ToInt32(lCookieData.BookID), User);

            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            QuestionModel vm = new QuestionModel();

            vm.Get(System.Web.HttpContext.Current);
            if (vm.SectionsListModel.SectionList.Any())
            {
                lResult = Ok(vm.SectionsListModel.SectionList);
            }
            else
            {
                lResult = Ok();
            }
            return lResult;
        }


        [HttpGet()]
        [Route("api/Terms/GetAnswers")]
        [Authorize]
        public IHttpActionResult GetAnswers()
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lstAnswers = new List<Answer>();
            var pContext = System.Web.HttpContext.Current;

            // Get BookID, VersionID & Year from Cookie
            var lCookieData = CookieManagement.GetCookie(pContext);

            try
            {
                using (Loreal_DEVEntities3 db = new Loreal_DEVEntities3())
                {
                    lstAnswers = db.Answers
                        .Where(a => a.BookID == lCookieData.BookID && a.Year == lCookieData.Year && a.VersionID == lCookieData.VersionID)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in GetAnswers.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }

            if (lstAnswers.Any())
            {
                lResult = Ok(lstAnswers);
            }
            else
            {
                lResult = Ok();
            }

            return lResult;
        }


        // POST api/<controller>
        [Route("api/Terms/saveAnswers/answers")]
        [Authorize]
        public IHttpActionResult saveAnswers(List<Answer> answers)
        {
            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lAnswerIDs = new List<string>();
            var lAnswerIDsArray = lAnswerIDs.ToArray();

            try
            {
                foreach (var answer in answers)
                {
                    answer.BookID = Convert.ToInt32(lCookieData.BookID);
                    answer.VersionID = lCookieData.VersionID < 1 ? 1 : lCookieData.VersionID;
                    answer.Year = lCookieData.Year;
                    answer.Load_Date = DateTime.Now;
                    string lID = Save(answer);
                    lAnswerIDs.Add(lID);
                }

            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in saveAnswers.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }

            if (lAnswerIDs.Any())
            {
                lResult = Ok(lAnswerIDs);
            }
            else if (lAnswerIDs.Contains("-1"))
            {
                lResult = BadRequest();
            }
            else
            {
                lResult = Ok();
            }

            return lResult;
        }

        private string Save(Answer pAnswer)
        {
            int lAnswerID = -1;
            var messages = new ModelStateDictionary();
            var lLogMsg = string.Empty;

            try
            {
                using (Loreal_DEVEntities3 db = new Loreal_DEVEntities3())
                {
                    // Ensure the correct Answer is either updated or created.
                    //var isUpdate = db.Answers.Find(pAnswer.AnswerID);
                    var lAnswer = db.Answers.Where(a => (a.QuestionID == pAnswer.QuestionID
                        && a.BookID == pAnswer.BookID
                        && a.Year == pAnswer.Year
                        && a.VersionID == pAnswer.VersionID)).FirstOrDefault();

                    if (lAnswer != null)
                    {
                        // Update
                        lAnswer.AnswerFreeForm = pAnswer.AnswerFreeForm;
                        lAnswer.AnswerYesNo = pAnswer.AnswerYesNo;
                        lAnswer.VersionID = pAnswer.VersionID;
                        lAnswerID = db.SaveChanges();
                    }
                    else
                    {
                        // Create
                        db.Answers.Add(pAnswer);
                        lAnswerID = db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in Save Answer. ANSWER ID: {0}, QUESTION ID: {1} BookID: {2}.", pAnswer.AnswerID, pAnswer.QuestionID, pAnswer.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
            return lAnswerID.ToString();
        }

        private ModelStateDictionary ConvertToModelState(System.Web.Mvc.ModelStateDictionary state)
        {
            ModelStateDictionary lResult = new ModelStateDictionary();

            foreach (var list in state.ToList())
            {
                for (int i = 0; i < list.Value.Errors.Count; i++)
                {
                    lResult.AddModelError(list.Key, list.Value.Errors[i].ErrorMessage);
                }
            }

            return lResult;
        }




    }
}