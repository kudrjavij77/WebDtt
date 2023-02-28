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
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace WebDtt.Models.Controllers
{
    [Authorize]
    [Route("api/CRates/{action}", Name = "CRatesApi")]
    public class CRatesController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        [Authorize(Roles = "expert")]
        public async Task<HttpResponseMessage> GetByStudentExamId(int studentExamId, int expertId, DataSourceLoadOptions loadOptions) {
            var crates = _context.CRates
                .Include(i => i.CQuestion.CQuestionRates)
                .Where(i => i.StudentExamID == studentExamId && i.ExpertID == expertId)
                .OrderBy(i=>i.CQuestion.Number);

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(crates, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetByStudentExamIdForUser(int studentExamId, DataSourceLoadOptions loadOptions)
        {
            var crates = _context.CRates
                .Include(i => i.CQuestion.CQuestionRates)
                .Where(i => i.StudentExamID == studentExamId)
                .OrderBy(i => i.CQuestion.Number);

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(crates, loadOptions));
        }

        [HttpPost]
        [Authorize(Roles = "expert")]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new CRate();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.CRates.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.CRateID);
        }

        [HttpPut]
        [Authorize(Roles = "expert")]
        public async Task<HttpResponseMessage> Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.CRates.FirstOrDefault(item => item.CRateID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "CRate not found");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);
            model.UpdateDateTime=DateTime.Now;
            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            await _context.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Authorize(Roles = "admin")]
        public void Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.CRates.FirstOrDefault(item => item.CRateID == key);

            _context.CRates.Remove(model);
            _context.SaveChanges();
        }
        

        [HttpGet]
        public async Task<HttpResponseMessage> CQuestionsLookup(string kim, DataSourceLoadOptions loadOptions)
        {
            var id = Guid.Parse(kim);
            var lookup = await DataSourceLoader.LoadAsync(_context.CQuestions.Where(x => x.KIM == id).Select(x => new
            {
                x.ObjectID,
                x.Number,
                x.CheckMode,
                CQuestionRates = x.CQuestionRates.OrderBy(c => c.KeyRate)
            }).OrderBy(x => x.Number), loadOptions);
            return Request.CreateResponse(HttpStatusCode.OK, lookup);
        }

        [HttpGet]
        public HttpResponseMessage PersonsLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Persons
                         orderby i.ViewClass
                         select new {
                             Value = i.PersonID,
                             Text = i.ViewClass
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage StudentExamsLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.StudentExams
                         orderby i.PersonID
                         select new {
                             Value = i.StudentExamID,
                             Text = i.PersonID
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        private void PopulateModel(CRate model, IDictionary values) {
            string CRATE_ID = nameof(CRate.CRateID);
            string STUDENT_EXAM_ID = nameof(CRate.StudentExamID);
            string EXPERT_ID = nameof(CRate.ExpertID);
            string CQUESTION_ID = nameof(CRate.CQuestionID);
            string START_DATE_TIME = nameof(CRate.StartDateTime);
            string UPDATE_DATE_TIME = nameof(CRate.UpdateDateTime);
            string FINISH_DATE_TIME = nameof(CRate.FinishDateTime);
            string RATE = nameof(CRate.Rate);
            string FLAGS = nameof(CRate.Flags);

            if(values.Contains(CRATE_ID)) {
                model.CRateID = Convert.ToInt32(values[CRATE_ID]);
            }

            if(values.Contains(STUDENT_EXAM_ID)) {
                model.StudentExamID = Convert.ToInt32(values[STUDENT_EXAM_ID]);
            }

            if(values.Contains(EXPERT_ID)) {
                model.ExpertID = Convert.ToInt32(values[EXPERT_ID]);
            }

            if(values.Contains(CQUESTION_ID)) {
                model.CQuestionID = new Guid(Convert.ToString(values[CQUESTION_ID]));
            }

            if(values.Contains(START_DATE_TIME)) {
                model.StartDateTime = Convert.ToDateTime(values[START_DATE_TIME]);
            }

            if(values.Contains(UPDATE_DATE_TIME)) {
                model.UpdateDateTime = values[UPDATE_DATE_TIME] != null ? Convert.ToDateTime(values[UPDATE_DATE_TIME]) : (DateTime?)null;
            }

            if(values.Contains(FINISH_DATE_TIME)) {
                model.FinishDateTime = values[FINISH_DATE_TIME] != null ? Convert.ToDateTime(values[FINISH_DATE_TIME]) : (DateTime?)null;
            }

            if(values.Contains(RATE))
            {
                model.Rate = values[RATE] != null
                    ? Convert.ToSingle(values[RATE], CultureInfo.InvariantCulture)
                    : (float?) null;
                model.Flags |= 1;
            }

            if(values.Contains(FLAGS)) {
                model.Flags = Convert.ToInt32(values[FLAGS]);
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