using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AisExam8.Base;
using DevExpress.CodeParser;
using WebDtt.Models;
using WebDtt.Models.Extentions;

namespace WebDtt.Controllers
{
    
    public class CheckStudentExamController : Controller
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        // GET: CheckStudentExam
        [Authorize(Roles = "expert")]
        public ActionResult ExpertWorkView()
        {
            return View();
        }

        [Authorize(Roles = "expert")]
        public ActionResult Stats()
        {
            return View();
        }

        [Authorize(Roles = "expert,manager")]
        public ActionResult StatByQuestion()
        {
            return View();
        }

        [Authorize(Roles = "expert")]
        public ActionResult CheckedStudentExams()
        {
            return View();
        }

        [Authorize(Roles = "manager")]
        public ActionResult AllStats()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "expert")]
        public ActionResult _PopupFormAddNewSeToCheck()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "expert")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _PopupFormAddNewSeToCheck(AddSeToCheckViewModel model)
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            if (!user.IsInRole("expert")) return RedirectToAction("Login","Account");

            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ??
                                throw new InvalidOperationException());
            var expertId = _context.Persons.FirstOrDefault(p => p.UserID == id).PersonID;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var timeTrue = DateTime.Now - new TimeSpan(23, 59, 59);
                    var newStudentExamsToCheck = _context.StudentExams
                        .Where(se =>
                            ((se.Flags & 128) != 128) && ((se.Flags & 4) != 4) && (se.ExamID == model.ExamID) &&
                            (se.FinishDateTime != null) && (se.FinishDateTime < timeTrue) &&
                            (se.ExpertID == null) && (se.CheckState == 0)).Take(model.CountStudentExams).ToList();

                    if (!newStudentExamsToCheck.Any())
                    {
                        transaction.Commit();
                        ModelState.AddModelError("", "Обнаружена ошибка");
                        ViewBag.Message = "Нет работ для выдачи по выбранному экзамену!";
                        return View(model);
                    }

                    foreach (var se in newStudentExamsToCheck)
                    {
                        var KIM = _context.ExamKims
                            .Where(e => e.ExamKimID == se.ExamKimID)
                            .Select(x => new {x.KIM}).FirstOrDefault();

                        var cQuestions = _context.CQuestions
                            .Where(c => c.KIM == KIM.KIM).ToList();

                        //var cRates = _context.CRates.Where(c => c.StudentExamID == se.StudentExamID);
                        //if (cRates.Any())
                        //{
                        //    transaction.Commit();
                        //    ViewBag.Message = "Ошибка назначения работы. CRates уже есть!!";
                        //    return PartialView("Error");
                        //}

                        foreach (var c in cQuestions)
                        {
                            var cRate = _context.CRates
                                .FirstOrDefault(cr =>
                                    cr.CQuestionID == c.ObjectID && cr.ExpertID == expertId &&
                                    cr.StudentExamID == se.StudentExamID);
                            if (cRate == null)
                            {
                                _context.CRates.Add(new CRate()
                                {
                                    StudentExamID = se.StudentExamID,
                                    ExpertID = expertId,
                                    CQuestionID = c.ObjectID,
                                    StartDateTime = DateTime.Now,
                                    Flags = 0
                                });
                            }
                        }

                        se.ExpertID = expertId;
                        se.CheckState = 1;
                    }

                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    Log.My.Error<CheckStudentExamController>(e);
                    transaction.Rollback();
                }
            }

            //ModelState.AddModelError("", "Обнаружена ошибка");
            ViewBag.Message = "";
            return RedirectToAction("ExpertWorkView");
        }


        [Authorize(Roles = "expert")]
        public async Task<ActionResult> FinishCheckStudentExam(int studentExamId)
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            if (!user.IsInRole("expert")) return RedirectToAction("Login", "Account");

            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ??
                                throw new InvalidOperationException());
            var expertId = _context.Persons.FirstOrDefault(p => p.UserID == id).PersonID;

            var studentExam = _context.StudentExams
                .Include(se=>se.CRates)
                .FirstOrDefault(se => se.StudentExamID == studentExamId);

            if (studentExam == null)
            {
                ViewBag.Message = "Работа не найдена";
                return PartialView("Error");
            }

            if (studentExam.ExpertID != expertId)
            {
                ViewBag.Message = "Работа не выдана вам на проверку";
                return PartialView("Error");
            }

            if ((studentExam.CheckState & 1) != 1)
            {
                ViewBag.Message = "Работа не находится на проверке!";
                return PartialView("Error");
            }

            var validate =  ValidateFinishCheck(studentExam.CRates.ToList(), "");

            if (validate!="")
            {
                ViewBag.Message = validate;
                return PartialView("Error");
            }

            studentExam.CheckState = 2;
            foreach (var cRate in studentExam.CRates)
            {
                cRate.FinishDateTime=DateTime.Now;
            }
            await _context.SaveChangesAsync();
            return View("ExpertWorkView");
        }


        private string ValidateFinishCheck(List<CRate> cRates, string message)
        {
            foreach (var cRate in cRates)
            {
                if (cRate.Rate == null)
                {
                    if ((cRate.Flags & 1) == 1) continue;
                    var cQuestion = _context.CQuestions
                        .Where(cq => cq.ObjectID == cRate.CQuestionID)
                        .Select(x => new { x.Number }).FirstOrDefault();
                    message += $"Критерий {cQuestion.Number + 1} не был оценен | ";
                }
                else
                {
                    var cQuestion = _context.CQuestions
                        .Where(cq => cq.ObjectID == cRate.CQuestionID)
                        .Select(x => new {x.Number, x.CQuestionRates}).FirstOrDefault();
                    if (cQuestion.CQuestionRates.Select(x=>x.KeyRate).ToList().Contains(cRate.Rate is int ? (int) cRate.Rate : -1)) 
                        message += $"Значение критерия {cQuestion.Number + 1} не входит в диапазон допустимых значений | ";
                }
            }

            return message;
        }


    }
}