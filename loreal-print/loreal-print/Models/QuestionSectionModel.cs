using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models
{
    public class QuestionSectionModel
    {
        public QuestionSectionModel()
        {
            Init();
        }

        private void Init()
        {
            SubSectionList = new List<QuestionSubSectionModel>();
            Name = string.Empty;
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public List<QuestionSubSectionModel> SubSectionList { get; set; }
    }
}