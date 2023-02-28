using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DevExpress.XtraRichEdit;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using WebDtt.Models;

namespace WebDtt.Controllers.Api
{
    [Authorize]
    public class ChartsController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        public HttpResponseMessage StudentExamsCountOnExam(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context
                .Exams.Include(e => e.Subject)
                .Include(e => e.StudentExams);
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }
    }
}
