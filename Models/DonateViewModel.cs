using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDtt.Models
{
    public class DonateViewModel
    {
        public int OrderID { get; set; }
        public int PartOfDonate { get; set; }
        public byte Total { get; set; }
        public DateTime DonateDate { get; set; }
        public int Flags { get; set; }
        public HttpPostedFileBase Bill { get; set; }
    }
}