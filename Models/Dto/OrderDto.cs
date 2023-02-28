using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDtt.Models.Dto
{
    public class UserOrderViewModel
    {
        public int OrderID { get; set; }
        public int OrderTypeID { get; set; }
        public int CreatorPersonID { get; set; }
        public int StudentPersonID { get; set; }
        public int Flags { get; set; }
        public DateTime CreateDate => DateTime.Now;
        public DateTime UpdateDate { get; set; }
        public int ReceiptFileID { get; set; }

        public virtual File File { get; set; }
        public virtual Person Creator { get; set; }
        public virtual OrderType OrderType { get; set; }
        public virtual Person Student { get; set; }
        public virtual ICollection<StudentExam> StudentExams { get; set; }
    }

    public class UserOrderKpcViewModel
    {
        public int OrderID { get; set; }
        public int OrderTypeID { get; set; }
        public int CreatorPersonID { get; set; }
        public int StudentPersonID { get; set; }
        public int Flags { get; set; }
        public DateTime CreateDate => DateTime.Now;
        public DateTime UpdateDate { get; set; }
        public int ReceiptFileID { get; set; }
        public int DonateTypeID { get; set; }

        public virtual File File { get; set; }
        public virtual Person Creator { get; set; }
        public virtual OrderType OrderType { get; set; }
        public virtual Person Student { get; set; }
        public virtual Group Group { get; set; }
    }

    public class OrderKpc
    {
        public int OrderID { get; set; }
        public string OrderNumber
        {
            get => Group != null ? $"{OrderID}/{Group.GroupName}" : $"{OrderID}";
        }

        public int OrderTypeID { get; set; }
        public OrderType OrderType { get; set; }
        public int CreatorPersonID { get; set; }
        public Person CreatorPerson { get; set; }
        public int StudentPersonID { get; set; }
        public Person StudentPerson { get; set; }
        public int GroupID { get; set; }
        public Group Group { get; set; }
        public int DonateTypeID { get; set; }
        public DonateType DonateType { get; set; }
        public List<DonateInfo> DonateInfos { get; set; }
        public List<DonateStatu> DonateStatus { get; set; }
        public int CountDonates { get; set; }
        public DateTime CreateDate { get; set; }
        public int Flags { get; set; }
        public int DonateIndex { get; set; }
        public int Discount { get; set; }
        public bool MomBlank { get; set; }

    }

    public class ReceiptKpc
    {
        public string OrderNumber { get; set; }
        public Person StudentPerson { get; set; }
        public DonateInfo DonateInfo { get; set; }
    }

    public class OrderDonate
    {
        public string Info { get; set; }
        public int OrderId { get; set; }
        public int Green { get; set; }
        public int Yellow { get; set; }
        public int Red { get; set; }
        public int Grey { get; set; }


        public OrderDonate(int orderId, List<DonateStatu> donateStatus, DonateType donateType)
        {
            Info = orderId.ToString();
            OrderId = orderId;
            Green = donateStatus.Any(d => d.Flags == 2) ? donateStatus.Where(d => d.Flags == 2).Sum(d => d.Total) : 0;
            Yellow = donateStatus.Any(d => d.Flags == 1) ? donateStatus.Where(d => d.Flags == 1).Sum(d => d.Total) : 0;
            var virtRed = donateType.DonateInfoes
                .Where(di => di.EndDate != null && DateTime.Now.AddDays(5) >= (di.EndDate))
                .Sum(di => di.Total);
            Red = virtRed > Green + Yellow ? virtRed - (Green + Yellow) : 0;
            Grey = donateType.FullTotal > (Green + Yellow + Red) ? donateType.FullTotal - (Green + Yellow + Red) : 0;
        }
    }

    
}