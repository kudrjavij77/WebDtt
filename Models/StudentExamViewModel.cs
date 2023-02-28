using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDtt.Models
{
    public sealed class StudentExamViewModel
    {
        public int StudentExamID { get; set; }
        public int PersonID { get; set; }
        public int ExamID { get; set; }
        public int Flags { get; set; }
        public int? OrderID { get; set; }
        public int? StationsExamsID { get; set; }
        public int? FileID { get; set; }

        public StationExam StationExam { get; set; }
        public File File { get; set; }
        public Person Person { get; set; }
        public Exam Exam { get; set; }
        public ICollection<CAnswerFile> CAnswerFiles { get; set; }
        public Order Order { get; set; }

        public StudentExamViewModel(StudentExam st)
        {
            StudentExamID = st.StudentExamID;
            PersonID = st.PersonID;
            ExamID = st.ExamID;
            Flags = st.Flags;
            if (st.OrderID != null) OrderID = st.OrderID;
            if (st.StationsExamsID != null) StationsExamsID = st.StationsExamsID;
            if (st.FileID != null) FileID = st.FileID;
            if (st.StationExam != null) StationExam = st.StationExam;
            if (st.File != null) File = st.File;
            if (st.Person != null) Person = st.Person;
            if (st.Exam != null) Exam = st.Exam;
            if (st.CAnswerFiles != null) CAnswerFiles = st.CAnswerFiles;
            if (st.Order != null) Order = st.Order;
        }

    }
}