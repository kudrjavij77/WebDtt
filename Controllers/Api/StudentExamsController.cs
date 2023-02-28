using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace WebDtt.Models.Controllers
{
    [Authorize(Roles="operator,manager")]
    [Route("api/StudentExams/{action}", Name = "StudentExamsApi")]
    public class StudentExamsController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        public async Task<HttpResponseMessage> Get(DataSourceLoadOptions loadOptions)
        {
            //var filterDate = DateTime.Now.AddMonths(-4);
            var studentexams = _context.StudentExams
                //.Where(x => x.Order.CreateDate > filterDate)
                .Select(x=> new { 
                //ExamTestDateTime = x.Exam.TestDateTime,
                //NameOfExamType = x.Exam.ExamType.ExamTypeName,
                //ExamSubjectCode = x.Exam.Subject.SubjectCode,
                //ExamSubjectName = x.Exam.Subject.SubjectName,
                StExOrderType = x.Order.OrderType.OrderTypeName,
                OrderFlags = x.Order.Flags,
                Exam = _context.Exams.Where(e=>e.ExamID==x.ExamID).Select(e => new { 
                    e.Subject,
                    e.TestDateTime,
                    e.ExamType
                }).FirstOrDefault(),
                /* StationExamCode =  x.StationExam.Station.StationCode,
                 StationExamName = x.StationExam.Station.StationName,
                 StationExamAddress = x.StationExam.Station.StationAddress,*/
                Station = _context.StationExams.Where(se=>se.StationExamID == x.StationsExamsID).Select(se=>se.Station).FirstOrDefault(),
                x.ExamID,
                x.PersonTestDateTime,
                x.FinishDateTime,
                x.ExpertID,
                x.CheckState,
                x.PersonID,
                x.StudentExamID,
                x.StationsExamsID,
                x.OrderID,
                x.Flags,
                x.Order.CreateDate
            });
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(studentexams, loadOptions));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new StudentExam();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.StudentExams.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.StudentExamID);
        }

        [HttpPut]
        [Authorize(Roles = "admin")]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.StudentExams.FirstOrDefault(item => item.StudentExamID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "StudentExam not found");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Authorize(Roles ="admin")]
        public void Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.StudentExams.FirstOrDefault(item => item.StudentExamID == key);

            _context.StudentExams.Remove(model);
            _context.SaveChanges();
        }        

        private void PopulateModel(StudentExam model, IDictionary values) {
            string STUDENT_EXAM_ID = nameof(StudentExam.StudentExamID);
            string PERSON_ID = nameof(StudentExam.PersonID);
            string EXAM_ID = nameof(StudentExam.ExamID);
            string FLAGS = nameof(StudentExam.Flags);
            string ORDER_ID = nameof(StudentExam.OrderID);
            string STATIONS_EXAMS_ID = nameof(StudentExam.StationsExamsID);
            string FILE_ID = nameof(StudentExam.FileID);
            string FINISH_DATE_TIME = nameof(StudentExam.FinishDateTime);
            string PERSON_TEST_DATE_TIME = nameof(StudentExam.PersonTestDateTime);

            if(values.Contains(STUDENT_EXAM_ID)) {
                model.StudentExamID = Convert.ToInt32(values[STUDENT_EXAM_ID]);
            }

            if(values.Contains(PERSON_ID)) {
                model.PersonID = Convert.ToInt32(values[PERSON_ID]);
            }

            if(values.Contains(EXAM_ID)) {
                model.ExamID = Convert.ToInt32(values[EXAM_ID]);
            }

            if(values.Contains(FLAGS)) {
                model.Flags = Convert.ToInt32(values[FLAGS]);
            }

            if(values.Contains(ORDER_ID)) {
                model.OrderID = values[ORDER_ID] != null ? Convert.ToInt32(values[ORDER_ID]) : (int?)null;
            }

            if(values.Contains(STATIONS_EXAMS_ID)) {
                model.StationsExamsID = values[STATIONS_EXAMS_ID] != null ? Convert.ToInt32(values[STATIONS_EXAMS_ID]) : (int?)null;
            }

            if(values.Contains(FILE_ID)) {
                model.FileID = values[FILE_ID] != null ? Convert.ToInt32(values[FILE_ID]) : (int?)null;
            }

            if (values.Contains(FINISH_DATE_TIME))
            {
                model.FinishDateTime = Convert.ToDateTime(values[FINISH_DATE_TIME]);
            }

            if (values.Contains(PERSON_TEST_DATE_TIME))
            {
                model.PersonTestDateTime = Convert.ToDateTime(values[PERSON_TEST_DATE_TIME]);
            }
        }

        private string GetFullErrorMessage(ModelStateDictionary modelState) {
            var messages = new List<string>();

            foreach(var entry in modelState) {
                foreach(var error in entry.Value.Errors)
                    messages.Add(error.ErrorMessage);
            }

            return String.Join(" ", messages);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}