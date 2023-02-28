using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebDtt.Controllers
{
    [Authorize(Roles = "admin,operator,manager")]
    public class StationExamController : Controller
    {
        // GET: StationExam
        public ActionResult Index()
        {
            return View();
        }
    }
}