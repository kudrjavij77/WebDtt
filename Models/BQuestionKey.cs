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
    
    public partial class BQuestionKey
    {
        public int BQuestionKeyID { get; set; }
        public string KeyValue { get; set; }
        public int Flags { get; set; }
        public System.Guid BQuestion { get; set; }
        public int ExamKimID { get; set; }
    
        public virtual BQuestion BQuestion1 { get; set; }
        public virtual ExamKim ExamKim { get; set; }
    }
}
