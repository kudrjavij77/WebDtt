using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using WebDtt.Models;

namespace WebDtt.Controllers.Api
{
    [Authorize(Roles = "admin")]
    [Route("api/RolesApi/{action}", Name = "RolesApi")]
    public class RolesController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var roles = _context.Roles.Select(i => new {
                i.RoleID,
                i.RoleName,
                i.RoleDescription,
                i.Descriminator
            }).ToList();

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "RoleID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(DataSourceLoader.Load(roles, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new Role();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.Roles.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.RoleID);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Roles.FirstOrDefault(item => item.RoleID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Role not found");

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
            var model = _context.Roles.FirstOrDefault(item => item.RoleID == key);

            _context.Roles.Remove(model);
            _context.SaveChanges();
        }


        private void PopulateModel(Role model, IDictionary values) {
            string ROLE_ID = nameof(Role.RoleID);
            string ROLE_NAME = nameof(Role.RoleName);
            string ROLE_DESCRIPTION = nameof(Role.RoleDescription);
            string DESCRIMINATOR = nameof(Role.Descriminator);

            if(values.Contains(ROLE_ID)) {
                model.RoleID = Convert.ToInt32(values[ROLE_ID]);
            }

            if(values.Contains(ROLE_NAME)) {
                model.RoleName = Convert.ToString(values[ROLE_NAME]);
            }

            if(values.Contains(ROLE_DESCRIPTION)) {
                model.RoleDescription = Convert.ToString(values[ROLE_DESCRIPTION]);
            }

            if(values.Contains(DESCRIMINATOR)) {
                model.Descriminator = Convert.ToString(values[DESCRIMINATOR]);
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