using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using WebDtt.Models;

namespace WebDtt.Controllers.Api
{
    [Authorize(Roles = "admin,operator,manager")]
    [Route("api/Exams/{action}", Name = "ExamsApi")]
    public class ExamsController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [AllowAnonymous]
        [HttpGet]
        public async Task<HttpResponseMessage> Get(DataSourceLoadOptions loadOptions)
        {
            var exams = _context.Exams
                .Include(e => e.Subject)
                .Include(e => e.ExamType);

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "ExamID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(exams, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new Exam();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"), 
                new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.Exams.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.ExamID);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Exams.FirstOrDefault(item => item.ExamID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Exam not found");

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
        public void Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Exams.FirstOrDefault(item => item.ExamID == key);

            _context.Exams.Remove(model);
            _context.SaveChanges();
        }


        [HttpGet]
        public HttpResponseMessage SubjectsLookup(DataSourceLoadOptions loadOptions) {
            var lookup = _context.Subjects.ToList();
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage ExamTypesLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.ExamTypes.ToList();
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<HttpResponseMessage> GetExamsOchWithPlaces(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Exams
                .Where(e => e.ExamTypeID == 1 && (e.Flags & 128) != 128)
                .Select(x => new
                {
                    SubjectName = x.Subject.SubjectName,
                    Class = x.Class,
                    Date = x.TestDateTime,
                    x.ExamID,
                    x.ExamTypeID,
                    x.SubjectID,
                    x.Subject,
                    x.ExamType,
                    x.Flags,
                    Price = x.Price,
                    Places = _context.StationExams.Where(ste => ste.ExamID == x.ExamID).Sum(y => y.ReservedCapacity),
                    StudentExams = _context.StudentExams.Count(se => se.ExamID == x.ExamID
                                                                     && (se.Flags & 128) != 128
                                                                     && (se.Flags & 4) != 4)
                });

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<HttpResponseMessage> GetExamsDistant(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Exams
                .Where(e => e.ExamTypeID == 3 && e.Flags < 2)
                .Select(x => new
                {
                    x.ExamID,
                    x.TestDateTime,
                    x.SubjectID,
                    x.ExamTypeID,
                    x.Subject,
                    x.Class,
                    x.ExamType,
                    x.Price,
                    x.Flags,
                    CountOrders = _context.StudentExams.Count(se => se.ExamID == x.ExamID)
                });
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }


        private void PopulateModel(Exam model, IDictionary values) {
            string EXAM_ID = nameof(Exam.ExamID);
            string EXAM_TYPE_ID = nameof(Exam.ExamTypeID);
            string SUBJECT_ID = nameof(Exam.SubjectID);
            string CLASS = nameof(Exam.Class);
            string TEST_DATE_TIME = nameof(Exam.TestDateTime);
            string PRICE = nameof(Exam.Price);
            string FLAGS = nameof(Exam.Flags);
            string DURATION = nameof(Exam.Duration);

            if(values.Contains(EXAM_ID)) {
                model.ExamID = Convert.ToInt32(values[EXAM_ID]);
            }

            if (values.Contains(EXAM_TYPE_ID))
            {
                model.ExamTypeID = Convert.ToInt32(values[EXAM_TYPE_ID]);
            }

            if (values.Contains(SUBJECT_ID)) {
                model.SubjectID = Convert.ToInt32(values[SUBJECT_ID]);
            }

            if(values.Contains(CLASS)) {
                model.Class = Convert.ToByte(values[CLASS]);
            }

            if(values.Contains(TEST_DATE_TIME)) {
                model.TestDateTime = Convert.ToDateTime(values[TEST_DATE_TIME]);
            }

            if (values.Contains(PRICE))
            {
                model.Price = Convert.ToInt32(values[PRICE]);
            }

            if (values.Contains(FLAGS)) {
                model.Flags = Convert.ToInt32(values[FLAGS]);
            }

            if (values.Contains(DURATION))
            {
                model.Duration = Convert.ToInt32(values[DURATION]);
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