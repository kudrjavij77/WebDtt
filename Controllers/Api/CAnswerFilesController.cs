using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DevExpress.XtraMap;
using WebDtt.Models;

namespace WebDtt.Models.Controllers
{
    [Authorize]
    [Route("api/CAnswerFiles/{action}", Name = "CAnswerFilesApi")]
    public class CAnswerFilesController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };


        [HttpGet]
        public async Task<HttpResponseMessage> Get(DataSourceLoadOptions loadOptions)
        {
            var canswerfiles = _context.CAnswerFiles.Include(ca=>ca.File);

            var result = await DataSourceLoader.LoadAsync(canswerfiles, loadOptions);
            return Request.CreateResponse(result);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetCAnswerFilesByStudentExamId(int studentExamId, DataSourceLoadOptions loadOptions)
        {
            var canswerfiles = _context.CAnswerFiles
                .Where(ca=>ca.StudentExamID==studentExamId);

            var result = await DataSourceLoader.LoadAsync(canswerfiles, loadOptions);
            return Request.CreateResponse(result);
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new CAnswerFile();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.CAnswerFiles.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.CAnswerFileID);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.CAnswerFiles
                .Include(c => c.File)
                .Include(c => c.StudentExam.Person)
                .FirstOrDefault(item => item.CAnswerFileID == key);
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());
            if (model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "CAnswerFile not found");

            if(userid != model.StudentExam.Person.UserID && user.IsInRole("user"))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    $"Невозможно изменить файл, так как он не принадлежит пользователю");

            if(user.IsInRole("user") && 
               DateTime.Now > model.StudentExam.FinishDateTime + new TimeSpan(23, 59, 59) && 
               DateTime.Now >= model.StudentExam.FinishDateTime)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    $"Невозможно изменить файл, так как время на изменение прикрепленных файлов истекло");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        public HttpResponseMessage Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.CAnswerFiles
                .Include(c=>c.File)
                .Include(c=>c.StudentExam.Person)
                .FirstOrDefault(item => item.CAnswerFileID == key);

            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            if(model != null && userid!=model.StudentExam.Person.UserID && user.IsInRole("user"))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    $"Невозможно удалить файл, так как он не принадлежит пользователю");

            var stopChangedDateTime = model.StudentExam.FinishDateTime + new TimeSpan(23, 59, 59);
            if (model.StudentExam.FinishDateTime != null && stopChangedDateTime < DateTime.Now)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    $"Невозможно удалить файл, так как время изменения истекло");

            var file = model.File;
            _context.CAnswerFiles.Remove(model);
            var f = new FileInfo(file.FullPath);
            var d = new DirectoryInfo(Path.Combine(file.FullPath.Replace(file.FileName, "Deleted")));
            if(!d.Exists) d.Create();
            var delFile = Path.Combine(d.FullName, file.FileName);

            if (new FileInfo(delFile).Exists) f.Delete();
            else f.MoveTo(Path.Combine(d.FullName, file.FileName));

            _context.Files.Remove(file);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [HttpGet]
        public HttpResponseMessage FilesLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Files
                         orderby i.FileName
                         select new {
                             Value = i.FileID,
                             Text = i.FileName
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage StudentExamsLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.StudentExams
                         orderby i.PersonID
                         select new {
                             Value = i.StudentExamID,
                             Text = i.PersonID
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        private void PopulateModel(CAnswerFile model, IDictionary values) {
            string PAGE_NUMBER = nameof(CAnswerFile.PageNumber);
            
            if(values.Contains(PAGE_NUMBER)) {
                model.PageNumber = values[PAGE_NUMBER] != null ? Convert.ToInt32(values[PAGE_NUMBER]) : (int?)null;
                model.UpdateDate = DateTime.Now;
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