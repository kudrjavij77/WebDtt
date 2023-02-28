using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DevExpress.Utils.Extensions;
using DevExpress.XtraPrinting.Native;
using WebDtt.Models;
using WebDtt.Models.Extentions;

namespace WebDtt.Controllers
{
    [Authorize]
    public class StudentExamController : Controller
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [Authorize(Roles = "operator,manager")]
        public ActionResult GetAllStudentExams()
        {
            return View();
        }

        [Authorize(Roles = "user,operator,manager")]
        [HttpGet]
        public ActionResult ViewDetail(int id)
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            var model = _context.StudentExams
                .Include(s => s.Exam.Subject)
                .Include(s => s.Exam.ExamType)
                .Include(s => s.StationExam.Station)
                .Include(s => s.Exam.ExamAddons)
                .FirstOrDefault(s => s.StudentExamID == id);

            if (model == null) return HttpNotFound();

            //var studentExamView = new StudentExamViewModel(model);

            if (!user.IsInRole("user"))
            {
                ViewBag.StudentExam = model;
                return View();
            }

            var person = _context.Persons
                .FirstOrDefault(p => p.UserID == userid && p.PersonID == model.PersonID);
            if (person == null)
                return RedirectToAction("Login", "Account");

            ViewBag.StudentExam = model;
            return View();
        }
        // GET: StudentExam

        
        public ActionResult ValidateStartTest(int studentExamId)
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            if (!user.IsInRole("user"))
            {
                ViewBag.Message = "Нет прав доступа к выбранному разделу.";
                return PartialView("Error");
            }
            
            var se = _context.StudentExams
                .Include(s=>s.Exam.Subject)
                .Include(s=>s.BAnswers)
                .Include(s=>s.Person)
                .FirstOrDefault(s => s.StudentExamID == studentExamId);

            if (se == null)
            {
                ViewBag.Message = "Тест для участника не найден";
                return PartialView("Error");
            }

            if (se.Person.UserID != userid)
            {
                ViewBag.Message = "Вы пытаетесь получить доступ к ресурсу, который вам не принадлежит!";
                return PartialView("Error");
            }

            if ((se.Flags & 128) == 128)
            {
                ViewBag.Message = "Тест удален";
                return PartialView("Error");
            }
            if ((se.Flags & 4) == 4)
            {
                ViewBag.Message = "Оформлен возврат средств";
                return PartialView("Error");
            }
            if ((se.Flags & 2) != 2)
            {
                ViewBag.Message = "Недоступен к прохождению. Обратитесь в тех поддержку.";
                return PartialView("Error");
            }
            if (se.FinishDateTime != null || (se.Flags & 16) == 16)
            {
                ViewBag.Message = $"Невозможно пройти тест, так как он уже был пройден. Время окончания теста: {se.FinishDateTime}";
                return PartialView("Error");
            }

            if (se.ElectronicKIMID != null) return RedirectToAction("StartTest", "KIMPresentor", new { studentExamId = studentExamId });

            if (se.PersonTestDateTime == null)
            {
                var lastDateTime = se.Exam.TestDateTime + new TimeSpan(48, 0, 0);
                if (se.Exam.TestDateTime > DateTime.Now || DateTime.Now > lastDateTime)
                {
                    ViewBag.Message = "Текущее время не удовлетворяет времени написания теста. Обратитесь в поддержку.";
                    return PartialView("Error");
                }
            }
            else
            {
                var lastDateTime = se.PersonTestDateTime + new TimeSpan(48, 0, 0);
                if (se.PersonTestDateTime > DateTime.Now || DateTime.Now > lastDateTime)
                {
                    ViewBag.Message = "Текущее время не удовлетворяет времени написания теста. Обратитесь в поддержку.";
                    return PartialView("Error");
                }
            }

            //дергаем кимы, подходящие по предмету и активные
            //TODO: проверить что такой КИМ не писал!!!
            /*var eKimsAlreadyWritten = _context.StudentExams.Where(s => s.PersonID == se.PersonID &&
                                                                       (s.Flags & 2) == 2 && (s.Flags & 128) != 128 && s.ElectronicKIMID!=null)
                .Select(x=>new {x.ElectronicKIM});

            var electronicKIMs = _context.ElectronicKIMs.Select(e => new
                {
                    e.ObjectID,
                    e.KIM,
                    KimDiscipline = _context.Disciplines
                        .Where(d=>e.KIM1.Discipline==d.ObjectID)
                        .Select(d=>d.Code).FirstOrDefault(),
                    e.Flags
                    
                })
                .Where(ek => ek.KimDiscipline == se.Exam.Subject.SubjectCode 
                             //условие активного КИМ
                             && (ek.Flags & 1) == 1)
                .ToList();

            if (!electronicKIMs.Any())
            {
                ViewBag.Message = "Нет вариантов для выбранного теста! Обратитесь в поддержку.";
                return PartialView("Error");
            }


            //рандомим ким и присваиваем его id studentExam
            var random = new Random();
            var index = random.Next(electronicKIMs.Count);
            se.ElectronicKIMID = electronicKIMs[index].ObjectID;
            var KIM = electronicKIMs[index].KIM;
            _context.SaveChanges();*/


            //берем активные на экзамен варианты ким
            var examKims = _context.ExamKims
                .Include(e=>e.ElectronicKIM)
                .Where(e => (e.ElectronicKIM.Flags & 1) == 1 && e.ExamID==se.ExamID)
                .ToList();

            if (!examKims.Any())
            {
               ViewBag.Message = "Нет вариантов для выбранного теста! Обратитесь в поддержку.";
               return PartialView("Error");
            }

            //берем варианты ким, которые участник решал до этого, включая очные
            var electronicKIMsAlreadyWritten = _context.StudentExams
                .Where(s => s.PersonID == se.PersonID 
                            //&& s.ElectronicKIMID != null
                            )
                .Select(s=>s.ExamKim).ToList();

            var result = new List<ExamKim>();

            //
            if (electronicKIMsAlreadyWritten.Any())
            {
                result.AddRange(examKims
                    .SelectMany(examKim => electronicKIMsAlreadyWritten,
                        (examKim, writtenExamKim) => new {examKim, writtenExamKim})
                    .Where(@t =>
                        //написанные варианты не равны доступным
                        @t.writtenExamKim.ExamKimID != @t.examKim.ExamKimID &&
                        //написанные варианты не равны доступным, которые унаследованы от предыдущих
                        @t.writtenExamKim.ExamKimID != @t.examKim.ParentExamKimID)
                    .Select(@t => @t.examKim));
            }
            else
            {
                result.AddRange(examKims);
            }

            if (!result.Any())
            {
                ViewBag.Message = "Нет вариантов для выбранного теста! Обратитесь в поддержку.";
                return PartialView("Error");
            }

            //рандомим вариант ким и присваиваем его id studentExam
            var random = new Random();
            var index = random.Next(result.Count);
            se.ExamKimID = result[index].ExamKimID;
            se.ElectronicKIMID = result[index].ElectronicKIMID;
            var KIM = result[index].ElectronicKIM.KIM;
            _context.SaveChanges();


            //Добавляем пустые BAnswers StudentExam
            var bQuestions = _context.BQuestions.Where(b => b.KIM == KIM).ToList();
            
            if (!bQuestions.Any())
            {
                ViewBag.Message = "Ошибка структуры КИМ! Обратитесь в поддержку.";
                return PartialView("Error");
            }

            if (se.BAnswers.Any()) return RedirectToAction("StartTest", "KIMPresentor", new { studentExamId = studentExamId });

            foreach (var bQuestion in bQuestions)
            {
                _context.BAnswers.Add(new BAnswer()
                {
                    StudentExamID = studentExamId,
                    BQuestionID = bQuestion.ObjectID
                });
            }
            _context.SaveChanges();

            return RedirectToAction("StartTest", "KIMPresentor", new { studentExamId = studentExamId});
        }

        [HttpPost]
        [Authorize(Roles = "manager")]
        //TODO: Check this third
        public async Task SendElectronicKim(int studentExamId)
        {
            var stExam = _context.StudentExams
                .FirstOrDefault(se => se.StudentExamID == studentExamId &&
                                           se.FinishDateTime!=null &&
                                           (se.Flags & 16) == 16 &&
                                           (se.Flags & 4) != 4 &&
                                           (se.Flags & 128) != 128);
            var target = "D:\\aisexam8filestore\\VariantKIM";
            if (stExam != null && stExam.ExamKimID != null)
            {
                var path = Path.Combine(target, stExam.ExamKimID.ToString());
                var dir = new DirectoryInfo(path);
                if (dir.Exists)
                { var file = dir.GetFiles();
                    if (file.Any())
                    {
                        var eKim = _context.ExamKims.FirstOrDefault(ek => ek.ExamKimID == stExam.ExamKimID);
                        if (eKim == null) return;

                        var dirStudentExam = new DirectoryInfo(Server.MapPath($"~/Files/StudentExams/{stExam.StudentExamID}"));
                        if (!dirStudentExam.Exists) dirStudentExam.Create();

                        var zipPath = Server.MapPath($"~/Files/StudentExams/{stExam.StudentExamID}/Вариант КИМ № {eKim.VariantNumber}.zip");

                        var f = new FileInfo(zipPath);
                        if (!f.Exists) ZipFile.CreateFromDirectory(path, zipPath);

                        var student = _context.Persons.FirstOrDefault(p => p.PersonID == stExam.PersonID);
                        var user = _context.Users.FirstOrDefault(u => u.UserID == student.UserID);
                        var order = _context.Orders.Include(o => o.OrderType)
                            .FirstOrDefault(o => o.OrderID == stExam.OrderID && o.OrderTypeID == 1);
                        
                        EmailServiceSend emailService = new EmailServiceSend();
                        await emailService.SendKimAfterTestToStudent(order,user,student, zipPath);
                        stExam.Flags |= 8;
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "manager")]
        public async Task SendLinkToBroadcast(int studentExamId)
        {
            var stExam = _context.StudentExams
                .FirstOrDefault(se => se.StudentExamID == studentExamId &&
                                      se.FinishDateTime != null &&
                                      (se.Flags & 16) == 16 &&
                                      (se.Flags & 4) != 4 &&
                                      (se.Flags & 128) != 128);
            if (stExam == null) return;
            var linkToWebinar = _context.ExamAddons
                .FirstOrDefault(ex => ex.ExamID == stExam.ExamID && (ex.Flags & 128) != 128 && (ex.Flags & 1) == 1);
            if (linkToWebinar == null) return;

            var exam = _context.Exams.Include(e => e.ExamType).Include(e => e.Subject)
                .FirstOrDefault(e => e.ExamID == stExam.ExamID);
            if (exam == null) return;

            var student = _context.Persons.FirstOrDefault(p => p.PersonID == stExam.PersonID);
            var user = _context.Users.FirstOrDefault(u => u.UserID == student.UserID);
            var order = _context.Orders.Include(o => o.OrderType)
                .FirstOrDefault(o => o.OrderID == stExam.OrderID && o.OrderTypeID == 1);

            EmailServiceSend emailService = new EmailServiceSend();
            await emailService.SendLinkToBroadCast(order, user, student, exam, linkToWebinar);
            stExam.Flags |= 64;
            await _context.SaveChangesAsync();

        }

        [Authorize]
        public ActionResult DownLoadElectronicKIM(int studentExamId)
        {
            var stExam = _context.StudentExams.FirstOrDefault(se => se.StudentExamID == studentExamId &&
                                                                    se.FinishDateTime != null && (se.Flags & 16) == 16 &&
                                                                    (se.Flags & 4) != 4 && (se.Flags & 128) != 128);
            if (stExam == null)
            {
                ViewBag.Message = "Тест участника не найден";
                return PartialView("Error");
            }
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());
            var student = _context.Persons.FirstOrDefault(p => p.PersonID == stExam.PersonID && p.UserID==userid);
            if (user.IsInRole("user") && student == null)
            {
                ViewBag.Message = "Вы пытаетесь получить доступ к ресурсу, который вам не принадлежит!";
                return PartialView("Error");
            }

            if (stExam.ExamKimID == null)
            {
                ViewBag.Message = "Участнику не присвоен вариант КИМ";
                return PartialView("Error");
            }
            var target = "D:\\aisexam8filestore\\VariantKIM";
            var path = Path.Combine(target, stExam.ExamKimID.ToString());
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                ViewBag.Message = "КИМ не существует";
                return PartialView("Error");
            }

            if (!dir.GetFiles().Any()) 
            {
                ViewBag.Message = "Файл КИМ не существует";
                return PartialView("Error");
            }

            var eKim = _context.ExamKims.FirstOrDefault(ek => ek.ExamKimID == stExam.ExamKimID);
            if (eKim == null)
            {
                ViewBag.Message = "КИМ не существует";
                return PartialView("Error");
            }

            var dirStudentExam = new DirectoryInfo(Server.MapPath($"~/Files/StudentExams/{stExam.StudentExamID}"));
            if (!dirStudentExam.Exists) dirStudentExam.Create();

            var zipPath = Server.MapPath($"~/Files/StudentExams/{stExam.StudentExamID}/Вариант КИМ № {eKim.VariantNumber}.zip");
            //var file = dir.GetFiles();
            var f = new FileInfo(zipPath);
            if (!f.Exists) ZipFile.CreateFromDirectory(path, zipPath);



            return File(zipPath, "application/zip", $"Вариант КИМ № {eKim.VariantNumber}.zip");
        }

        [HttpPost]
        [Authorize(Roles = "manager")]
        public async Task SendEmail(int studentExamId)
        {
            var studentExam = await _context.StudentExams.FirstOrDefaultAsync(se => se.StudentExamID == studentExamId);
        }
        
        [HttpPost]
        [Authorize(Roles = "manager")]
        public async Task ResultIsTrue(int studentExamId)
        {
            var stExam = _context.StudentExams.FirstOrDefault(se => se.StudentExamID == studentExamId &&
                                                                    se.FinishDateTime != null && (se.Flags & 16) == 16 &&
                                                                    (se.Flags & 4) != 4 && (se.Flags & 128) != 128 &&
                                                                    se.CheckState==2);
            if (stExam?.ExamKimID == null) return;

            var exam = _context.Exams.Include(e => e.ExamType).Include(e => e.Subject)
                .FirstOrDefault(e => e.ExamID == stExam.ExamID);

            if (exam == null) return;

            var student = _context.Persons.FirstOrDefault(p => p.PersonID == stExam.PersonID);
            var user = _context.Users.FirstOrDefault(u => u.UserID == student.UserID);
            var order = _context.Orders.Include(o => o.OrderType)
                .FirstOrDefault(o => o.OrderID == stExam.OrderID && o.OrderTypeID == 1);

            EmailServiceSend emailService = new EmailServiceSend();
            await emailService.SendResultIsTrue(order, user, student, exam);
            stExam.Flags |= 32;
            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [Authorize(Roles = "manager")]
        public async Task AttachStudentExamToStationExam(int studentExamId)
        {
            var studentExam = _context.StudentExams.FirstOrDefault(s => s.StudentExamID == studentExamId);
            if (studentExam == null) return;
            if (studentExam.Flags != 1) return;

            var exam = _context.Exams.FirstOrDefault(e => e.ExamID == studentExam.ExamID);
            if (exam == null) return;
            if ((exam.Flags & 128) == 128 || DateTime.Now > exam.TestDateTime) return;

            var stationExams = _context.StationExams
                .Where(st => st.ExamID == studentExam.ExamID)
                .OrderBy(st=>st.StationPriority);
            if (!stationExams.Any()) return;

            var studentExamsOnPpe = _context.StudentExams
                .Count(s => s.ExamID == exam.ExamID && 
                            (s.Flags == 0 || (s.Flags & 1) == 1 || (s.Flags & 2) == 2));
            
            var stationExam = stationExams.FirstOrDefault(s => s.ReservedCapacity > studentExamsOnPpe);
            if (stationExam==null) return;

            studentExam.StationsExamsID = stationExam.StationExamID;
            studentExam.Flags |= 2;
            
            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [Authorize(Roles = "manager")]
        public async Task DetachStudentExamToStationExam(int studentExamId)
        {
            var studentExam = _context.StudentExams.FirstOrDefault(s => s.StudentExamID == studentExamId);
            if (studentExam == null) return;
            
            var exam = _context.Exams.FirstOrDefault(e => e.ExamID == studentExam.ExamID);
            if (exam == null) return;
            if ((exam.Flags & 128) == 128 || DateTime.Now >= exam.TestDateTime) return;

            if ((studentExam.Flags & 2) != 2 | exam.ExamTypeID != 1) return;

            studentExam.StationsExamsID = null;
            studentExam.Flags = 1;

            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [Authorize(Roles = "manager")]
        public async Task SendPassOfExamToStudentExam(int studentExamId)
        {
            var studentExam = _context.StudentExams.FirstOrDefault(s => s.StudentExamID == studentExamId);
            if (studentExam == null) return;
            if ((studentExam.Flags & 2) != 2 || studentExam.StationsExamsID==null) return;

            var exam = _context.Exams
                .Include(e=>e.ExamType)
                .Include(e=>e.Subject)
                .FirstOrDefault(e => e.ExamID == studentExam.ExamID);
            if (exam == null) return;
            if ((exam.Flags & 128) == 128 || DateTime.Now > exam.TestDateTime) return;

            var stationExam = _context.StationExams
                .Include(st=>st.Station)
                .FirstOrDefault(st => st.StationExamID == studentExam.StationsExamsID);
            if (stationExam == null) return;

            var student = _context.Persons.FirstOrDefault(p => p.PersonID == studentExam.PersonID);
            var user = _context.Users.FirstOrDefault(u => u.UserID == student.UserID);
            var order = _context.Orders.Include(o => o.OrderType)
                .FirstOrDefault(o => o.OrderID == studentExam.OrderID && o.OrderTypeID == 1);

            EmailServiceSend emailService = new EmailServiceSend();
            await emailService.SendPassOfExamToStudentExam(order, user, student, exam, stationExam);

            studentExam.Flags |= 256;
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public ActionResult ViewResultOfStudentExam(int studentExamId)
        {
            var cR = _context.CRates.Where(c => c.StudentExamID == studentExamId & c.Rate != null);
            var bR = _context.BAnswers.Where(x => x.StudentExamID == studentExamId & x.Rate != null);

            var primaryMark = 0;

            if (bR.Any()) primaryMark += int.Parse(bR.Sum(b => b.Rate).ToString());
            if (cR.Any()) primaryMark += int.Parse(cR.Sum(c => c.Rate).ToString());
            

            var TestRate = _context.Scales.FirstOrDefault(s =>
                s.Class == _context.StudentExams.FirstOrDefault(se => se.StudentExamID == studentExamId).Person
                    .ParticipantClass
                && s.PrimaryMark == primaryMark
                && s.SubjectID == _context.StudentExams.FirstOrDefault(se => se.StudentExamID == studentExamId).Exam.SubjectID).TestRate;

            var stExam = _context.StudentExams.Select(x=> new
            {
                x.StudentExamID,
                x.Flags,
                x.ExamID,
                x.FinishDateTime,
                x.PersonID,
                x.ExamKimID,
                StudentLastName = x.Person.LastName,
                StudentFirstName = x.Person.FirstName,
                StudentPatronymic = x.Person.Patronymic,
                x.CheckState,
                VariantKim = x.ExamKim.VariantNumber,
                ExamDate = x.Exam.TestDateTime,
                SubjectName = _context.Exams.FirstOrDefault(e=>e.ExamID==x.ExamID).Subject.SubjectName
            }).FirstOrDefault(se => se.StudentExamID == studentExamId &&
                                    se.FinishDateTime != null && (se.Flags & 16) == 16 &&
                                    (se.Flags & 4) != 4 && (se.Flags & 128) != 128);
            if (stExam == null)
            {
                ViewBag.Message = "Тест участника не найден";
                return PartialView("Error");
            }
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());
            var student = _context.Persons.FirstOrDefault(p => p.PersonID == stExam.PersonID && p.UserID == userid);
            if (user.IsInRole("user") && student == null)
            {
                ViewBag.Message = "Вы пытаетесь получить доступ к данным, которые вам не принадлежат!";
                return PartialView("Error");
            }

            ViewBag.StudentExam = stExam;
            ViewBag.StudentExamId = studentExamId;
            ViewBag.TestRate = TestRate;
            return View();
        }


    }
}