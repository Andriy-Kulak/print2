using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models
{
    public class QuestionSectionsListModel
    {
        public QuestionSectionsListModel()
        {
            Init();
        }

        private void Init()
        {
            SectionList = new List<QuestionSectionModel>();
        }

        public List<QuestionSectionModel> SectionList { get; set; }
    }
}