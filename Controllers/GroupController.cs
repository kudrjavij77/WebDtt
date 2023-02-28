using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebDtt.Controllers
{
    public class GroupController : Controller
    {
        // GET: Group
        [Authorize(Roles = "manager")]
        public ActionResult AllGroups()
        {
            return View();
        }

        
    }
}