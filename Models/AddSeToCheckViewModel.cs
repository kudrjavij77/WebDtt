using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebDtt.Models
{
    public class AddSeToCheckViewModel
    {
        [Required]
        [Display(Name = "Проверяемый экзамена")]
        public int ExamID { get; set; }

        [Required]
        [Display(Name="Количество работ")]
        public int CountStudentExams { get; set; }
    }
}