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
using DevExpress.Utils.Extensions;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using WebDtt.Models;
using WebDtt.Models.Extentions;

namespace WebDtt.Controllers.Api
{
    [Authorize(Roles = "admin,operator,manager")]
    [Route("api/StationExams/{action}", Name = "StationExamsApi")]
    public class StationExamsController : ApiController
    {
        
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false}
        };

        private ValidationModels _valmod = new ValidationModels();
        
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var stationexams = _context.StationExams
                .Include(se => se.Exam.Subject)
                .Include(se => se.Station)
                //.Include(se=>se.StudentExams)
                .ToList();

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "StationExamID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(DataSourceLoader.Load(stationexams, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetStationExam(DataSourceLoadOptions loadOptions)
        {
            var stationExams = _context.StationExams.Include(s => s.Station);
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(stationExams, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new StationExam();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);



            if (model.ExamStartupTime.Hours >= 21)
            {
                model.ExamStartupTime = model.ExamStartupTime.Add(new TimeSpan(3, 0, 0));
                model.ExamStartupTime = model.ExamStartupTime.Add(new TimeSpan(model.ExamStartupTime.Hours - 24, 0, 0));
            }
            else
            {
                model.ExamStartupTime = model.ExamStartupTime.Add(new TimeSpan(3, 0, 0));
            }
            
            
            
            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            _valmod.ValidateRulesForStationExams(model);

            model.CreateDate=DateTime.Now;

            var result = _context.StationExams.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.StationExamID);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.StationExams.FirstOrDefault(item => item.StationExamID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "StationExam not found");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            model.UpdateDate=DateTime.Now.ToLocalTime();

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            _valmod.ValidateRulesForStationExams(model);

            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        public void Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.StationExams.FirstOrDefault(item => item.StationExamID == key);

            _context.StationExams.Remove(model);
            _context.SaveChanges();
        }


        [HttpGet]
        public HttpResponseMessage ExamsLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Exams
                .Include(e=>e.Subject)
                .Include(e=>e.ExamType)
                .Where(e=>e.ExamTypeID==1)
                .ToList();
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage StationsLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Stations.ToList();
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        private void PopulateModel(StationExam model, IDictionary values) {
            string STATION_EXAM_ID = nameof(StationExam.StationExamID);
            string STATION_ID = nameof(StationExam.StationID);
            string EXAM_ID = nameof(StationExam.ExamID);
            string RESERVED_CAPACITY = nameof(StationExam.ReservedCapacity);
            string STATION_PRIORITY = nameof(StationExam.StationPriority);
            string FLAGS = nameof(StationExam.Flags);
            string CREATE_DATE = nameof(StationExam.CreateDate);
            string UPDATE_DATE = nameof(StationExam.UpdateDate);
            string EXAM_STARTUP_TIME = nameof(StationExam.ExamStartupTime);

            if(values.Contains(STATION_EXAM_ID)) {
                model.StationExamID = Convert.ToInt32(values[STATION_EXAM_ID]);
            }

            if(values.Contains(STATION_ID)) {
                model.StationID = Convert.ToInt32(values[STATION_ID]);
            }

            if(values.Contains(EXAM_ID)) {
                model.ExamID = Convert.ToInt32(values[EXAM_ID]);
            }

            if(values.Contains(RESERVED_CAPACITY)) {
                model.ReservedCapacity = Convert.ToInt32(values[RESERVED_CAPACITY]);
            }

            if(values.Contains(STATION_PRIORITY)) {
                model.StationPriority = Convert.ToInt32(values[STATION_PRIORITY]);
            }

            if(values.Contains(FLAGS)) {
                model.Flags = Convert.ToInt32(values[FLAGS]);
            }

            if(values.Contains(CREATE_DATE)) {
                model.CreateDate = Convert.ToDateTime(values[CREATE_DATE]);
            }

            if(values.Contains(UPDATE_DATE)) {
                model.UpdateDate = values[UPDATE_DATE] != null ? Convert.ToDateTime(values[UPDATE_DATE]) : (DateTime?)null;
            }

            if(values.Contains(EXAM_STARTUP_TIME))
            {
                var t = Convert.ToDateTime(values[EXAM_STARTUP_TIME]);
                model.ExamStartupTime = new TimeSpan(t.Hour,t.Minute,00);
                //model.ExamStartupTime = t.TimeOfDay;
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