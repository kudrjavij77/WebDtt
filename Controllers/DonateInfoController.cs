using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebDtt.Controllers
{
    [System.Web.Http.Authorize(Roles = "manager")]
    public class DonateInfoController : Controller
    {
        // GET: DonateInfo
        public ActionResult Index()
        {
            return View();
        }
    }
}