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
    
    public partial class ObjectAnket
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ObjectAnket()
        {
            this.AnketAnswers = new HashSet<AnketAnswer>();
        }
    
        public int ObjectAnketID { get; set; }
        public Nullable<int> AnketID { get; set; }
        public Nullable<int> PersonGroupID { get; set; }
        public Nullable<int> StudentExamID { get; set; }
        public Nullable<System.DateTime> StartDateTime { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
        public int Flags { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AnketAnswer> AnketAnswers { get; set; }
        public virtual Anket Anket { get; set; }
        public virtual PersonGroup PersonGroup { get; set; }
        public virtual StudentExam StudentExam { get; set; }
    }
}