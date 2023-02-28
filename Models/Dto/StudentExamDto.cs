using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDtt.Models.Dto
{
    public class StudentExamViewModel
    {
        public int StudentExamID { get; set; }
        public int PersonID { get; set; }
        public int ExamID { get; set; }
        public int Flags { get; set; }
        public int? OrderID { get; set; }
        public int? OrderFlags => OrderID;
        public int? StationsExamsID { get; set; }
        public int? FileID { get; set; }
        public DateTime? FinishDateTime { get; set; }
        public DateTime? PersonTestDateTime { get; set; }
        public int ExamTestDateTime => ExamID;
        public int ExamTypeName => ExamID;
        public int ExamClass => ExamID;
        public int SubjectCode => ExamID;
        public int SubjectName => ExamID;
        public int? OrderTypeName => OrderID;
        public int? StationCode => StationsExamsID;
        public int? StationName => StationsExamsID;
        public int? StationAddress => StationsExamsID;
    }
}