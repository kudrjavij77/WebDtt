using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DevExpress.Utils.Extensions;
using WebDtt.Models;
using WebDtt.Models.Dto;
using WebDtt.Models.Extentions;
using WebDtt.Properties;

namespace WebDtt.Controllers
{
    public class AnyTestController : Controller
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [Authorize]
        [HttpGet]
        public ActionResult Ankets()
        {
            return View();
        }

        // GET: AnyTest
        [Authorize]
        [HttpGet]
        public ActionResult AnketaKpc2021(int objectId)
        {
            var objectTest = _context.ObjectAnkets.FirstOrDefault(x => x.ObjectAnketID == objectId);
            if (objectTest == null)
                return RedirectToAction("Ankets");

            if(objectTest.PersonGroupID == null)
                return RedirectToAction("Ankets");

            var personGroup = _context.PersonGroups.FirstOrDefault(x => x.PersonGroupID == objectTest.PersonGroupID);

            var userId = _context.Persons.FirstOrDefault(x => x.PersonID == personGroup.PersonID).UserID;

            var authorizeUserId = Guid.Parse(System.Web.HttpContext.Current.GetOwinContext().Authentication.User.Claims.Select(u => u.Value)
                                                 .FirstOrDefault() ?? throw new InvalidOperationException());

            if (userId != authorizeUserId)
                return RedirectToAction("Ankets");

            var group = _context.Groups.Where(x => x.GroupID == personGroup.GroupID).Select(x => new
            {
                x.GroupID,
                x.CurriculumID,
                _context.Curricula.FirstOrDefault(c=>c.CurriculumID==x.CurriculumID).SubjectID
            });

            var subjectName = _context.Subjects.FirstOrDefault(x => x.SubjectID == group.FirstOrDefault().SubjectID).SubjectName;

            _context.Entry(objectTest).State = EntityState.Modified;
            objectTest.StartDateTime = DateTime.Now;
            _context.SaveChanges();

            ViewBag.SubjectName = subjectName;
            ViewBag.PersonObjectAnketID = objectId;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult AnketaKpc2021(AnketsKpc2021 model)
        {
            var fields = model.GetType().GetProperties();

            foreach (var field in model.GetType().GetProperties())
            {
                if (field.Name != "PersonObjectAnketID")
                {
                    var answer = _context.AnketAnswers.FirstOrDefault(x =>
                        x.PersonObjectAnketID == model.PersonObjectAnketID && x.QuestionKey == field.Name);
                    if (answer != null)
                    {
                        _context.Entry(answer).State = EntityState.Modified;
                        answer.Value = field.GetValue(model) != null ? field.GetValue(model).ToString() : null;
                    }
                    else
                    {
                        _context.AnketAnswers.Add(new AnketAnswer()
                        {
                            PersonObjectAnketID = model.PersonObjectAnketID,
                            QuestionKey = field.Name,
                            Value = field.GetValue(model) != null ? field.GetValue(model).ToString() : null
                        });
                    }
                }
            }

            var objectAnket = _context.ObjectAnkets.FirstOrDefault(x => x.ObjectAnketID == model.PersonObjectAnketID);

            _context.Entry(objectAnket).State = EntityState.Modified;
            if (objectAnket != null) objectAnket.EndDateTime = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction("Ankets");
        }

        public async Task SendMessageAboutAnket(int objectId)
        {
            var objectankets = _context.ObjectAnkets.Select(i => new {
                i.ObjectAnketID,
                i.AnketID,
                _context.Ankets.FirstOrDefault(a=>a.AnketID==i.AnketID).AnketName,
                i.PersonGroupID,
                i.StudentExamID,
                _context.Persons.Where(p =>
                        (i.StudentExamID != null && p.PersonID == _context.StudentExams.FirstOrDefault(st => st.StudentExamID == i.StudentExamID).PersonID) ||
                        (i.PersonGroupID != null && p.PersonID == _context.PersonGroups.FirstOrDefault(pg => pg.PersonGroupID == i.PersonGroupID).PersonID))
                    .Select(x => new
                    {
                        x.PersonID,
                        x.LastName,
                        x.FirstName,
                        ShortFio = x.LastName + " " + x.FirstName
                    }).FirstOrDefault().ShortFio,
                _context.Persons.Where(p =>
                        (i.StudentExamID != null && p.PersonID == _context.StudentExams.FirstOrDefault(st => st.StudentExamID == i.StudentExamID).PersonID) ||
                        (i.PersonGroupID != null && p.PersonID == _context.PersonGroups.FirstOrDefault(pg => pg.PersonGroupID == i.PersonGroupID).PersonID))
                    .Select(x => new
                    {
                        x.PersonID,
                        x.UserID,
                        _context.Users.FirstOrDefault(u=>u.UserID==x.UserID).Email
                    }).FirstOrDefault().Email,
                _context.Subjects.FirstOrDefault(s => 
                    (i.StudentExamID != null && s.SubjectID == _context.StudentExams.FirstOrDefault(st => st.StudentExamID == i.StudentExamID).Exam.SubjectID) ||
                    (i.PersonGroupID != null && s.SubjectID == _context.PersonGroups.FirstOrDefault(pg => pg.PersonGroupID == i.PersonGroupID).Group.Curriculum.SubjectID)).SubjectName

            }).FirstOrDefault(i=>i.ObjectAnketID==objectId);

            if(objectankets==null) return;
            var callbackUrl = "https://dtt.ege.spb.ru/AnyTest/Ankets";
            EmailServiceSend emailService = new EmailServiceSend();
            using (var message = new MailMessage()
            {
                Subject = objectankets.AnketName,
                Body = $"<html><head><title>{objectankets.AnketName}</title></head>" +
                       $"<body>Для вас подготовлен опрос '{objectankets.AnketName}' по предмету {objectankets.SubjectName} для участника {objectankets.ShortFio}." +
                       $"Пройдите опрос, чтобы сделать нашу работу еще лучше." +
                       $"Чтобы пройти опрос, <a href=\"" + callbackUrl + "\">перейдите по ссылке</a>" +
                       $"</body>" +
                       "<footer>Пожалуйста, не отвечайте на это письмо.</footer>",
                IsBodyHtml = true,
                From = new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ")
            })
            {
                message.To.Add(new MailAddress(objectankets.Email, "Дорогой друг"));
                await emailService.SendManagerMessage(message);
            }

            var objAnket = await _context.ObjectAnkets.FirstOrDefaultAsync(x => x.ObjectAnketID == objectId);
            if (objAnket == null) return;

            _context.Entry(objAnket).State = EntityState.Modified;
            objAnket.Flags |= 1;

            await _context.SaveChangesAsync();

        }

    }
}