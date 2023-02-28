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
using DevExpress.Data.ODataLinq.Helpers;
using DevExpress.Utils.Extensions;
using Microsoft.Ajax.Utilities;
using WebDtt.Models;
using WebDtt.Models.CustomTypes;
using WebDtt.Models.Extentions;

namespace WebDtt.Controllers.Api
{
    [Authorize]
    [Route("api/DonateStatus/{action}", Name = "DonateStatus")]
    public class DonateStatusController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        [Authorize(Roles = "user,manager,operator")]
        public async Task<HttpResponseMessage> GetDonateStatusToOrder(int orderId, DataSourceLoadOptions loadOptions)
        {
            var ds = _context.DonateStatus.Include(d=>d.File).Where(d => d.OrderID == orderId);
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(ds, loadOptions));
        }



        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.DonateStatus.FirstOrDefault(d => d.DonateID == key);
            

            if (model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Donate not found");

            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());
            if (user.IsInRole("user"))
            {
                var order = _context.Orders.Include(o => o.Person1).FirstOrDefault(o => o.OrderID == model.OrderID);
                if (order != null && order.Person1.UserID != userid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        $"Вам не принадлежат эти данные!");
                }

                if (model.Flags > 1)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Чек обработан. Вы больше не можете его изменять.");
                }
            }

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
        public HttpResponseMessage Delete(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.DonateStatus.FirstOrDefault(d => d.DonateID == key);


            if (model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Donate not found");

            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());
            if (user.IsInRole("user"))
            {
                var order = _context.Orders.Include(o => o.Person1).FirstOrDefault(o => o.OrderID == model.OrderID);
                if (order != null && order.Person1.UserID != userid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        $"Вам не принадлежат эти данные!");
                }

                if (model.Flags > 1)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Чек обработан. Вы больше не можете его изменять.");
                }
            }

            model.Flags = 128;
            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private void PopulateModel(DonateStatu model, IDictionary values)
        {
            string DONATE_DATE = nameof(DonateStatu.DonateDate);
            string PART_OF_DONATE = nameof(DonateStatu.PartOfDonate);
            string TOTAL = nameof(DonateStatu.Total);
            string FLAGS = nameof(DonateStatu.Flags);

            if (values.Contains(DONATE_DATE))
            {
                model.DonateDate = Convert.ToDateTime(values[DONATE_DATE]);
            }

            if (values.Contains(PART_OF_DONATE))
            {
                model.PartOfDonate = Convert.ToByte(values[PART_OF_DONATE]);
            }

            if (values.Contains(TOTAL))
            {
                model.Total = Convert.ToInt32(values[TOTAL]);
            }

            if(values.Contains(FLAGS))
            {
                model.Flags = Convert.ToInt32(values[FLAGS]);
            }
        }

        private string GetFullErrorMessage(ModelStateDictionary modelState)
        {
            var messages = new List<string>();

            foreach (var entry in modelState)
            {
                foreach (var error in entry.Value.Errors)
                    messages.Add(error.ErrorMessage);
            }

            return String.Join(" ", messages);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
