using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebDtt.Controllers
{
    [Authorize(Roles = "operator,user,manager,admin")]
    public class PersonController : Controller
    {
        // GET: Person students
        [Authorize(Roles = "user")]
        public ActionResult PersonsForUser()
        {
            return View();
        }

        [Authorize(Roles = "operator,manager,admin")]
        public ActionResult Students()
        {
            return View();
        }

        [Authorize(Roles = "operator,manager,admin")]
        public ActionResult Delegates()
        {
            return View();
        }

        [Authorize(Roles = "operator,manager,admin")]
        public ActionResult Teachers()
        {
            return View();
        }

        [Authorize(Roles = "operator,manager,admin")]
        public ActionResult Experts()
        {
            return View();
        }
    }
}