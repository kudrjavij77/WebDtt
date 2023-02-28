using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DevExpress.Utils.Extensions;
using DevExpress.XtraGauges.Core.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using WebDtt.Models;

namespace WebDtt.Controllers.Api
{
    
    [Route("api/CheckStudentExams/{action}", Name = "CheckStudentExamsApi")]
    public class CheckStudentExamsController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        [Authorize(Roles = "expert")]
        public async Task<HttpResponseMessage> GetStudentExamsInWork(DataSourceLoadOptions loadOptions)
        {
            var user = HttpContext.Current.GetOwinContext().Authentication.User;
            if (!user.IsInRole("expert")) return null;

            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ??
                                throw new InvalidOperationException());
            var expert = _context.Persons.FirstOrDefault(p => p.UserID == id).PersonID;


            var studentExams = _context.StudentExams//.Include(se=>se.ElectronicKIM)
                //.Include(se=>se.ExamKim)
                .Select(x=>new
                {
                    x.StudentExamID,
                    x.ExamID,
                    Exam = _context.Exams.FirstOrDefault(e=>e.ExamID==x.ExamID),
                    Subject = _context.Subjects.FirstOrDefault(s=>s.SubjectID==_context.Exams.FirstOrDefault(e=>e.ExamID==x.ExamID).SubjectID),
                    ExamType = _context.ExamTypes.FirstOrDefault(et=>et.ExamTypeID== _context.Exams.FirstOrDefault(e => e.ExamID == x.ExamID).ExamTypeID),
                    x.PersonID,
                    x.CheckState,
                    x.Flags,
                    x.ExamKimID,
                    x.ElectronicKIMID,
                    x.ExpertID,
                    _context.ExamKims.FirstOrDefault(ek=>ek.ExamKimID==x.ExamKimID).VariantNumber
                })
                .Where(se=>se.ExpertID!=null && se.ExpertID==expert && (se.CheckState&1)==1);

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(studentExams, loadOptions));

        }

        [HttpGet]
        [Authorize(Roles = "expert")]
        public async Task<HttpResponseMessage> GetExpertExams(DataSourceLoadOptions loadOptions)
        {
            var user = HttpContext.Current.GetOwinContext().Authentication.User;
            if (!user.IsInRole("expert")) return null;
            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ??
                                throw new InvalidOperationException());
            var subIds = _context.Persons.Select(p => new
            {
                p.UserID,
                p.ParticipantClass,
                subId = p.Subjects.Select(s=>s.SubjectID)
            }).FirstOrDefault(p => p.UserID == id);

            IQueryable<Exam> sd;

            
            sd = subIds.ParticipantClass != null
                ? _context.Exams
                    .Include(e => e.ExamType)
                    .Include(e => e.Subject)
                    .Where(e => subIds.subId.Contains(e.SubjectID) && subIds.ParticipantClass == e.Class)
                : _context.Exams
                    .Include(e => e.ExamType)
                    .Include(e => e.Subject)
                    .Where(e => subIds.subId.Contains(e.SubjectID));
            
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(sd, loadOptions));
        }


        [HttpGet]
        [Authorize(Roles = "manager")]
        public async Task<HttpResponseMessage> GetCheckStats(DataSourceLoadOptions loadOptions)
        {
            var timeTrue = DateTime.Now - new TimeSpan(23, 59, 59);
            var stats = _context.Exams.Select(x=>new ExpertStatsViewModel()
            {
                ExamID = x.ExamID,
                Class = x.Class,
                TestDateTime = x.TestDateTime,
                SubjectCode = x.Subject.SubjectCode,
                SubjectName = x.Subject.SubjectName,
                ExamTypeName = x.ExamType.ExamTypeName,
                AllCount = _context.StudentExams.Count(se=> se.ExamID == x.ExamID && (se.Flags&16)==16 && 
                                                            (se.Flags & 4) != 4 && (se.Flags & 128) != 128),
                OnHands = _context.StudentExams.Count(se=>se.ExamID==x.ExamID && (se.Flags & 16) == 16 && 
                                                          (se.CheckState&1)== 1 && (se.Flags & 4) != 4 && (se.Flags & 128) != 128),
                Checked = _context.StudentExams.Count(se => se.ExamID == x.ExamID && (se.Flags & 16) == 16 && 
                                                            (se.CheckState & 2) == 2 && (se.Flags & 4) != 4 && (se.Flags & 128) != 128),
                Ready = _context.StudentExams.Count(se => se.ExamID == x.ExamID && (se.Flags & 16) == 16 && 
                                                          se.CheckState == 0 && (se.Flags & 4) != 4 && (se.Flags & 128) != 128 && se.FinishDateTime < timeTrue)
            });

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(stats, loadOptions));

        }


        [HttpGet]
        [Authorize(Roles = "expert")]
        public HttpResponseMessage GetExpertStats(DataSourceLoadOptions loadOptions)
        {
            var user = HttpContext.Current.GetOwinContext().Authentication.User;
            if (!user.IsInRole("expert")) return null;
            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ??
                                throw new InvalidOperationException());
            var subIds = _context.Persons.Select(p => new
            {
                p.UserID,
                p.PersonID,
                p.ParticipantClass,
                subId = p.Subjects.Select(s => s.SubjectID)
            }).FirstOrDefault(p => p.UserID == id);

            var timeTrue = DateTime.Now - new TimeSpan(23, 59, 59);

            IEnumerable<ExpertStatsViewModel> stats;
            if (subIds.ParticipantClass == null)
            {
                stats = _context.Exams.Where(e => subIds.subId.Contains(e.SubjectID))
                    .Select(x =>
                        new ExpertStatsViewModel()
                        {
                            ExamID = x.ExamID,
                            Class = x.Class,
                            TestDateTime = x.TestDateTime,
                            SubjectCode = x.Subject.SubjectCode,
                            SubjectName = x.Subject.SubjectName,
                            ExamTypeName = x.ExamType.ExamTypeName,
                            AllCount = _context.StudentExams.Count(se =>
                                se.ExamID == x.ExamID && (se.Flags & 16) == 16 && (se.Flags & 4) != 4 &&
                                (se.Flags & 128) != 128),
                            OnHands = _context.StudentExams.Count(se =>
                                se.ExamID == x.ExamID && (se.Flags & 16) == 16 &&
                                (se.Flags & 4) != 4 && (se.Flags & 128) != 128 &&
                                se.ExpertID == subIds.PersonID &&
                                (se.CheckState & 1) == 1),
                            Checked = _context.StudentExams.Count(se =>
                                se.ExamID == x.ExamID && (se.Flags & 16) == 16 &&
                                (se.Flags & 4) != 4 && (se.Flags & 128) != 128 &&
                                se.ExpertID == subIds.PersonID &&
                                (se.CheckState & 2) == 2),
                            Ready = _context.StudentExams.Count(se => se.ExamID == x.ExamID && (se.Flags & 16) == 16 &&
                                                                      (se.Flags & 4) != 4 && (se.Flags & 128) != 128 &&
                                                                      se.ExpertID == null && (se.CheckState & 0) == 0 &&
                                                                      se.FinishDateTime < timeTrue)

                        });
            }
            else
            {
                stats = _context.Exams
                    .Where(e => subIds.subId.Contains(e.SubjectID) && subIds.ParticipantClass == e.Class)
                    .Select(x => new ExpertStatsViewModel()
                    {
                        ExamID = x.ExamID,
                        Class = x.Class,
                        TestDateTime = x.TestDateTime,
                        SubjectCode = x.Subject.SubjectCode,
                        SubjectName = x.Subject.SubjectName,
                        ExamTypeName = x.ExamType.ExamTypeName,
                        AllCount = _context.StudentExams.Count(se =>
                            se.ExamID == x.ExamID && (se.Flags & 16) == 16 && (se.Flags & 4) != 4 &&
                            (se.Flags & 128) != 128),
                        OnHands = _context.StudentExams.Count(se => se.ExamID == x.ExamID && (se.Flags & 16) == 16 &&
                                                                    (se.Flags & 4) != 4 && (se.Flags & 128) != 128 &&
                                                                    se.ExpertID == subIds.PersonID &&
                                                                    (se.CheckState & 1) == 1),
                        Checked = _context.StudentExams.Count(se => se.ExamID == x.ExamID && (se.Flags & 16) == 16 &&
                                                                    (se.Flags & 4) != 4 && (se.Flags & 128) != 128 &&
                                                                    se.ExpertID == subIds.PersonID &&
                                                                    (se.CheckState & 2) == 2),
                        Ready = _context.StudentExams.Count(se => se.ExamID == x.ExamID && (se.Flags & 16) == 16 &&
                                                                  (se.Flags & 4) != 4 && (se.Flags & 128) != 128 &&
                                                                  se.ExpertID == null && (se.CheckState & 0) == 0 &&
                                                                  se.FinishDateTime < timeTrue)

                    });
            }


            return Request.CreateResponse(DataSourceLoader.Load(stats, loadOptions));
        }


        [HttpGet]
        [Authorize(Roles = "expert")]
        public async Task<HttpResponseMessage> GetStudentExamsAlreadyChecked(DataSourceLoadOptions loadOptions)
        {
            var user = HttpContext.Current.GetOwinContext().Authentication.User;
            if (!user.IsInRole("expert")) return null;
            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ??
                                throw new InvalidOperationException());
            var subIds = _context.Persons.Select(p => new
            {
                p.UserID,
                p.ParticipantClass,
                subId = p.Subjects.Select(s => s.SubjectID)
            }).FirstOrDefault(p => p.UserID == id);

            IQueryable<Exam> sd;


            sd = subIds.ParticipantClass != null
                ? _context.Exams
                    .Include(e => e.ExamType)
                    .Include(e => e.Subject)
                    .Where(e => subIds.subId.Contains(e.SubjectID) && subIds.ParticipantClass == e.Class)
                : _context.Exams
                    .Include(e => e.ExamType)
                    .Include(e => e.Subject)
                    .Where(e => subIds.subId.Contains(e.SubjectID));

            var stExams = _context.StudentExams.Where(st=>sd.Contains(st.Exam) && st.CheckState==2 && (st.Flags&128)!=128);

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(stExams, loadOptions));
        }


        [HttpGet]
        [Authorize(Roles = "expert")]
        public HttpResponseMessage ElectronicKimLookup(DataSourceLoadOptions loadOptions)
        {
            var eKim = _context.ElectronicKIMs.ToList();
            return Request.CreateResponse(DataSourceLoader.Load(eKim, loadOptions));
        }


        [HttpGet]
        [Authorize(Roles = "expert,manager")]
        public async Task<HttpResponseMessage> GetStatByBQuestion(int examId, DataSourceLoadOptions loadOptions)
        {
            var stExams = _context.StudentExams
                .Where(se =>
                    (se.Flags & 128) != 128 && (se.Flags & 16) == 16 && se.ExamID == examId && se.CheckState == 2);
                
            if (!stExams.Any()) return null;

            var bQues = _context.BQuestions
                .Where(bq => stExams.Select(s => s.ExamKim.KIM).Distinct().Contains(bq.KIM))
                .Select(x => new
                {
                    Number = x.Number+1,
                    MaxRate = x.KeyRate,
                    RateNull = _context.BAnswers.Count(ba => ba.BQuestionID==x.ObjectID && ba.Rate== null && (ba.StudentExam.Flags & 128) != 128 && (ba.Flags&1)==1 && ba.StudentExam.ExamID==examId),
                    Rate0 = _context.BAnswers.Count(ba => ba.Rate != null && ba.BQuestionID==x.ObjectID && ba.Rate == 0 && (ba.StudentExam.Flags & 128) != 128 && (ba.Flags & 1) == 1 && ba.StudentExam.ExamID == examId),
                    Rate1 = _context.BAnswers.Count(ba => ba.Rate != null && ba.BQuestionID == x.ObjectID && ba.Rate == 1 && (ba.StudentExam.Flags & 128) != 128 && (ba.Flags & 1) == 1 && ba.StudentExam.ExamID == examId),
                    Rate2 = _context.BAnswers.Count(ba => ba.Rate != null && ba.BQuestionID == x.ObjectID && ba.Rate == 2 && (ba.StudentExam.Flags & 128) != 128 && (ba.Flags & 1) == 1 && ba.StudentExam.ExamID == examId),
                    Rate3 = _context.BAnswers.Count(ba => ba.Rate != null && ba.BQuestionID == x.ObjectID && ba.Rate == 3 && (ba.StudentExam.Flags & 128) != 128 && (ba.Flags & 1) == 1 && ba.StudentExam.ExamID == examId),
                    Rate4 = _context.BAnswers.Count(ba => ba.Rate != null && ba.BQuestionID == x.ObjectID && ba.Rate == 4 && (ba.StudentExam.Flags & 128) != 128 && (ba.Flags & 1) == 1 && ba.StudentExam.ExamID == examId),
                    Rate5 = _context.BAnswers.Count(ba => ba.Rate != null && ba.BQuestionID == x.ObjectID && ba.Rate == 5 && (ba.StudentExam.Flags & 128) != 128 && (ba.Flags & 1) == 1 && ba.StudentExam.ExamID == examId),
                    Rate6 = _context.BAnswers.Count(ba => ba.Rate != null && ba.BQuestionID == x.ObjectID && ba.Rate == 6 && (ba.StudentExam.Flags & 128) != 128 && (ba.Flags & 1) == 1 && ba.StudentExam.ExamID == examId),
                    Rate7 = _context.BAnswers.Count(ba => ba.Rate != null && ba.BQuestionID == x.ObjectID && ba.Rate == 7 && (ba.StudentExam.Flags & 128) != 128 && (ba.Flags & 1) == 1 && ba.StudentExam.ExamID == examId),
                    Rate8 = _context.BAnswers.Count(ba => ba.Rate != null && ba.BQuestionID == x.ObjectID && ba.Rate == 8 && (ba.StudentExam.Flags & 128) != 128 && (ba.Flags & 1) == 1 && ba.StudentExam.ExamID == examId),
                    Rate9 = _context.BAnswers.Count(ba => ba.Rate != null && ba.BQuestionID == x.ObjectID && ba.Rate == 9 && (ba.StudentExam.Flags & 128) != 128 && (ba.Flags & 1) == 1 && ba.StudentExam.ExamID == examId),
                    Rate10 = _context.BAnswers.Count(ba => ba.Rate != null && ba.BQuestionID == x.ObjectID && ba.Rate == 10 && (ba.StudentExam.Flags & 128) != 128 && (ba.Flags & 1) == 1 && ba.StudentExam.ExamID == examId)
                })
                .OrderBy(x => x.Number);

            //var bQue = _context.BAnswers
            //    .Where(b => stExams.Select(s=>s.StudentExamID).Contains(b.StudentExamID) && (b.Flags & 1) == 1)
            //    .Select(x=>new
            //    {
            //        ExamID = x.StudentExam.Exam.ExamID,
            //        TestDateTime = x.StudentExam.Exam.TestDateTime,
            //        Class = x.StudentExam.Exam.Class,
            //        SubjectCode = x.StudentExam.Exam.Subject.SubjectCode,
            //        SubjectName = x.StudentExam.Exam.Subject.SubjectName,
            //        ExamTypeName = x.StudentExam.Exam.ExamType.ExamTypeName,
            //        BQuestionID = x.BQuestion.ObjectID,
            //        Number = x.BQuestion.Number + 1,
            //        BAnswerID = x.BAnswerID,
            //        MaxRate = x.BQuestion.KeyRate,
            //        Rate = x.Rate
            //    });

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(bQues, loadOptions));
        }

        [HttpGet]
        [Authorize(Roles = "expert,manager")]
        public async Task<HttpResponseMessage> GetStatByCQuestion(int examId, DataSourceLoadOptions loadOptions)
        {
            var stExams = _context.StudentExams
                .Where(se =>
                    (se.Flags & 128) != 128 && (se.Flags & 16) == 16 && se.ExamID == examId && se.CheckState == 2);

            if (!stExams.Any()) return null;

            var cQues = _context.CQuestions
                .Where(cq => stExams.Select(s => s.ExamKim.KIM).Distinct().Contains(cq.KIM))
                .Select(x =>new
                {
                    Number = x.Number+1,
                    MaxRate = x.CQuestionRates.Max(cqr=>cqr.KeyRate),
                    RateNull = _context.CRates.Count(cr => cr.Rate == null && cr.CQuestionID == x.ObjectID && (cr.StudentExam.Flags & 128) != 128 && (cr.Flags & 1) == 1 && cr.StudentExam.ExamID == examId),
                    Rate0 = _context.CRates.Count(cr => cr.Rate != null && cr.CQuestionID == x.ObjectID && cr.Rate == 0 && (cr.StudentExam.Flags&128)!= 128 && (cr.Flags & 1) == 1 && cr.StudentExam.ExamID == examId),
                    Rate1 = _context.CRates.Count(cr => cr.Rate != null && cr.CQuestionID == x.ObjectID && cr.Rate == 1 && (cr.StudentExam.Flags & 128) != 128 && (cr.Flags & 1) == 1 && cr.StudentExam.ExamID == examId),
                    Rate2 = _context.CRates.Count(cr => cr.Rate != null && cr.CQuestionID == x.ObjectID && cr.Rate == 2 && (cr.StudentExam.Flags & 128) != 128 && (cr.Flags & 1) == 1 && cr.StudentExam.ExamID == examId),
                    Rate3 = _context.CRates.Count(cr => cr.Rate != null && cr.CQuestionID == x.ObjectID && cr.Rate == 3 && (cr.StudentExam.Flags & 128) != 128 && (cr.Flags & 1) == 1 && cr.StudentExam.ExamID == examId),
                    Rate4 = _context.CRates.Count(cr => cr.Rate != null && cr.CQuestionID == x.ObjectID && cr.Rate == 4 && (cr.StudentExam.Flags & 128) != 128 && (cr.Flags & 1) == 1 && cr.StudentExam.ExamID == examId),
                    Rate5 = _context.CRates.Count(cr => cr.Rate != null && cr.CQuestionID == x.ObjectID && cr.Rate == 5 && (cr.StudentExam.Flags & 128) != 128 && (cr.Flags & 1) == 1 && cr.StudentExam.ExamID == examId),
                    Rate6 = _context.CRates.Count(cr => cr.Rate != null && cr.CQuestionID == x.ObjectID && cr.Rate == 6 && (cr.StudentExam.Flags & 128) != 128 && (cr.Flags & 1) == 1 && cr.StudentExam.ExamID == examId),
                    Rate7 = _context.CRates.Count(cr => cr.Rate != null && cr.CQuestionID == x.ObjectID && cr.Rate == 7 && (cr.StudentExam.Flags & 128) != 128 && (cr.Flags & 1) == 1 && cr.StudentExam.ExamID == examId),
                    Rate8 = _context.CRates.Count(cr => cr.Rate != null && cr.CQuestionID == x.ObjectID && cr.Rate == 8 && (cr.StudentExam.Flags & 128) != 128 && (cr.Flags & 1) == 1 && cr.StudentExam.ExamID == examId),
                    Rate9 = _context.CRates.Count(cr => cr.Rate != null && cr.CQuestionID == x.ObjectID && cr.Rate == 9 && (cr.StudentExam.Flags & 128) != 128 && (cr.Flags & 1) == 1 && cr.StudentExam.ExamID == examId),
                    Rate10 = _context.CRates.Count(cr => cr.Rate != null && cr.CQuestionID == x.ObjectID && cr.Rate == 10 && (cr.StudentExam.Flags & 128) != 128 && (cr.Flags & 1) == 1 && cr.StudentExam.ExamID == examId)
                })
                .OrderBy(x=>x.Number);

            //var cQue = _context.CRates
            //    .Where(c=>stExams.Select(s=>s.StudentExamID).Contains(c.StudentExamID) && (c.Flags&1)==1)
            //    .Select(x=>new
            //    {
            //        ExamID = x.StudentExam.Exam.ExamID,
            //        TestDateTime = x.StudentExam.Exam.TestDateTime,
            //        Class = x.StudentExam.Exam.Class,
            //        SubjectCode = x.StudentExam.Exam.Subject.SubjectCode,
            //        SubjectName = x.StudentExam.Exam.Subject.SubjectName,
            //        ExamTypeName = x.StudentExam.Exam.ExamType.ExamTypeName,
            //        CQuestionID = x.CQuestion.ObjectID,
            //        Number = x.CQuestion.Number + 1,
            //        CRateID = x.CRateID,
            //        MaxRate = x.CQuestion.CQuestionRates.Max(y => y.KeyRate),
            //        Rate = x.Rate
            //    });
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(cQues, loadOptions));
        }

    }
}
