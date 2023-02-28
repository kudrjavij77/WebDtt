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
    [Authorize]
    [Route("api/BAnswers/{action}", Name = "BAnswersApi")]
    public class BAnswersController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var banswers = _context.BAnswers.Select(i => new {
                i.BAnswerID,
                i.StudentExamID,
                i.BQuestionID,
                i.Value,
                i.Rate
            });

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "BAnswerID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(DataSourceLoader.Load(banswers, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetByStudentExamIdForUser(int studentExamId, DataSourceLoadOptions loadOptions)
        {
            var crates = _context.BAnswers
                .Where(i => i.StudentExamID == studentExamId)
                .Select(x=>new
                {
                    x.BAnswerID,
                    x.BQuestion.Number,
                    x.BQuestionID,
                    x.Rate,
                    x.Flags,
                    x.Value,
                    x.StudentExamID,
                    x.StudentExam.ExamKimID,
                    x.StudentExam.ExamKim.ParentExamKimID,
                    //TODO: проверить, что условие для ParentExamKimID работает правильно
                    Key = _context.BQuestionKeys.Where(k=>k.BQuestion==x.BQuestionID && k.Flags==0 && k.ExamKimID == x.StudentExam.ExamKimID //&& (k.ExamKim.Exam.Flags&128)!=128 
                                                          || k.BQuestion == x.BQuestionID && k.Flags == 0 && k.ExamKimID == x.StudentExam.ExamKim.ParentExamKimID)
                })
                .OrderBy(i => i.Number);

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(crates, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new BAnswer();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.BAnswers.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.BAnswerID);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.BAnswers.FirstOrDefault(item => item.BAnswerID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "BAnswer not found");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
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
            var model = _context.BAnswers.FirstOrDefault(item => item.BAnswerID == key);

            _context.BAnswers.Remove(model);
            _context.SaveChanges();
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

        private void PopulateModel(BAnswer model, IDictionary values) {
            string BANSWER_ID = nameof(BAnswer.BAnswerID);
            string STUDENT_EXAM_ID = nameof(BAnswer.StudentExamID);
            string BQUESTION_ID = nameof(BAnswer.BQuestionID);
            string VALUE = nameof(BAnswer.Value);
            string RATE = nameof(BAnswer.Rate);

            if(values.Contains(BANSWER_ID)) {
                model.BAnswerID = Convert.ToInt32(values[BANSWER_ID]);
            }

            if(values.Contains(STUDENT_EXAM_ID)) {
                model.StudentExamID = Convert.ToInt32(values[STUDENT_EXAM_ID]);
            }

            if(values.Contains(BQUESTION_ID)) {
                model.BQuestionID = new Guid(Convert.ToString(values[BQUESTION_ID]));
            }

            if(values.Contains(VALUE)) {
                model.Value = Convert.ToString(values[VALUE]);
            }

            if(values.Contains(RATE)) {
                model.Rate = values[RATE] != null ? Convert.ToSingle(values[RATE], CultureInfo.InvariantCulture) : (float?)null;
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