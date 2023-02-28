using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebDtt.Models;
using WebDtt.Models.Extentions;

namespace WebDtt.Controllers
{
    [Authorize(Roles = "user")]
    public class KIMPresentorController : Controller
    {
        private AISExam_testingEntities db = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };
        // GET: KIMPresentor
        [HttpGet]
        public ActionResult StartTest(int studentExamId)
        {
            var cookieValue = CryptoPeaceduke.Encrypt(studentExamId.ToString(), "BelkaPizda");
            var cookie = new HttpCookie("studentExamId", cookieValue);
            Response.Headers.Set("studentExamId", cookieValue);
            Response.SetCookie(cookie);
              
            //var cooky = db.Cookies.FirstOrDefault(x => x.EntityId == studentExamId && x.CookieName == "studentExamId");
            //if(cooky == null) {cooky= new Cooky() { CookieName = "studentExamId", EntityId = studentExamId, CookieValue = cookieValue };}
            //else
            //{
            //    cooky.CookieValue = cookieValue;
            //}

            //db.SaveChanges();
            //TODO:validate studentExam of user
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());
            var se = db.StudentExams.Where(s => s.StudentExamID == studentExamId)
                .Select(x=>new {UserID=x.Person.UserID, x.Flags, x.FinishDateTime, x.ElectronicKIMID}).FirstOrDefault();

            if (se == null || se.ElectronicKIMID == null)
            {
                ViewBag.Message = "Непонятная ошибка. Обратитесь в поддержку.";
                return PartialView("Error");
            }
            if (se.UserID != userid)
            {
                ViewBag.Message = "Вы пытаетесь получить не свой вариант КИМ.";
                return PartialView("Error");
            }

            if ((se.Flags & 16) == 16 || se.FinishDateTime != null)
            {
                ViewBag.Message = "Вы уже завершили экзамен.";
                return PartialView("Error");
            }
            
            ViewBag.Id = se.ElectronicKIMID;
            return View();
        }
    }
}