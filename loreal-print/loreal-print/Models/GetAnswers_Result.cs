//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace loreal_print.Models
{
    using System;
    
    public partial class GetAnswers_Result
    {
        public string Year { get; set; }
        public int SectionID { get; set; }
        public string Section { get; set; }
        public int SectionOrder { get; set; }
        public int SubSectionID { get; set; }
        public string SubSection { get; set; }
        public int SubSectionOrder { get; set; }
        public int SubSectionToQuestionOrder { get; set; }
        public int QuestionID { get; set; }
        public string Question { get; set; }
        public string IsTopLevel { get; set; }
        public string QuestionType { get; set; }
        public string AnswerType { get; set; }
        public Nullable<int> ParentQuestionID { get; set; }
        public Nullable<int> SubQuestionOrder { get; set; }
        public string AnswerYesNo { get; set; }
        public string AnswerFreeForm { get; set; }
    }
}
