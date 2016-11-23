using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.MEC_Common.Security.Principal
{
    public class CustomPrincipalSerializerModel
    {
        public CustomPrincipalSerializerModel()
        {
            Init();
        }

        private void Init()
        {
            Id = string.Empty;
            UserName = string.Empty;
            Email = string.Empty;
            Role = string.Empty;
            Year = "2017";
            Status = string.Empty;
            Book = string.Empty;
            Publisher = string.Empty;
        }
        //public long Id { get; set; }
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        public string Year { get; set; }
        public int VersionID { get; set; }
        public string Status { get; set; }
        public string Book { get; set; }
        public int? BookID { get; set; }
        public string Publisher { get; set; }
        public int? PublisherID { get; set; }
    }
}