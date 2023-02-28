using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using WebDtt.Models;

namespace WebDtt.Controllers.Api
{
    [Authorize(Roles = "admin,operator,manager")]
    [Route("api/SubjectsApi/{action}", Name = "SubjectsApi")]
    public class SubjectsController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false}
        };

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var subjects = _context.Subjects;

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "SubjectID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(DataSourceLoader.Load(subjects, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new Subject();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.Subjects.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.SubjectID);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Subjects.FirstOrDefault(item => item.SubjectID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Subject not found");

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
            var model = _context.Subjects.FirstOrDefault(item => item.SubjectID == key);

            _context.Subjects.Remove(model);
            _context.SaveChanges();
        }


        private void PopulateModel(Subject model, IDictionary values) {
            string SUBJECT_ID = nameof(Subject.SubjectID);
            string SUBJECT_CODE = nameof(Subject.SubjectCode);
            string SUBJECT_NAME = nameof(Subject.SubjectName);
            string SHORT_SUBJECT_NAME = nameof(Subject.ShortSubjectName);

            if(values.Contains(SUBJECT_ID)) {
                model.SubjectID = Convert.ToInt32(values[SUBJECT_ID]);
            }

            if(values.Contains(SUBJECT_CODE)) {
                model.SubjectCode = Convert.ToByte(values[SUBJECT_CODE]);
            }

            if(values.Contains(SUBJECT_NAME)) {
                model.SubjectName = Convert.ToString(values[SUBJECT_NAME]);
            }

            if(values.Contains(SHORT_SUBJECT_NAME)) {
                model.ShortSubjectName = Convert.ToString(values[SHORT_SUBJECT_NAME]);
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