using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models
{
    public class VersionInfoModel
    {
        public VersionInfoModel()
        {
            Init();
        }

        private void Init()
        {
            VersionList = new List<BookToVersion>();
            Status = string.Empty;
        }

        public int VersionID { get; set; }
        public string Status { get; set; }
        public List<BookToVersion> VersionList { get; set; }
    }
}