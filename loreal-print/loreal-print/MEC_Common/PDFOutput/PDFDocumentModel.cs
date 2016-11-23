using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.MEC_Common.PDFOutput
{
    public class PDFDocumentModel
    {
        public PDFDocumentModel()
        {
            Init();
        }

        private void Init()
        {
            Year = "2017";
            Publisher = string.Empty;
            Book = string.Empty;
        }

        public int PublisherID { get; set; }
        public string Publisher { get; set; }
        public int BookID { get; set; }
        public string Book { get; set; }
        public int VersionID { get; set; }
        public string Year { get; set; }
    }
}