using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DevExpress.CodeParser;

namespace WebDtt.Models
{
    public class ExpertStatsViewModel
    {
        public int ExamID { get; set; }
        [Display(Name = "Дата экзамена")]
        public DateTime TestDateTime { get; set; }
        [Display(Name = "Класс")]
        public int Class { get; set; }
        [Display(Name = "Код предмета")]
        public int SubjectCode { get; set; }
        [Display(Name = "Предмет")]
        public string SubjectName { get; set; }
        [Display(Name = "Форма экзамена")]
        public string ExamTypeName { get; set; }
        [Display(Name = "Всего загружено")]
        public int AllCount { get; set; }
        [Display(Name = "Выдано на проверку")]
        public int OnHands { get; set; }
        [Display(Name = "Проверено")]
        public int Checked { get; set; }
        [Display(Name = "Доступно к выдаче")]
        public int Ready { get; set; }
    }
}