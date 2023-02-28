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
    [Route("api/DonateTypes/{action}", Name = "DonateTypes")]
    public class DonateTypesController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var donatetypes = _context.DonateTypes.Select(i => new {
                i.DonateTypeID,
                i.FullTotal,
                i.CountDonates,
                i.Flags,
                i.FullTotalString,
                DonateInfoes = _context.DonateInfoes.Where(d=>d.DonateTypeID==i.DonateTypeID).ToList()
            });

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "DonateTypeID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(DataSourceLoader.Load(donatetypes, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new DonateType();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.DonateTypes.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.DonateTypeID);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.DonateTypes.FirstOrDefault(item => item.DonateTypeID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "DonateType not found");

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
            var model = _context.DonateTypes.FirstOrDefault(item => item.DonateTypeID == key);

            _context.DonateTypes.Remove(model);
            _context.SaveChanges();
        }

        [HttpGet]
        public async Task<HttpResponseMessage> DonateInfosLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.DonateInfoes;
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }


        private void PopulateModel(DonateType model, IDictionary values) {
            string DONATE_TYPE_ID = nameof(DonateType.DonateTypeID);
            string FULL_TOTAL = nameof(DonateType.FullTotal);
            string COUNT_DONATES = nameof(DonateType.CountDonates);
            string FLAGS = nameof(DonateType.Flags);
            string FULL_TOTAL_STRING = nameof(DonateType.FullTotalString);

            if(values.Contains(DONATE_TYPE_ID)) {
                model.DonateTypeID = Convert.ToInt32(values[DONATE_TYPE_ID]);
            }

            if(values.Contains(FULL_TOTAL)) {
                model.FullTotal = Convert.ToInt32(values[FULL_TOTAL]);
            }

            if(values.Contains(COUNT_DONATES)) {
                model.CountDonates = Convert.ToByte(values[COUNT_DONATES]);
            }

            if(values.Contains(FLAGS)) {
                model.Flags = Convert.ToInt32(values[FLAGS]);
            }

            if(values.Contains(FULL_TOTAL_STRING)) {
                model.FullTotalString = Convert.ToString(values[FULL_TOTAL_STRING]);
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