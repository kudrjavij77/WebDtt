using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebDtt.Controllers
{
    public class CourseController : Controller
    {
        // GET: Course
        [Authorize(Roles = "admin")]
        public ActionResult AllCourses()
        {
            return View();
        }
    }
}