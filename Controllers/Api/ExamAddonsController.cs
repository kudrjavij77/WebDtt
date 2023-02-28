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
    [Route("api/ExamAddons/{action}", Name = "ExamAddonsApi")]
    public class ExamAddonsController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        [Authorize(Roles = "operator,manager,admin")]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var examaddons = _context.ExamAddons.Select(i => new {
                i.ExamAddonID,
                i.ExamID,
                i.AddonShortName,
                i.AddonDescription,
                i.LinkAddress,
                i.StartDate,
                i.StartTime,
                i.Flags
            });

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "ExamAddonID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(DataSourceLoader.Load(examaddons, loadOptions));
        }

        [HttpPost]
        [Authorize(Roles = "operator,manager,admin")]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new ExamAddon();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.ExamAddons.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.ExamAddonID);
        }

        [HttpPut]
        [Authorize(Roles = "operator,manager,admin")]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.ExamAddons.FirstOrDefault(item => item.ExamAddonID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "ExamAddon not found");

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
        [Authorize(Roles = "operator,manager,admin")]
        public void Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.ExamAddons.FirstOrDefault(item => item.ExamAddonID == key);

            if (model != null) model.Flags = 128;
            _context.SaveChanges();
        }


        [HttpGet]
        public async Task<HttpResponseMessage> ExamsLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Exams
                .Include(e => e.Subject)
                .Include(e=>e.ExamType);
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        private void PopulateModel(ExamAddon model, IDictionary values) {
            string EXAM_ADDON_ID = nameof(ExamAddon.ExamAddonID);
            string EXAM_ID = nameof(ExamAddon.ExamID);
            string ADDON_SHORT_NAME = nameof(ExamAddon.AddonShortName);
            string ADDON_DESCRIPTION = nameof(ExamAddon.AddonDescription);
            string LINK_ADDRESS = nameof(ExamAddon.LinkAddress);
            string START_DATE = nameof(ExamAddon.StartDate);
            string START_TIME = nameof(ExamAddon.StartTime);
            string FLAGS = nameof(ExamAddon.Flags);

            if(values.Contains(EXAM_ADDON_ID)) {
                model.ExamAddonID = Convert.ToInt32(values[EXAM_ADDON_ID]);
            }

            if(values.Contains(EXAM_ID)) {
                model.ExamID = values[EXAM_ID] != null ? Convert.ToInt32(values[EXAM_ID]) : (int?)null;
            }

            if(values.Contains(ADDON_SHORT_NAME)) {
                model.AddonShortName = Convert.ToString(values[ADDON_SHORT_NAME]);
            }

            if(values.Contains(ADDON_DESCRIPTION)) {
                model.AddonDescription = Convert.ToString(values[ADDON_DESCRIPTION]);
            }

            if(values.Contains(LINK_ADDRESS)) {
                model.LinkAddress = Convert.ToString(values[LINK_ADDRESS]);
            }

            if (values.Contains(START_DATE))
            {
                model.StartDate = values[START_DATE] != null ? Convert.ToDateTime(values[START_DATE]).Date : (DateTime?)null;
            }

            if (values.Contains(START_TIME))
            {
                var t = Convert.ToDateTime(values[START_TIME]);
                model.StartTime = new TimeSpan(t.Hour, t.Minute, 00);
            }

            if (values.Contains(FLAGS))
            {
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