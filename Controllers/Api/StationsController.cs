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
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebDtt.Models;

namespace WebDtt.Controllers.Api
{
    [Authorize(Roles = "admin,operator,manager")]
    [Route("api/Stations/{action}", Name = "StationsApi")]
    public class StationsController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        public async Task<HttpResponseMessage> Get(DataSourceLoadOptions loadOptions) {
            var stations = _context.Stations;                       
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(stations, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new Station();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.Stations.Add(model);
            _context.SaveChanges();
            
            return Request.CreateResponse(HttpStatusCode.Created, result.StationID);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Stations.FirstOrDefault(item => item.StationID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Station not found");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            model.UpdateDate = DateTime.Now.ToLocalTime();

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        public void Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Stations.FirstOrDefault(item => item.StationID == key);

            _context.Stations.Remove(model);
            _context.SaveChanges();
        }


        private void PopulateModel(Station model, IDictionary values) {
            string STATION_ID = nameof(Station.StationID);
            string STATION_CODE = nameof(Station.StationCode);
            string KSA = nameof(Station.Ksa);
            string STATION_NAME = nameof(Station.StationName);
            string FULL_NAME = nameof(Station.FullName);
            string STATION_ADDRESS = nameof(Station.StationAddress);
            string CAPACITY = nameof(Station.Capacity);
            string FLAGS = nameof(Station.Flags);
            string CREATE_DATE = nameof(Station.CreateDate);
            string UPDATE_DATE = nameof(Station.UpdateDate);

            if(values.Contains(STATION_ID)) {
                model.StationID = Convert.ToInt32(values[STATION_ID]);
            }

            if (values.Contains(STATION_CODE)) {
                model.StationCode = Convert.ToInt32(values[STATION_CODE]);
            }

            if (values.Contains(STATION_NAME)) {
                model.StationName = Convert.ToString(values[STATION_NAME]);
            }

            if (values.Contains(KSA)) {
                model.Ksa = Convert.ToString(values[KSA]);
            }

            if (values.Contains(FULL_NAME)) {
                model.FullName = Convert.ToString(values[FULL_NAME]);
            }

            if(values.Contains(STATION_ADDRESS)) {
                model.StationAddress = Convert.ToString(values[STATION_ADDRESS]);
            }

            if(values.Contains(CAPACITY)) {
                model.Capacity = Convert.ToInt32(values[CAPACITY]);
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