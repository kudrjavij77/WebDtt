using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using DevExpress.XtraEditors.ButtonPanel;
using WebDtt.Models;
using WebDtt.Models.Dto;

namespace WebDtt.Controllers
{
    public class HomeController : Controller
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        public ActionResult Index()
        {
            News lastNews = null;
            if (_context != null)
                lastNews = _context.News.OrderByDescending(x=>x.CreateDate).First();

            ViewBag.Year = DateTime.Now.Month > 7 ? DateTime.Now.Year + 1 : DateTime.Now.Year;


            ViewBag.lastNews = new NewsDto(lastNews);
            //Количество пеподавателей
            ViewBag.TeacherCount = 25;
            //Количество программ обучения
            ViewBag.ProgramCount = 32;
            //Средний прирост балла
            ViewBag.AvgScore = 30;
            //Количество студентов
            ViewBag.StudentCount = "20000+";
            //Количество отличников
            ViewBag.BextStudentCount = "1500+";
            //тебе нужно изменить эту вьюху
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostMessage(MessageViewModel model)
        {
            var message = new Message();
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            if (user.Identity.IsAuthenticated)
            {
                message.UserID = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ??
                                            throw new InvalidOperationException());
                if (user.IsInRole("user")) message.Flags = 1;
            }

            message.CreateDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                message.Email = model.Email;
                message.SubjectString = model.SubjectString;
                message.MessageString = model.MessageString;
                _context.Messages.Add(message);
                _context.SaveChanges();
                return View("Contact");
            }

            ModelState.AddModelError("",  "Обнаружена ошибка");
            return View();
        }
    }
}