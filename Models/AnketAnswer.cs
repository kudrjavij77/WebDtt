//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebDtt.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class AnketAnswer
    {
        public int AnswerID { get; set; }
        public int PersonObjectAnketID { get; set; }
        public string QuestionKey { get; set; }
        public string Value { get; set; }
    
        public virtual ObjectAnket ObjectAnket { get; set; }
    }
}
