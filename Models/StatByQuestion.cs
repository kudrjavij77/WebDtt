using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebDtt.Models
{
    public class StatByBQuestion
    {
        public int ExamID { get; set; }
        public DateTime TestDateTime { get; set; }
        [Display(Name = "Класс")]
        public int Class { get; set; }
        [Display(Name = "Код предмета")]
        public int SubjectCode { get; set; }
        [Display(Name = "Предмет")]
        public string SubjectName { get; set; }
        [Display(Name = "Форма экзамена")]
        public string ExamTypeName { get; set; }

        public Guid BQuestionID { get; set; }
        public int Number { get; set; }
        public int BAnswerID { get; set; }
        public int MaxRate { get; set; }
        public float Rate { get; set; }
    }

    public class StatByCQuestion
    {
        public int ExamID { get; set; }
        public DateTime TestDateTime { get; set; }
        [Display(Name = "Класс")]
        public int Class { get; set; }
        [Display(Name = "Код предмета")]
        public int SubjectCode { get; set; }
        [Display(Name = "Предмет")]
        public string SubjectName { get; set; }
        [Display(Name = "Форма экзамена")]
        public string ExamTypeName { get; set; }

        public Guid CQuestionID { get; set; }
        public int Number { get; set; }
        public int CRateID { get; set; }
        public int MaxRate { get; set; }
        public float Rate { get; set; }
    }

    public class StatByDQuestion { }

}