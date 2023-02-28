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

namespace WebDtt.Models.Controllers
{
    [Authorize]
    [Route("api/Messages/{action}", Name = "MessagesApi")]
    public class MessagesController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        [Authorize(Roles="operator,manager")]
        public async Task<HttpResponseMessage> Get(DataSourceLoadOptions loadOptions) {
            var messages = _context.Messages;

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "MessageID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(messages, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new Message();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.Messages.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.MessageID);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Messages.FirstOrDefault(item => item.MessageID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Message not found");

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
            var model = _context.Messages.FirstOrDefault(item => item.MessageID == key);

            _context.Messages.Remove(model);
            _context.SaveChanges();
        }


        [HttpGet]
        public async Task<HttpResponseMessage> UsersLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Users;
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        private void PopulateModel(Message model, IDictionary values) {
            string MESSAGE_ID = nameof(Message.MessageID);
            string USER_ID = nameof(Message.UserID);
            string SUBJECT_STRING = nameof(Message.SubjectString);
            string MESSAGE_STRING = nameof(Message.MessageString);
            string CREATE_DATE = nameof(Message.CreateDate);
            string FLAGS = nameof(Message.Flags);

            if(values.Contains(MESSAGE_ID)) {
                model.MessageID = Convert.ToInt32(values[MESSAGE_ID]);
            }

            if(values.Contains(USER_ID)) {
                model.UserID = values[USER_ID] != null ? new Guid(Convert.ToString(values[USER_ID])) : (Guid?)null;
            }

            if(values.Contains(SUBJECT_STRING)) {
                model.SubjectString = Convert.ToString(values[SUBJECT_STRING]);
            }

            if(values.Contains(MESSAGE_STRING)) {
                model.MessageString = Convert.ToString(values[MESSAGE_STRING]);
            }

            if(values.Contains(CREATE_DATE)) {
                model.CreateDate = Convert.ToDateTime(values[CREATE_DATE]);
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