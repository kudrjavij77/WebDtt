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
using DevExpress.Data.Mask;
using DevExpress.Data.ODataLinq.Helpers;
using DevExpress.Utils.Extensions;
using DevExpress.XtraGauges.Core.Model;
using Microsoft.Ajax.Utilities;
using WebDtt.Models;
using WebDtt.Models.CustomTypes;
using WebDtt.Models.Extentions;

namespace WebDtt.Models.Controllers
{
    [Authorize]
    [Route("api/Orders/{action}", Name = "OrdersApi")]
    public class OrdersController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = {LazyLoadingEnabled = false}
        };

        [HttpGet]
        [Authorize(Roles = "user")]
        public async Task<HttpResponseMessage> GetUserOrders(DataSourceLoadOptions loadOptions)
        {
            var user = HttpContext.Current.GetOwinContext().Authentication.User;
            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ??
                                throw new InvalidOperationException());
            var orders = _context.Orders
            .Include(o => o.Person1.PersonType)
            .Include(o => o.Person.PersonType)
            .Include(o => o.PersonGroups.Select(pg=>pg.Group))
            .Include(o => o.OrderType)
            .Where(o => o.Person1.UserID == id && (o.Flags & 128) != 128);

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(orders, loadOptions));
        }

        

        [HttpGet]
        [Authorize(Roles = "user")]
        public async Task<HttpResponseMessage> GetUserPersonsLookup(DataSourceLoadOptions loadOptions)
        {
            var user = HttpContext.Current.GetOwinContext().Authentication.User;
            if (user.IsInRole("user"))
            {
                var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ??
                                    throw new InvalidOperationException());
                var students = _context.Persons
                    .Include(p => p.PersonType)
                    .Include(p => p.StudentExams
                        .Select(se => se.Exam))
                    .Where(p => p.UserID == id && (p.Flags & 128) != 128);

                //var stex = _context.StudentExams.Include(se => se.Exam).ToList();

                return Request.CreateResponse(await DataSourceLoader.LoadAsync(students, loadOptions));
            }


            var studentss = _context.Persons
                .Include(p => p.PersonType);

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(studentss, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetExams(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Exams
                .Include(e => e.Subject)
                .Include(e => e.ExamType)
                ////TODO: если нужно зарегать на экзамен, дата которого прошла, нужно закоменить проверку даты
                .Where(e => /*e.TestDateTime > DateTime.Now &&*/ e.Flags == 0);
            var result = await DataSourceLoader.LoadAsync(lookup, loadOptions);
            return Request.CreateResponse(result);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetExamTypes(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.ExamTypes
                .Where(x =>
                    _context.Exams
                        .Any(e => e.ExamTypeID == x.ExamTypeID
                                  //&& e.TestDateTime > DateTime.Now
                                  ));
            var result = await DataSourceLoader.LoadAsync(lookup, loadOptions);
            return Request.CreateResponse(result);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetStudentExams(int orderId, DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.StudentExams
                .Include(st => st.Exam.Subject)
                .Include(st => st.Exam.ExamType)
                .Include(st => st.Exam.ExamAddons)
                .Include(st => st.StationExam.Station)
                .Where(st => st.OrderID == orderId);
            var result = await DataSourceLoader.LoadAsync(lookup, loadOptions);
            return Request.CreateResponse(result);
        }

        [HttpGet]
        [Authorize(Roles = "operator,manager")]
        public async Task<HttpResponseMessage> Get(DataSourceLoadOptions loadOptions)
        {
            //var orders = _context.Orders
            //    .Include(o => o.OrderType)
            //    .Where(o => o.OrderTypeID == 1);

            var orders = await _context.Orders.Select(x => new
            {
                x.OrderID,
                x.OrderTypeID,
                OrderType = _context.OrderTypes.FirstOrDefault(ot=>ot.OrderTypeID==x.OrderTypeID),
                OrderNumber = "",
                x.StudentPersonID,
                x.CreatorPersonID,
                x.CreateDate,
                x.UpdateDate,
                x.Flags
            }).Where(x => x.OrderTypeID == 1).ToListAsync();

            //var ors = orders.CastTo<List<Order>>();

            

            var result = DataSourceLoader.Load(orders, loadOptions);
            return Request.CreateResponse(result);
        }

        [HttpGet]
        [Authorize(Roles = "operator,manager")]
        public async Task<HttpResponseMessage> GetOrdersKpc(DataSourceLoadOptions loadOptions)
        {
            var datePlus5 = DateTime.Now.AddDays(5);
            
            var orders = _context.Orders.Select(o => new
            {
                o.OrderID,
                o.OrderTypeID,
                o.Flags,
                o.StudentPersonID,
                o.CreatorPersonID,
                o.CreateDate,
                o.UpdateDate,
                DeleteDate = _context.PersonGroups.FirstOrDefault(p=>p.OrderID==o.OrderID && p.PersonID==o.StudentPersonID).DeleteDate,
                EndEducationDate = _context.PersonGroups.FirstOrDefault(p => p.OrderID == o.OrderID && p.PersonID == o.StudentPersonID).EndEducationDateTime,
                o.DonateTypeID,
                o.Discount,
                GroupID = _context.PersonGroups.FirstOrDefault(p => p.OrderID == o.OrderID && p.PersonID == o.StudentPersonID).GroupID,
                MomDonate = (_context.PersonGroups.FirstOrDefault(p=>p.OrderID==o.OrderID && p.PersonID==o.StudentPersonID).Flags & 1) == 1 || 
                            (o.Flags & 16) == 16,
                EndEducation = (_context.PersonGroups.FirstOrDefault(pg=>pg.OrderID==o.OrderID).Flags & 128) == 128 || 
                               (_context.PersonGroups.FirstOrDefault(pg => pg.OrderID == o.OrderID).Flags & 4) == 4,
                Vozvrat = (o.Flags & 4) == 4,
                Deleted = (o.Flags & 128) == 128,
                AllPayed = _context.DonateTypes.FirstOrDefault(dt=>dt.DonateTypeID==o.DonateTypeID).FullTotal * (100 - o.Discount) / 100 - 
                           (_context.DonateStatus.Any(d => d.OrderID == o.OrderID && (d.Flags & 2) == 2) ?
                               _context.DonateStatus.Where(d => d.OrderID == o.OrderID && (d.Flags & 2) == 2)
                                   .Sum(d => d.Total) : 0) <= 0,
                Pereplata = _context.DonateTypes.FirstOrDefault(dt => dt.DonateTypeID == o.DonateTypeID).FullTotal * (100 - o.Discount) / 100 -
                            (_context.DonateStatus.Any(d => d.OrderID == o.OrderID && (d.Flags & 2) == 2) ?
                                _context.DonateStatus.Where(d => d.OrderID == o.OrderID && (d.Flags & 2) == 2)
                                    .Sum(d => d.Total) : 0),
                Comment = _context.PersonGroups.FirstOrDefault(pg=>pg.OrderID==o.OrderID).Comment,
                Student = _context.Persons.Select(x=>new
                {
                    x.PersonID, x.LastName, x.FirstName, x.Patronymic, x.Phones, x.Email,
                    Fio = x.LastName + " " + x.FirstName + " " + x.Patronymic 
                }).FirstOrDefault(x=>x.PersonID==o.StudentPersonID),
                Parent = _context.Persons.Select(x => new
                {
                    x.PersonID, x.LastName, x.FirstName, x.Patronymic, x.Phones, x.Email,
                    Fio = x.LastName + " " + x.FirstName + " " + x.Patronymic
                }).FirstOrDefault(x => x.PersonID == o.CreatorPersonID),
                DonateToProcess = _context.DonateStatus.Count(d => d.OrderID==o.OrderID && d.Flags==1),
                o.CountDonates,
                Green = _context.DonateStatus.Any(d => d.OrderID == o.OrderID && (d.Flags & 2) == 2) ?
                    _context.DonateStatus.Where(d => d.OrderID == o.OrderID && (d.Flags & 2) == 2)
                        .Sum(d => d.Total) : 0,
                Red = _context.DonateInfoes
                    .Where(di => (di.EndDate == null | datePlus5 >= (di.EndDate)) && di.DonateTypeID == o.DonateTypeID)
                    .Sum(di => di.Total) * (100 - o.Discount) / 100 > 
                          (_context.DonateStatus.Any(d => d.OrderID == o.OrderID && (d.Flags & 2) == 2) ?
                              _context.DonateStatus.Where(d => d.OrderID == o.OrderID && (d.Flags & 2) == 2)
                                  .Sum(d => d.Total) : 0 ) +
                          ( _context.DonateStatus.Any(d => d.OrderID == o.OrderID && (d.Flags & 1) == 1) ?
                              _context.DonateStatus.Where(d => d.OrderID == o.OrderID && (d.Flags & 1) == 1)
                                  .Sum(d => d.Total) : 0) ?
                    _context.DonateInfoes
                        .Where(di => (di.EndDate == null | datePlus5 >= (di.EndDate)) && di.DonateTypeID == o.DonateTypeID)
                        .Sum(di => di.Total) * (100 - o.Discount) / 100 -
                    (_context.DonateStatus.Any(d => d.OrderID == o.OrderID && (d.Flags & 2) == 2) ?
                        _context.DonateStatus.Where(d => d.OrderID == o.OrderID && (d.Flags & 2) == 2)
                            .Sum(d => d.Total) : 0) -
                    (_context.DonateStatus.Any(d => d.OrderID == o.OrderID && (d.Flags & 1) == 1) ?
                        _context.DonateStatus.Where(d => d.OrderID == o.OrderID && (d.Flags & 1) == 1)
                            .Sum(d => d.Total) : 0) : 0


            }).Where(o=>o.OrderTypeID==2);
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(orders, loadOptions));
        }

        [HttpGet]
        [Authorize(Roles = "operator,manager,expert")]
        public async Task<HttpResponseMessage> GetPersonLookup(DataSourceLoadOptions loadOptions)
        {
            var person = _context.Persons;
                //.Include(p => p.PersonType);

            var result = await DataSourceLoader.LoadAsync(person, loadOptions);
            return Request.CreateResponse(result);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            var model = new Order();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.Orders.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.OrderID);
        }

        [HttpPut]
        [Authorize(Roles = "manager")]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Orders.Include(o=>o.Person1)
                .FirstOrDefault(item => item.OrderID == key);            
            
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Order not found");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            string FLAGS = nameof(Order.Flags);
            if (values.Contains(FLAGS))
            {
                var flags = Convert.ToInt32(values[FLAGS]);
                if (model.Flags > flags) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, $"Статус договора нельзя изменить на предшествующий");

                var studExams = _context.StudentExams
                .Include(st => st.Exam)
                .Where(st => st.OrderID == key);
                foreach (var studentExam in studExams) { model?.StudentExams.Add(studentExam); }
                //var user = _context.Users.FirstOrDefault(u => u.UserID == model.Person1.UserID);
                //var emailService = new EmailServiceSend();
                //if (user != null) await emailService.ChangeStatusOrder(user.Email, model);
            }

            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Authorize(Roles = "manager, user")]
        public HttpResponseMessage Delete(FormDataCollection form) {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Orders
                .Include(o=>o.OrderType)
                .Include(o=>o.Person1)
                .Include(o=>o.StudentExams)
                .FirstOrDefault(item => item.OrderID == key);
            
            if (model != null && model.Flags == 128)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    $"Невозможно удалить договор №{model.OrderNumber}, так как уже удален.");

            if (model != null && model.Flags != 0)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    $"Невозможно удалить договор №{model.OrderNumber}, так как произведена оплата.");

            switch (model.OrderTypeID)
            {
                case 1:
                {
                    if (model != null)
                    {
                        foreach (var studentExam in model.StudentExams)
                        {
                            _context.Entry(studentExam).State = EntityState.Modified;
                            studentExam.Flags += 128;
                        }
                    }

                    break;
                }
                case 2:
                {
                    if (model != null)
                    {
                        var personGroup = _context.PersonGroups.FirstOrDefault(p => p.OrderID == model.OrderID && p.PersonID==model.StudentPersonID);
                        if (personGroup == null)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Не найден ученик в группе");
                        }
                        _context.Entry(personGroup).State = EntityState.Modified;
                        var logPersonGroup = new HistoryLog()
                        {
                            ExecuterID = id,
                            ObjectName = nameof(personGroup),
                            ObjectID = personGroup.PersonGroupID,
                            FieldName = nameof(personGroup.Flags),
                            OldValue = personGroup.Flags.ToString(),
                            NewValue = (personGroup.Flags + 128).ToString(),
                            CreateDate = DateTime.Now
                        };
                        _context.HistoryLogs.Add(logPersonGroup);
                        personGroup.Flags += 128;
                        personGroup.DeleteDate = DateTime.Now;
                    }
                    break;
                }
            }

            _context.Entry(model).State = EntityState.Modified;
            
            var logOrder = new HistoryLog()
            {
                ExecuterID = id,
                ObjectName = nameof(model),
                ObjectID = model.OrderID,
                FieldName = nameof(model.Flags),
                OldValue = model.Flags.ToString(),
                NewValue = (model.Flags + 128).ToString(),
                CreateDate = DateTime.Now
            };
            model.Flags += 128;

            _context.HistoryLogs.Add(logOrder);
            //var user = _context.Users.FirstOrDefault(u => u.UserID == model.Person1.UserID);
            //var emailService = new EmailServiceSend();
            //if (user != null) await emailService.ChangeStatusOrder(user.Email, model);

            _context.SaveChanges();


            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [HttpGet]
        public HttpResponseMessage FilesLookup(DataSourceLoadOptions loadOptions) {
            var lookup = _context.Files.OrderBy(i => i.FileName).Select(i => new {Value = i.FileID, Text = i.FileName});
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> OrderTypesLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.OrderTypes;

            var result = await DataSourceLoader.LoadAsync(lookup, loadOptions);
            return Request.CreateResponse(result);
        }


        private void PopulateModel(Order model, IDictionary values) {
            string ORDER_ID = nameof(Order.OrderID);
            string ORDER_TYPE_ID = nameof(Order.OrderTypeID);
            string CREATOR_PERSON_ID = nameof(Order.CreatorPersonID);
            string STUDENT_PERSON_ID = nameof(Order.StudentPersonID);
            string FLAGS = nameof(Order.Flags);
            string CREATE_DATE = nameof(Order.CreateDate);
            string UPDATE_DATE = nameof(Order.UpdateDate);
            string RECEIPT_FILE_ID = nameof(Order.ReceiptFileID);
            string DISCOUNT = nameof(Order.Discount);
            string COMMENT = "Comment";

            if (values.Contains(COMMENT))
            {
                var personGroup = _context.PersonGroups.FirstOrDefault(pg => pg.OrderID == model.OrderID);
                if (personGroup != null)
                {
                    _context.Entry(personGroup).State = EntityState.Modified;
                    personGroup.Comment = Convert.ToString(values[COMMENT]);
                    _context.SaveChanges();
                }
            }

            if(values.Contains(ORDER_ID)) {
                model.OrderID = Convert.ToInt32(values[ORDER_ID]);
            }

            if(values.Contains(ORDER_TYPE_ID)) {
                model.OrderTypeID = Convert.ToInt32(values[ORDER_TYPE_ID]);
            }

            if(values.Contains(CREATOR_PERSON_ID)) {
                model.CreatorPersonID = Convert.ToInt32(values[CREATOR_PERSON_ID]);
            }

            if(values.Contains(STUDENT_PERSON_ID)) {
                model.StudentPersonID = Convert.ToInt32(values[STUDENT_PERSON_ID]);
            }

            if (values.Contains(FLAGS))
            {
                var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
                var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());
                var flags = Convert.ToInt32(values[FLAGS]);
                if (model.Flags < flags)
                {
                    switch (model.OrderTypeID)
                    {
                        case 1:
                        {
                            foreach (var modelStudentExam in model.StudentExams)
                            {
                                switch (flags)
                                {
                                    case 2:
                                    {
                                        modelStudentExam.Flags =
                                            modelStudentExam.Exam.ExamTypeID >= 2 && modelStudentExam.Exam.ExamTypeID <= 4
                                                ? 2
                                                : 1;
                                        break;
                                    }
                                    case 4:
                                    {
                                        modelStudentExam.Flags = 4;
                                        modelStudentExam.StationsExamsID = null;
                                        break;
                                    }
                                    case 128:
                                    {
                                        modelStudentExam.Flags = 128;
                                        modelStudentExam.StationsExamsID = null;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                        case 2:
                        {
                            var personGroup = _context.PersonGroups.FirstOrDefault(pg =>
                                pg.OrderID == model.OrderID && pg.PersonID == model.StudentPersonID);
                            switch (flags)
                            {
                                case 4:
                                {
                                    _context.Entry(personGroup).State = EntityState.Modified;
                                    var logPersonGroup = new HistoryLog()
                                    {
                                        ExecuterID = id,
                                        ObjectName = nameof(personGroup),
                                        ObjectID = personGroup.PersonGroupID,
                                        FieldName = nameof(personGroup.Flags),
                                        OldValue = personGroup.Flags.ToString(),
                                        NewValue = (personGroup.Flags + flags).ToString(),
                                        CreateDate = DateTime.Now
                                    };
                                    _context.HistoryLogs.Add(logPersonGroup);
                                    personGroup.Flags += flags;
                                    personGroup.EndEducationDateTime = DateTime.Now;
                                    break;
                                }
                                case 128:
                                {
                                    _context.Entry(personGroup).State = EntityState.Modified;
                                    var logPersonGroup = new HistoryLog()
                                    {
                                        ExecuterID = id,
                                        ObjectName = nameof(personGroup),
                                        ObjectID = personGroup.PersonGroupID,
                                        FieldName = nameof(personGroup.Flags),
                                        OldValue = personGroup.Flags.ToString(),
                                        NewValue = (personGroup.Flags + flags).ToString(),
                                        CreateDate = DateTime.Now
                                    };
                                    _context.HistoryLogs.Add(logPersonGroup);
                                    personGroup.Flags += flags;
                                    personGroup.DeleteDate = DateTime.Now;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    var logOrder = new HistoryLog()
                    {
                        ExecuterID = id,
                        ObjectName = nameof(model),
                        ObjectID = model.OrderID,
                        FieldName = nameof(model.Flags),
                        OldValue = model.Flags.ToString(),
                        NewValue = (model.Flags + flags).ToString(),
                        CreateDate = DateTime.Now
                    };

                    model.Flags = flags;
                }
                
                
            }

            if (values.Contains(CREATE_DATE)) {
                model.CreateDate = Convert.ToDateTime(values[CREATE_DATE]);
            }

            if(values.Contains(UPDATE_DATE)) {
                model.UpdateDate = values[UPDATE_DATE] != null ? Convert.ToDateTime(values[UPDATE_DATE]) : (DateTime?)null;
            }

            if(values.Contains(RECEIPT_FILE_ID)) {
                model.ReceiptFileID = values[RECEIPT_FILE_ID] != null ? Convert.ToInt32(values[RECEIPT_FILE_ID]) : (int?)null;
            }

            if (values.Contains(DISCOUNT))
            {
                model.Discount = Convert.ToInt32(values[DISCOUNT]);
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