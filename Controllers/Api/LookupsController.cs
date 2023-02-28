using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;
using DevExpress.Utils.Extensions;
using DevExpress.XtraGauges.Win.Gauges.Circular;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using WebDtt.Models;
using WebDtt.Models.Dto;
using static System.Data.Entity.Core.Objects.EntityFunctions;

namespace WebDtt.Controllers.Api
{
    [Route("api/Lookups/{action}", Name = "LookupsApi")]
    public class LookupsController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        public async Task<HttpResponseMessage> EducationFormLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.EducationForms.Select(x => new
            {
                x.EducationFormID,
                x.Description,
                x.EducationFormName,
                x.Flags
            }).Where(x => (x.Flags & 128) != 128);

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> SubjectsLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Subjects.Select(x => new
            {
                x.SubjectID,
                x.SubjectName,
                x.SubjectCode,
                x.ShortSubjectName
            }).Where(x=>x.SubjectCode!=14).Where(x=>x.SubjectCode!=13);
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage GroupsLookup(DataSourceLoadOptions loadOptions)
        {
            DateTime firstSunday = new DateTime(1753, 1, 7);
            var lookup = _context.Groups.Select(x => new
            {
                x.GroupID,
                x.GroupName,
                x.Capacity,
                x.CurriculumID,
                Class = _context.Curricula.FirstOrDefault(c=>c.CurriculumID==x.CurriculumID).Class,
                Subject = _context.Curricula.FirstOrDefault(c=>c.CurriculumID==x.CurriculumID).SubjectID,
                Description = _context.Curricula.Select(c => new
                {
                    c.CurriculumID,
                    c.Description
                }).FirstOrDefault(c => c.CurriculumID == x.CurriculumID),
                x.Duration,
                x.LessonDuration,
                x.EducationFormID,
                x.Flags,
                x.Price,
                x.StartDateTime,
                x.StationID,
                x.AuditoriumNumber,
                FreePlaces = x.Capacity - _context.PersonGroups.Count(pg => pg.GroupID==x.GroupID && pg.OrderID!=null && (pg.Flags&128)!= 128 && (pg.Flags & 4) != 4),
                DonateTypes = _context.DonateTypes.Select(d => new
                {
                    d.DonateTypeID,
                    d.CountDonates,
                    d.FullTotal,
                    d.Flags,
                    DonateInfos = _context.DonateInfoes.Where(di => di.DonateTypeID == d.DonateTypeID),
                    ViewName = d.FullTotal + " руб." + "(этапы оплаты: " + d.CountDonates + ")"
                }).Where(d => _context.Groups.FirstOrDefault(gr => gr.GroupID == x.GroupID)
                    .DonateTypes.Contains(_context.DonateTypes.FirstOrDefault(dt => dt.DonateTypeID == d.DonateTypeID))),
                DayOfWeek = DiffDays(firstSunday, x.StartDateTime) % 7,
                DayOfWeekLesson = x.StartDateTime,
                TimeLesson = x.StartDateTime,
                FirstLesson = x.StartDateTime,
                //ViewDisplayName = x.GroupName + " - " + x.Duration + "ч. - " + x.StartDateTime.ToLongDateString() + " - " + x.StartDateTime.TimeOfDay,
                Station = _context.Stations.FirstOrDefault(s => s.StationID == x.StationID).StationAddress
            }).Where(x=>x.Flags==0).OrderBy(x=>x.StartDateTime);

            return Request.CreateResponse( DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> DonateTypesLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.DonateTypes.Select(d=>new
            {
                d.DonateTypeID,
                d.CountDonates,
                d.FullTotal,
                d.Flags,
                //DonateInfos = _context.DonateInfoes.Where(di => di.DonateTypeID == d.DonateTypeID),
                ViewName = d.FullTotal + "(" + d.CountDonates + ")"
            });
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        [Authorize]
        public async Task<HttpResponseMessage> GetFileLookup(int fileID, DataSourceLoadOptions loadOptions)
        {
            var l = _context.Files.Where(f => f.FileID == fileID);
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(l, loadOptions));
        }

        [HttpGet]
        [Authorize(Roles = "user,manager,operator")]
        public async Task<HttpResponseMessage> GetDonateInfosToOrder(int orderId, DataSourceLoadOptions loadOptions)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);
            var ds = _context.DonateInfoes.Where(d => d.DonateTypeID == order.DonateTypeID).OrderBy(d=>d.DonateNumber);

            foreach (var donateInfo in ds)
            {
                if (order!=null && donateInfo.DonateNumber == 1) donateInfo.EndDate = order.CreateDate.AddDays(5);
                if (order != null) donateInfo.Total = donateInfo.Total * (100 - order.Discount) / 100;
            }
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(ds, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> SchedularGroups(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Groups.Select(g=>new
            {
                g.GroupID,
                g.GroupName,
                g.StartDateTime,
                DayOfWeek = g.StartDateTime,
                FirstLesson = g.StartDateTime,
                g.CurriculumID,
                Class = g.Curriculum.Class,
                SubjectName = g.Curriculum.Subject.SubjectName,
                g.Capacity,
                g.EducationFormID,
                EducationFormName = g.EducationForm.EducationFormName,
                EducationForm = g.EducationForm.EducationFormName,
                FreePlaces = g.Capacity - _context.PersonGroups.Count(pg => pg.GroupID == g.GroupID && pg.OrderID != null && (pg.Flags & 128) != 128 && (pg.Flags & 4) != 4),
                g.Flags,
                g.Duration,
                g.LessonDuration,
                Adress = g.Station.StationAddress
            }).Where(g=>g.Flags<=1);
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GroupsToAllOrdersKpc(DataSourceLoadOptions loadOptions)
        {
            var groups = _context.Groups.Select(g =>new
            {
                g.GroupID,
                g.GroupName
            });
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(groups, loadOptions));
        }

        [HttpGet]
        [Authorize]
        public HttpResponseMessage GetDonateBarData(int orderId, DataSourceLoadOptions loadOptions)
        {
            var lookup = new List<OrderDonate>();
            var orders = _context.Orders.Select(x => new
            {
                x.OrderID,
                x.DonateTypeID,
                x.CreateDate,
                x.OrderTypeID,
                x.StudentPersonID,
                x.CreatorPersonID,
                x.Flags,
                x.Discount,
                DonateType = _context.DonateTypes.FirstOrDefault(dt => dt.DonateTypeID == x.DonateTypeID),
                DonateStatus = _context.DonateStatus.Where(ds=>ds.OrderID==x.OrderID && ds.Flags < 4).ToList()
                
            }).Where(x => x.OrderID == orderId).ToList();

            foreach (var order in orders)
            {
                var donateInfoes = _context.DonateInfoes.Where(di => di.DonateTypeID == order.DonateTypeID).ToList();
                order.DonateType.FullTotal = order.DonateType.FullTotal * (100 - order.Discount) / 100;
                foreach (var info in donateInfoes)
                {
                        if (info.DonateNumber==1) info.EndDate = order.CreateDate.AddDays(5);
                        info.Total = info.Total * (100 - order.Discount) / 100;
                        order.DonateType.DonateInfoes.Add(info);
                }
                lookup.Add(new OrderDonate(order.OrderID, order.DonateStatus, order.DonateType));
            }

            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }
    }
}
