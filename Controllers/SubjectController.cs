using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebDtt.Controllers
{
    [Authorize]
    public class SubjectController : Controller
    {
        // GET: Subject
        [Authorize(Roles = "admin,operator,manager")]
        public ActionResult Index()
        {
            return View();
        }
    }
}