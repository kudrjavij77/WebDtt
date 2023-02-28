using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebDtt.Models;
using WebDtt.Models.Dto;
using WebDtt.Models.Extentions;
using WebDtt.Properties;

namespace WebDtt.Controllers
{
    [Authorize]
    public class EmailSenderController : Controller
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };
        // GET: EmailSender
        [HttpGet]
        [Authorize(Roles = "manager")]
        public ActionResult EmailForm()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EmailForm(ManagerMailMessage model)
        {
            if (!ModelState.IsValid) return View(model);

            var groupIds = model.GroupIds.Split(',').Select(int.Parse).ToList();

            if(!model.ForStudents & !model.ForTeachers)
            {
                ModelState.AddModelError("", "Не выбраны адресаты!");
                ViewBag.Model = model;
                return View(model);
            }

            var emailList = new List<string>();
            foreach (var i in groupIds)
            {
                if (model.ForStudents)
                {
                    var studentEmails = _context.PersonGroups.Where(pg =>
                        pg.GroupID == i && (pg.Flags & 132) == 0 && pg.OrderID != null).Select(pg=>pg.Person.User.Email).ToList();
                    emailList.AddRange(studentEmails);
                }

                if (model.ForTeachers)
                {
                    var teachersEmails = _context.PersonGroups.Where(pg =>
                        pg.GroupID == i && (pg.Flags & 132) == 0 && (pg.Person.PersonTypeID==2||pg.Person.PersonTypeID==5))
                        .Select(pg => pg.Person.User.Email).ToList();
                    emailList.AddRange(teachersEmails);
                }
            }

            if (!emailList.Any())
            {
                ModelState.AddModelError("", "Не удалось получить адреса Email!");
                ViewBag.Model = model;
                return View(model);
            }

            var m = new EmailServiceSend();
            foreach (var email in emailList)
            {
                using (var message = new MailMessage()
                {
                    Subject = model.Title,
                    Body = $"<html><head><title>{model.Title}</title></head>" +
                           $"<body>{model.Body}</body>" +
                           "<footer>Пожалуйста, не отвечайте на это письмо.</footer>",
                    IsBodyHtml = true,
                    From = new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ")
                })
                {
                    message.To.Add(new MailAddress(email, "Дорогой друг"));
                    await m.SendManagerMessage(message);
                }
            }

            return RedirectToAction("SendSuccess");
        }

        [HttpGet]
        public ActionResult SendSuccess()
        {
            return View();
        }
    }
}