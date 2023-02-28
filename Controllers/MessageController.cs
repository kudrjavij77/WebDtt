using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace WebDtt.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        // GET: Message
        [Authorize(Roles="manager,operator")]
        public ActionResult AllMessage()
        {
            return View();
        }
    }
}