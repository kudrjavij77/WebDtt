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
    
    public partial class CAnswerFile
    {
        public int CAnswerFileID { get; set; }
        public Nullable<int> PageNumber { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public int FileID { get; set; }
        public int StudentExamID { get; set; }
    
        public virtual File File { get; set; }
        public virtual StudentExam StudentExam { get; set; }
    }
}
