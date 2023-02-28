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
    [Authorize(Roles = "admin,manager")]
    [Route("api/ExamKims/{action}", Name = "ExamKimsApi")]
    public class ExamKimsController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        public async Task<HttpResponseMessage> Get(DataSourceLoadOptions loadOptions)
        {
            var examkims = _context.ExamKims
                .Select(x => new
                {
                    x.ExamKimID,
                    x.ElectronicKIMID,
                    x.ExamID,
                    x.KIM,
                    x.ParentExamKimID,
                    x.VariantNumber,
                    Exam = _context.Exams.Select(e=>new
                    {
                        e.ExamID,
                        e.ExamTypeID,
                        e.Class,
                        e.Flags,
                        e.SubjectID,
                        e.TestDateTime,
                        Subject = _context.Subjects.Select(s=>new
                        {
                            s.SubjectID,
                            s.ShortSubjectName,
                            s.SubjectCode,
                            s.SubjectName
                        }).FirstOrDefault(s => s.SubjectID==e.SubjectID),
                        ExamType = _context.ExamTypes.Select(et=>new
                        {
                            et.ExamTypeID,
                            et.ExamTypeName,
                            et.ExamTypeDescription,
                            et.Flags
                        }).FirstOrDefault(et=>et.ExamTypeID==e.ExamTypeID)
                    }).FirstOrDefault(e => e.ExamID==x.ExamID),
                    KIMObject = _context.KIMs.Select(k=>new
                    {
                        k.ObjectID,
                        k.Name,
                        k.Class,
                        DisciplineID = k.Discipline,
                        Discipline = _context.Disciplines.Select(d=>new
                        {
                            d.ObjectID,
                            d.Code
                        }).FirstOrDefault(d=>d.ObjectID==k.Discipline)
                    }).FirstOrDefault(k=>k.ObjectID==x.KIM),
                    ElectronicKIM = _context.ElectronicKIMs.Select(ek=>new
                    {
                        ek.ObjectID,
                        KIMID = ek.KIM,
                        ek.Name,
                        ek.Flags,
                        KIM = _context.KIMs.Select(kim => new
                        {
                            kim.ObjectID,
                            kim.Name,
                            kim.Class,
                            DisciplineID = kim.Discipline,
                            Discipline = _context.Disciplines.Select(dis => new
                            {
                                dis.ObjectID,
                                dis.Code
                            }).FirstOrDefault(dis => dis.ObjectID == kim.Discipline)
                        }).FirstOrDefault(kim => kim.ObjectID == ek.KIM)
                    }).FirstOrDefault(ek=>ek.ObjectID==x.ElectronicKIMID)
                });
            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "ExamKimID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(examkims, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new ExamKim();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            //TODO: проверка создания варианта на экзамен
            var newExamKim =
                _context.ExamKims.FirstOrDefault(e =>
                    (e.ExamID == model.ExamID && e.KIM==model.KIM && e.VariantNumber==model.VariantNumber) || 
                    (e.ExamID == model.ExamID && e.KIM == model.KIM && e.VariantNumber == model.VariantNumber));

            var result = _context.ExamKims.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.ExamKimID);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.ExamKims.FirstOrDefault(item => item.ExamKimID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "ExamKim not found");

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
            var model = _context.ExamKims.FirstOrDefault(item => item.ExamKimID == key);

            _context.ExamKims.Remove(model);
            _context.SaveChanges();
        }


        //[HttpGet]
        //public HttpResponseMessage ExamsLookup(DataSourceLoadOptions loadOptions) {
        //    var lookup = _context.Exams.OrderBy(i => i.ExamViewNameWithoutClass)
        //        .Select(i => new {Value = i.ExamID, Text = i.ExamViewNameWithoutClass});
        //    return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        //}

        [HttpGet]
        public async Task<HttpResponseMessage> ExamsLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Exams
                .Include(e => e.Subject)
                .Include(e=>e.ExamType)
                .Where(e => (e.Flags & 128) != 128);
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> KimsLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.KIMs;
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> ElectronicKimsLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.ElectronicKIMs
                .Include(ek=>ek.KIM1)
                .Where(ek=>(ek.Flags&1)==1 && (ek.Flags & 128) != 128);
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        private void PopulateModel(ExamKim model, IDictionary values) {
            string EXAM_KIM_ID = nameof(ExamKim.ExamKimID);
            string EXAM_ID = nameof(ExamKim.ExamID);
            string VARIANT_NUMBER = nameof(ExamKim.VariantNumber);
            string KIM = nameof(ExamKim.KIM);
            string ELECTRONIC_KIMID = nameof(ExamKim.ElectronicKIMID);
            string PARENT_EXAM_KIM_ID = nameof(ExamKim.ParentExamKimID);

            if(values.Contains(EXAM_KIM_ID)) {
                model.ExamKimID = Convert.ToInt32(values[EXAM_KIM_ID]);
            }

            if(values.Contains(EXAM_ID)) {
                model.ExamID = Convert.ToInt32(values[EXAM_ID]);
            }

            if(values.Contains(VARIANT_NUMBER)) {
                model.VariantNumber = Convert.ToInt32(values[VARIANT_NUMBER]);
            }

            if(values.Contains(KIM)) {
                model.KIM = new Guid(Convert.ToString(values[KIM]));
            }

            if(values.Contains(ELECTRONIC_KIMID)) {
                model.ElectronicKIMID = values[ELECTRONIC_KIMID] != null ? new Guid(Convert.ToString(values[ELECTRONIC_KIMID])) : (Guid?)null;
            }

            if(values.Contains(PARENT_EXAM_KIM_ID)) {
                model.ParentExamKimID = values[PARENT_EXAM_KIM_ID] != null ? Convert.ToInt32(values[PARENT_EXAM_KIM_ID]) : (int?)null;
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