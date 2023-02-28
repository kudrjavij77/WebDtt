using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebDtt.Models;

namespace WebDtt.Controllers
{
    [Authorize]
    public class DonateStatuController : Controller
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };
        // GET: DonateStatu

    }
}