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
    
    public partial class CQuestionRate
    {
        public System.Guid ObjectID { get; set; }
        public byte MagicBit { get; set; }
        public long ts { get; set; }
        public int KeyRate { get; set; }
        public int Flags { get; set; }
        public System.Guid CQuestion { get; set; }
    
        public virtual CQuestion CQuestion1 { get; set; }
    }
}