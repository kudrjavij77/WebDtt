using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.Utils.Extensions;
using WebDtt.Models;
using WebDtt.Reports;

namespace WebDtt.Controllers
{
    [Authorize(Roles = "manager")]
    public class ReportController : Controller
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };
        // GET: Report

        //[HttpGet]
        //public ActionResult AttendanceStatement()
        //{
        //    return View();
        //}

        //[HttpPost]
        public ActionResult AttendanceStatement(int id)
        {
            var groups = _context.Groups
                .Where(g => g.GroupID == id).ToList();

            var group = groups.First();

            foreach (var personGroup in _context.PersonGroups.Where(pg=>pg.GroupID==group.GroupID && (pg.Flags&128)!=128).ToList())
            {
                var persons = _context.Persons.Where(p => p.PersonID == personGroup.PersonID).ToList();
                personGroup.Person = persons.First();
                group.PersonGroups.Add(personGroup);
            }

            var dataToReport = new WebDtt.Models.Dto.AttendanceStatementDto(group);
            //var path = Server.MapPath($"~/Files/temp/Ведомость учета посещаемости {dataToReport.GroupName}.pdf");
            //var f = new FileInfo(path);
            //if (f.Exists) f.Delete();

            var stream = new MemoryStream();

            using (var attStatement = new Reports.Attedance() { DataSource = dataToReport })
            {
                attStatement.ExportToPdf(stream);
            }

            return File(stream, "application/pdf", $"Ведомость учета посещаемости {dataToReport.GroupName}.pdf");
        }

    }
}