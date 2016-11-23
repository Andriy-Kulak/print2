using loreal_print.MEC_Common.Email;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using NLog;
using System.Web.Http;

namespace loreal_print.Controllers_Api
{
    public class UtilitiesController : ApiController
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

        [Route("api/Utilities/SendEmail/email")]
        [HttpPost]
        public IHttpActionResult SendEmail(Email email)
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            MailMessage lMessage = new MailMessage();

            try
            {
                var lToAddress = new MailAddress(email.EmailTo);
                var lUsername = ConfigurationManager.AppSettings["Username"];
                var lPwd = ConfigurationManager.AppSettings["Pwd"];
                lMessage.To.Add(lToAddress);
                //lMessage.Sender = lDisplayAddress;
                lMessage.IsBodyHtml = true;
                lMessage.From = new MailAddress(lUsername);
                lMessage.Subject = email.EmailSubject;
                lMessage.Body = email.EmailBody;

                SmtpClient client = new SmtpClient();
                client.Host = ConfigurationManager.AppSettings["SMTP"];

                NetworkCredential creds = new NetworkCredential(lUsername, lPwd);

                client.Credentials = creds;
                client.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
                client.EnableSsl = true;
                using (client)
                {
                    client.Send(lMessage);
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in SendEmail.");
                lLogMsg = string.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}. INNER EXCEPTION: {3}.", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }

            // If we made it to this point the send was successful.
            lResult = Ok("Success");

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