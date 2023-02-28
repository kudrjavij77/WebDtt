using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebDtt.Models
{
    public class MessageViewModel
    {
        [Required]
        [EmailAddress]
        [RegularExpression(@"^[\d\w\._\-]+@([\d\w\._\-]+\.)+[\w]+$", ErrorMessage = "Некорректный Email")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Тема")]
        [StringLength(maximumLength:50)]
        public string SubjectString { get; set; }
        [Required]
        [Display(Name = "Проблема")]
        public string MessageString { get; set; }
    }
}