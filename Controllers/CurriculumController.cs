using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebDtt.Controllers
{
    public class CurriculumController : Controller
    {
        // GET: Curriculum
        [Authorize(Roles = "admin,manager")]
        public ActionResult AllCourses()
        {
            return View();
        }
    }
}