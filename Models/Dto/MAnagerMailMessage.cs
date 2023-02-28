using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebDtt.Models.Dto
{
    public class ManagerMailMessage
    {
        [Required]
        [Display(Name = "Группы")]
        public string GroupIds { get; set; }
        [Required]
        [Display(Name = "Отправить слушателям")]
        public bool ForStudents { get; set; }
        [Required]
        [Display(Name = "Отправить преподавателям")]
        public bool ForTeachers { get; set; }
        [Required]
        [Display(Name = "Тема")]
        public string Title { get; set; }
        [Required]
        [Display(Name = "Текст")]
        public string Body { get; set; }
    }
}