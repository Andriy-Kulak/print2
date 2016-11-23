using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models
{
    public class QuestionSubSectionModel
    {
        public QuestionSubSectionModel()
        {
            Init();
        }

        private void Init()
        {
            QuestionList = new List<QuestionModel>();
            Name = string.Empty;
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public List<QuestionModel> QuestionList{ get; set; }
    }
}