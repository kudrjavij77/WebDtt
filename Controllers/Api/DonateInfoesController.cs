using System;
using System.Collections;
using System.Collections.Generic;
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
    [Authorize(Roles = "manager")]
    [Route("api/DonateInfoes/{action}", Name = "DonateInfoes")]
    public class DonateInfoesController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var donateinfoes = _context.DonateInfoes.Select(i => new {
                i.DonateInfoID,
                i.DonateNumber,
                i.Total,
                i.EndDate,
                i.Description,
                i.Flags,
                i.DonateTypeID,
                i.TotalString
            }).OrderBy(o=>o.DonateTypeID).OrderBy(o=>o.DonateNumber);

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "DonateInfoID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(DataSourceLoader.Load(donateinfoes, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new DonateInfo();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.DonateInfoes.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.DonateInfoID);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.DonateInfoes.FirstOrDefault(item => item.DonateInfoID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "DonateInfo not found");

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
            var model = _context.DonateInfoes.FirstOrDefault(item => item.DonateInfoID == key);

            _context.DonateInfoes.Remove(model);
            _context.SaveChanges();
        }


        [HttpGet]
        public HttpResponseMessage DonateTypesLookup(DataSourceLoadOptions loadOptions) {

            
            var lookup = _context.DonateTypes;
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        private void PopulateModel(DonateInfo model, IDictionary values) {
            string DONATE_INFO_ID = nameof(DonateInfo.DonateInfoID);
            string DONATE_NUMBER = nameof(DonateInfo.DonateNumber);
            string TOTAL = nameof(DonateInfo.Total);
            string END_DATE = nameof(DonateInfo.EndDate);
            string DESCRIPTION = nameof(DonateInfo.Description);
            string FLAGS = nameof(DonateInfo.Flags);
            string DONATE_TYPE_ID = nameof(DonateInfo.DonateTypeID);
            string TOTAL_STRING = nameof(DonateInfo.TotalString);

            if(values.Contains(DONATE_INFO_ID)) {
                model.DonateInfoID = Convert.ToInt32(values[DONATE_INFO_ID]);
            }

            if(values.Contains(DONATE_NUMBER)) {
                model.DonateNumber = Convert.ToByte(values[DONATE_NUMBER]);
            }

            if(values.Contains(TOTAL)) {
                model.Total = Convert.ToInt32(values[TOTAL]);
            }

            if(values.Contains(END_DATE)) {
                model.EndDate = values[END_DATE] != null ? Convert.ToDateTime(values[END_DATE]) : (DateTime?)null;
            }

            if(values.Contains(DESCRIPTION)) {
                model.Description = Convert.ToString(values[DESCRIPTION]);
            }

            if(values.Contains(FLAGS)) {
                model.Flags = Convert.ToInt32(values[FLAGS]);
            }

            if(values.Contains(DONATE_TYPE_ID)) {
                model.DonateTypeID = values[DONATE_TYPE_ID] != null ? Convert.ToInt32(values[DONATE_TYPE_ID]) : (int?)null;
            }

            if(values.Contains(TOTAL_STRING)) {
                model.TotalString = Convert.ToString(values[TOTAL_STRING]);
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