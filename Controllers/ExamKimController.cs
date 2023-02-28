using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebDtt.Models;

namespace WebDtt.Controllers
{
    [Authorize(Roles = "admin,manager")]
    public class ExamKimController : Controller
    {
        private AISExam_testingEntities _db = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        // GET: ExamKim
        public ActionResult ExamKimManager()
        {
            return View();
        }
    }
}