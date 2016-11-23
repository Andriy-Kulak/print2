using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models
{
    public class QuestionChildModel
    {
        public QuestionChildModel()
        {
            Init();
        }

        private void Init()
        {
            Name = string.Empty;
            QuestionType = string.Empty;
            AnswerType = string.Empty;
            AnswerYesNo = string.Empty;
            AnswerFreeForm = string.Empty;
        }

        public int ID { get; set; }
        public int? ParentID { get; set; }
        public string Name { get; set; }
        public string QuestionType { get; set; }
        public string AnswerType { get; set; }
        public string AnswerYesNo { get; set; }
        public string AnswerFreeForm { get; set; }
    }
}