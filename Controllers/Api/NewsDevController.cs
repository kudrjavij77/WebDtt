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
using WebDtt.Models;

namespace WebDtt.Controllers
{
    [Route("api/NewsDev/{action}", Name = "NewsDevApi")]
    [Authorize(Roles = "manager")]
    public class NewsDevController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> Get(DataSourceLoadOptions loadOptions) {
            var news = _context.News.Select(i => new {
                i.NewsID,
                i.Title,
                i.Body,
                i.Creator,
                i.Flags,
                i.CreateDate,
                i.UpdateDate,
                i.DeleteDate
            });

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "NewsID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(news, loadOptions));
        }

        [HttpPost]
        [Authorize(Roles = "admin,manager")]
        public async Task<HttpResponseMessage> Post(FormDataCollection form) {
            var model = new News();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));

            var user = HttpContext.Current.GetOwinContext().Authentication.User;
            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());
            model.Creator = id;
            model.CreateDate = DateTime.Now;

            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.News.Add(model);
            await _context.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.Created, new { result.NewsID });
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = await _context.News.FirstOrDefaultAsync(item => item.NewsID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Object not found");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            model.UpdateDate = DateTime.Now;

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            await _context.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        public async Task Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = await _context.News.FirstOrDefaultAsync(item => item.NewsID == key);
            if (model != null)
            {
                model.DeleteDate = DateTime.Now;
                if ((model.Flags & 128) != 128) model.Flags += 128;
            }
            //_context.News.Remove(model);
            await _context.SaveChangesAsync();
        }


        [HttpGet]
        public async Task<HttpResponseMessage> UsersLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Users
                         orderby i.Email
                         select new {
                             Value = i.UserID,
                             Text = i.Email
                         };
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        private void PopulateModel(News model, IDictionary values) {
            string NEWS_ID = nameof(News.NewsID);
            string TITLE = nameof(News.Title);
            string BODY = nameof(News.Body);
            string CREATOR = nameof(News.Creator);
            string FLAGS = nameof(News.Flags);
            string CREATE_DATE = nameof(News.CreateDate);
            string UPDATE_DATE = nameof(News.UpdateDate);
            string DELETE_DATE = nameof(News.DeleteDate);

            if(values.Contains(NEWS_ID)) {
                model.NewsID = Convert.ToInt32(values[NEWS_ID]);
            }

            if(values.Contains(TITLE)) {
                model.Title = Convert.ToString(values[TITLE]);
            }

            if(values.Contains(BODY)) {
                model.Body = Convert.ToString(values[BODY]);
            }

            if(values.Contains(CREATOR)) {
                model.Creator = ConvertTo<System.Guid>(values[CREATOR]);
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

            if(values.Contains(DELETE_DATE)) {
                model.DeleteDate = values[DELETE_DATE] != null ? Convert.ToDateTime(values[DELETE_DATE]) : (DateTime?)null;
            }
        }

        private T ConvertTo<T>(object value) {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
            if(converter != null) {
                return (T)converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
            } else {
                // If necessary, implement a type conversion here
                throw new NotImplementedException();
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