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
    
    public partial class ElectronicKIMTask
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ElectronicKIMTask()
        {
            this.ElectronicKIMTaskFiles = new HashSet<ElectronicKIMTaskFile>();
        }
    
        public System.Guid ObjectID { get; set; }
        public string Name { get; set; }
        public System.Guid ElectronicKIM { get; set; }
        public byte[] HtmlData { get; set; }
        public int DisplayOrder { get; set; }
        public int Flags { get; set; }
        public long TaskNumberFlags { get; set; }
        public int IdCol { get; set; }
    
        public virtual ElectronicKIM ElectronicKIM1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ElectronicKIMTaskFile> ElectronicKIMTaskFiles { get; set; }
    }
}
