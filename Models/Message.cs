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
    
    public partial class Message
    {
        public int MessageID { get; set; }
        public Nullable<System.Guid> UserID { get; set; }
        public string SubjectString { get; set; }
        public string MessageString { get; set; }
        public System.DateTime CreateDate { get; set; }
        public int Flags { get; set; }
        public string Email { get; set; }
    
        public virtual User User { get; set; }
    }
}