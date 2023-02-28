using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DevExpress.XtraGauges.Core.Model;

namespace WebDtt.Models.Controllers
{
    [Authorize]
    [Route("api/ObjectAnkets/{action}", Name = "ObjectAnketsApi")]
    public class ObjectAnketsController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        public async Task<HttpResponseMessage> Get(DataSourceLoadOptions loadOptions) 
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            var objectankets = _context.ObjectAnkets.Select(i => new {
                i.ObjectAnketID,
                i.AnketID,
                i.PersonGroupID,
                i.StudentExamID,
                StudentId = _context.Persons.Where(p=>
                    (i.StudentExamID!=null && p.PersonID == _context.StudentExams.FirstOrDefault(st=>st.StudentExamID==i.StudentExamID).PersonID) || 
                    (i.PersonGroupID!=null && p.PersonID == _context.PersonGroups.FirstOrDefault(pg=>pg.PersonGroupID==i.PersonGroupID).PersonID))
                    .Select(x=>new
                    {
                        x.PersonID,
                        x.LastName,
                        x.FirstName,
                        ShortFio = x.LastName + " " + x.FirstName
                    }).FirstOrDefault().ShortFio,
                i.StartDateTime,
                i.EndDateTime,
                i.Flags,
                EmailSend = (i.Flags & 1) == 1
            });

            if (user.IsInRole("user"))
            {
                var personGroups = _context.PersonGroups.Where(pg => pg.Order.Person.UserID == userid).Select(pg=>pg.PersonGroupID);
                var studentExams = _context.StudentExams.Where(st => st.Person.UserID == userid).Select(st=>st.StudentExamID);
                objectankets = objectankets.Where(x=>(personGroups.Contains((int) x.PersonGroupID) | studentExams.Contains((int) x.StudentExamID)) & x.EndDateTime==null);
            }

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(objectankets, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new ObjectAnket();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.ObjectAnkets.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.ObjectAnketID);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.ObjectAnkets.FirstOrDefault(item => item.ObjectAnketID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "ObjectAnket not found");

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
            var model = _context.ObjectAnkets.FirstOrDefault(item => item.ObjectAnketID == key);

            _context.ObjectAnkets.Remove(model);
            _context.SaveChanges();
        }


        [HttpGet]
        public HttpResponseMessage AnketsLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Ankets
                         orderby i.AnketName
                         select new {
                             Value = i.AnketID,
                             Text = i.AnketName
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage PersonGroupsLookup(DataSourceLoadOptions loadOptions) {
            var lookup = _context.PersonGroups
                .Select(i => new
                {
                    Value = i.PersonGroupID, 
                    Text = i.Group.Curriculum.Subject.SubjectName
                });
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage PersonsLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Persons
                .Select(i => new
                {
                    Value = i.PersonID,
                    Text = i.ViewShortFio
                });
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage StudentExamsLookup(DataSourceLoadOptions loadOptions) {
            var lookup = _context.StudentExams
                .Select(i => new
                {
                    Value = i.StudentExamID, 
                    Text = i.Exam.Subject.SubjectName
                });
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        private void PopulateModel(ObjectAnket model, IDictionary values) {
            string OBJECT_ANKET_ID = nameof(ObjectAnket.ObjectAnketID);
            string ANKET_ID = nameof(ObjectAnket.AnketID);
            string PERSON_GROUP_ID = nameof(ObjectAnket.PersonGroupID);
            string STUDENT_EXAM_ID = nameof(ObjectAnket.StudentExamID);
            string START_DATE_TIME = nameof(ObjectAnket.StartDateTime);
            string END_DATE_TIME = nameof(ObjectAnket.EndDateTime);
            string FLAGS = nameof(ObjectAnket.Flags);

            if(values.Contains(OBJECT_ANKET_ID)) {
                model.ObjectAnketID = Convert.ToInt32(values[OBJECT_ANKET_ID]);
            }

            if(values.Contains(ANKET_ID)) {
                model.AnketID = values[ANKET_ID] != null ? Convert.ToInt32(values[ANKET_ID]) : (int?)null;
            }

            if(values.Contains(PERSON_GROUP_ID)) {
                model.PersonGroupID = values[PERSON_GROUP_ID] != null ? Convert.ToInt32(values[PERSON_GROUP_ID]) : (int?)null;
            }

            if(values.Contains(STUDENT_EXAM_ID)) {
                model.StudentExamID = values[STUDENT_EXAM_ID] != null ? Convert.ToInt32(values[STUDENT_EXAM_ID]) : (int?)null;
            }

            if(values.Contains(START_DATE_TIME)) {
                model.StartDateTime = values[START_DATE_TIME] != null ? Convert.ToDateTime(values[START_DATE_TIME]) : (DateTime?)null;
            }

            if(values.Contains(END_DATE_TIME)) {
                model.EndDateTime = values[END_DATE_TIME] != null ? Convert.ToDateTime(values[END_DATE_TIME]) : (DateTime?)null;
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