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
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DevExpress.Utils.Extensions;
using DevExpress.XtraGauges.Core.Model;
using DevExpress.XtraReports.Wizards;
using WebDtt.Models.Dto;
using System.Security.Cryptography;
using System.Text;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using WebDtt.Models.Extentions;

namespace WebDtt.Models.Controllers
{
    [Authorize]
    [Route("api/Persons/{action}", Name = "PersonsApi")]
    public class PersonsController : ApiController
    {
        
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            List<Person> persons;
            List<StudentViewModel> students = new List<StudentViewModel>();

            var user = HttpContext.Current.GetOwinContext().Authentication.User;
            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());


            if (user.IsInRole("user"))
            {
                //persons = _context.Persons
                //    .Where(p => p.UserID == id && (p.Flags&128)!=128)
                //    .Include(p=>p.Orders)
                //    .Include(p=>p.PersonType)
                //    .Include(p=>p.Person1.PersonType)
                //    .Include(p=>p.User)
                //    .Where(p => p.PersonTypeID == 1)
                //    .OrderBy(p => p.UserID)
                //    .ThenBy(p=>p.PersonTypeID)
                //    .ToList();
                persons = _context.Persons
                    .Include(p => p.PersonType)
                    .Include(p => p.StudentExams
                        .Select(se => se.Exam))
                    .Include(p=>p.PersonGroups
                        .Select(pg=>pg.Group))
                    .Where(p => p.UserID == id && (p.Flags & 128) != 128)
                    .ToList();

                //var stex = _context.StudentExams.Include(se => se.Exam).ToList();

                //return Request.CreateResponse(DataSourceLoader.Load(students, loadOptions));

                //students.AddRange(persons.Select(person => new StudentViewModel(person)));
                return Request.CreateResponse(DataSourceLoader.Load(persons, loadOptions));
            }


            persons = _context.Persons
                .Include(p => p.PersonType)
                .Include(p => p.Person1.PersonType)
                .Include(p => p.User)
                .Where(p=>p.PersonTypeID==1)
                .ToList();

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "PersonID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            students.AddRange(persons.Select(person => new StudentViewModel(person)));
            return Request.CreateResponse(DataSourceLoader.Load(students, loadOptions));
        }

        [HttpGet]
        [Authorize(Roles = "operator,manager")]
        public HttpResponseMessage GetDelegates(DataSourceLoadOptions loadOptions)
        {
            var persons = _context.Persons
                .Include(p => p.Orders)
                .Include(p => p.PersonType)
                .Include(p => p.Persons1)
                .Where(p => p.PersonTypeID == 3)
                .ToList();

            return Request.CreateResponse(DataSourceLoader.Load(persons, loadOptions));
        }

        [HttpGet]
        [Authorize(Roles = "operator,manager")]
        public HttpResponseMessage GetTeachers(DataSourceLoadOptions loadOptions)
        {
            var persons = _context.Persons.Select(p=>new
                {
                    p.PersonID,
                    p.PersonTypeID,
                    p.Subjects,
                    p.PersonType,
                    p.LastName,
                    p.FirstName,
                    p.Patronymic,
                    p.BirthDate,
                    p.ParticipantClass,
                    p.DocSeria,
                    p.DocNumber,
                    p.IssuedBy,
                    p.IssedDate,
                    p.Email,
                    p.Phones,
                    p.Flags,
                    ViewShortFio = p.LastName +" "+ 
                                   p.FirstName.Substring(0,1) + " " +
                                   p.Patronymic.Substring(0,1)
                }).Where(p => p.PersonTypeID == 2 | p.PersonTypeID == 5);

            return Request.CreateResponse(DataSourceLoader.Load(persons, loadOptions));
        }

        [HttpGet]
        [Authorize(Roles = "operator,manager,expert")]
        public HttpResponseMessage GetExperts(DataSourceLoadOptions loadOptions)
        {
            var persons = _context.Persons
                .Include(p=>p.Subjects)
                .Where(p => p.PersonTypeID == 4 | p.PersonTypeID == 5)
                .ToList();
            return Request.CreateResponse(DataSourceLoader.Load(persons, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetPersonById(int personId, DataSourceLoadOptions loadOptions)
        {
            var person = _context.Persons
                .Include(p=>p.PersonType)
                .Where(p=>p.PersonID==personId);
            var result = await DataSourceLoader.LoadAsync(person, loadOptions);
            return Request.CreateResponse(result);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetPerson(DataSourceLoadOptions loadOptions)
        {
            var person = _context.Persons.Include(p => p.PersonType);
            var result = await DataSourceLoader.LoadAsync(person, loadOptions);
            return Request.CreateResponse(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<HttpResponseMessage> Post(FormDataCollection form)
        {
            var model = new Person();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"), 
                new JsonSerializerSettings (){ DateTimeZoneHandling = DateTimeZoneHandling.Local});

            model.PersonTypeID = values.Contains("ParticipantClass") ? 1 : 3;

            PopulateModel(model, values);

            if (model.PersonTypeID == 1 || model.PersonTypeID == 3)
            {
                var person = _context.Persons.Include(p => p.PersonType).FirstOrDefault(
                p => p.DocNumber == model.DocNumber &&
                     p.DocSeria == model.DocSeria &&
                     p.PersonTypeID == model.PersonTypeID);
                if (person != null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        $"{person.PersonType.PersonTypeName} с такими паспортными данными уже существует. Выберите его из списка.");
                var user = HttpContext.Current.GetOwinContext().Authentication.User;
                if (user.IsInRole("user"))
                {
                    var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());
                    model.UserID = id;
                }
            }

            var rExpert = _context.Roles.FirstOrDefault(r => r.RoleID == 5);
            var roleTeacher = _context.Roles.FirstOrDefault(r => r.RoleID == 4);
            EmailServiceSend emailService = new EmailServiceSend();
            switch (model.PersonTypeID)
            {
                case 1:
                    var st = new StudentViewModel(model);
                    Validate(st);
                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));
                    break;
                case 3:
                    var del = new DelegateViewModel(model);
                    Validate(del);
                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));
                    break;
                case 4:
                    var expert = new ExpertViewModel(model);
                    Validate(expert);
                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));
                                      
                    var newUserExpert = _context.Users.Include(u => u.Roles).FirstOrDefault(u => u.Email == model.Email);
                    if (newUserExpert != null)
                    {
                        if (!newUserExpert.Roles.Contains(roleTeacher) | newUserExpert.Roles.Contains(rExpert))
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                $"Пользователь с Email = {model.Email} уже существует!");

                        newUserExpert.Roles.Add(rExpert);
                        model.UserID = newUserExpert.UserID;
                        break;
                    }

                    newUserExpert = new User() 
                    {
                        UserID = Guid.NewGuid(),
                        Email = model.Email,
                        EmailConfirmed = true,
                        PasswordHash = Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes($"{model.Email.Substring(0,3)}@Expert"))),
                        Roles = new List<Role>()
                    };
                    newUserExpert.Roles.Add(rExpert);
                    _context.Users.Add(newUserExpert);
                    _context.Entry(newUserExpert).State = EntityState.Added;
                    _context.SaveChanges();

                    //отправить письмо на почту эксперту с логином и паролем
                    var callbackUrl = Url.Link("Default",
                        new { Controller = "Home", Action = "Index" });
                    //EmailServiceSend emailService = new EmailServiceSend();
                    await emailService.CreateExpert(model, callbackUrl);

                    model.UserID = newUserExpert.UserID;
                    break;
                case 2:
                    var teacher = new TeacherViewModel(model);
                    Validate(teacher);
                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));
                    var newUserTeacher = _context.Users.Include(u => u.Roles).FirstOrDefault(u => u.Email == model.Email);
                    if(newUserTeacher != null)
                    {
                        if (!newUserTeacher.Roles.Contains(rExpert) | newUserTeacher.Roles.Contains(roleTeacher))                        
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, $"Пользователь с Email = {model.Email} уже существует!");

                        newUserTeacher.Roles.Add(roleTeacher);
                        model.UserID = newUserTeacher.UserID;
                        break;
                    }

                    newUserTeacher = new User()
                    {
                        UserID = Guid.NewGuid(),
                        Email = model.Email,
                        EmailConfirmed = true,
                        PasswordHash = Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes($"{model.Email.Substring(0, 3)}@Teacher"))),
                        Roles = new List<Role>()
                    };
                    newUserTeacher.Roles.Add(roleTeacher);
                    _context.Users.Add(newUserTeacher);
                    _context.Entry(newUserTeacher).State = EntityState.Added;
                    _context.SaveChanges();

                    //отправить письмо на почту эксперту с логином и паролем
                    var cbUrl = Url.Link("Default",
                        new { Controller = "Home", Action = "Index" });
                    
                    await emailService.CreateTeacher(model, cbUrl);

                    model.UserID = newUserTeacher.UserID;

                    break;
            }

            model.CreateDate=DateTime.Now;
            var result = _context.Persons.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.PersonID);
        }

        [HttpPut]
        [Authorize(Roles = "operator,manager")]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Persons.Include(p=>p.Subjects).FirstOrDefault(item => item.PersonID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Person not found");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"), 
                new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });

            PopulateModel(model, values);

            if (values.Contains(nameof(Person.LastName)) || 
                values.Contains(nameof(Person.FirstName)) ||
                values.Contains(nameof(Person.Patronymic)) ||
                values.Contains(nameof(Person.PersonTypeID)))
            {
                var person = _context.Persons.Include(p => p.PersonType).FirstOrDefault(
                    p => p.DocNumber == model.DocNumber &&
                         p.DocSeria == model.DocSeria &&
                         p.PersonTypeID == model.PersonTypeID &&
                         p.PersonID != model.PersonID);
                if (person != null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        $"{person.PersonType.PersonTypeName} с такими паспортными данными уже существует!");
            }


            

            switch (model.PersonTypeID)
            {
                case 1:
                    var st = new StudentViewModel(model);
                    Validate(st);
                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));
                    break;
                case 3:
                    var del = new DelegateViewModel(model);
                    Validate(del);
                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));
                    break;
                case 4:
                    var expert = new ExpertViewModel(model);
                    Validate(expert);
                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));
                    break;
            }

            model.UpdateDate = DateTime.Now;

            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Authorize(Roles = "manager")]
        public HttpResponseMessage Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Persons.FirstOrDefault(item => item.PersonID == key);
            
            IDictionary values = new Dictionary<object, object>();
            values.Add("Flags", 128);
            
            PopulateModel(model,values);

            _context.SaveChanges();
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Authorize]
        public HttpResponseMessage PersonTypesLookup(DataSourceLoadOptions loadOptions) {
            List<PersonType> lookup;

            var user = HttpContext.Current.GetOwinContext().Authentication.User;
            //var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            if (user.IsInRole("user"))
            {
                lookup = _context.PersonTypes.Where(pt=>pt.PersonTypeID==1 | pt.PersonTypeID==3).ToList();
                return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
            }

            lookup = _context.PersonTypes.ToList();
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        [Authorize(Roles = "operator,manager")]
        public HttpResponseMessage GetUsersLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Users
                .Include(u=>u.Roles)
                .Where(u=>u.Roles.Any(r=>r.RoleID==3))
                .ToList();
            
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }



        [Authorize]
        private void PopulateModel(Person model, IDictionary values) {
            string PERSON_ID = nameof(Person.PersonID);
            string PERSON_GUID = nameof(Person.PersonGUID);
            string PERSON_TYPE_ID = nameof(Person.PersonTypeID);
            string LAST_NAME = nameof(Person.LastName);
            string FIRST_NAME = nameof(Person.FirstName);
            string PATRONYMIC = nameof(Person.Patronymic);
            string DOC_SERIA = nameof(Person.DocSeria);
            string DOC_NUMBER = nameof(Person.DocNumber);
            string ISSUED_BY = nameof(Person.IssuedBy);
            string ISSED_DATE = nameof(Person.IssedDate);
            string BIRTH_DATE = nameof(Person.BirthDate);
            string SEX = nameof(Person.Sex);
            string PHONES = nameof(Person.Phones);
            string EMAIL = nameof(Person.Email);
            string REGISTRATION_ADDRESS = nameof(Person.RegistrationAddress);
            string PARTICIPANT_CLASS = nameof(Person.ParticipantClass);
            string USER_ID = nameof(Person.UserID);
            string PARENT_PERSON_ID = nameof(Person.ParentPersonID);
            string FLAGS = nameof(Person.Flags);
            string CREATE_DATE = nameof(Person.CreateDate);
            string UPDATE_DATE = nameof(Person.UpdateDate);
            string NICK_NAME = nameof(Person.NickName);
            string CONFIRMED_DATA = nameof(StudentViewModel.ConfirmedData);
            string CONFIRMED_POLICY = nameof(StudentViewModel.ConfirmedPolicy);
            string IS_DELETED = nameof(StudentViewModel.IsDeleted);
            string SUBJECTS = nameof(Person.Subjects);

            if (values.Contains(PERSON_ID)) {
                model.PersonID = Convert.ToInt32(values[PERSON_ID]);
            }

            if(values.Contains(PERSON_GUID)) {
                model.PersonGUID = values[PERSON_GUID] != null ? new Guid(Convert.ToString(values[PERSON_GUID])) : (Guid?)null;
            }

            if(values.Contains(PERSON_TYPE_ID)) {
                model.PersonTypeID = Convert.ToInt32(values[PERSON_TYPE_ID]);
            }

            if(values.Contains(LAST_NAME)) {
                model.LastName = Convert.ToString(values[LAST_NAME]).Replace(" ", "");
            }

            if(values.Contains(FIRST_NAME)) {
                model.FirstName = Convert.ToString(values[FIRST_NAME]).Replace(" ", "");
            }

            if(values.Contains(PATRONYMIC))
            {
                var str = Convert.ToString(values[PATRONYMIC]).Replace(" ", "");
                model.Patronymic = str.Length > 0 ? str : null;
            }

            if(values.Contains(DOC_SERIA)) {
                model.DocSeria = Convert.ToString(values[DOC_SERIA]);
            }

            if(values.Contains(DOC_NUMBER)) {
                model.DocNumber = Convert.ToString(values[DOC_NUMBER]);
            }

            if(values.Contains(ISSUED_BY)) {
                model.IssuedBy = Convert.ToString(values[ISSUED_BY]);
            }

            if(values.Contains(ISSED_DATE)) {
                model.IssedDate = values[ISSED_DATE] != null ? Convert.ToDateTime(values[ISSED_DATE]) : (DateTime?)null;
            }

            if(values.Contains(BIRTH_DATE)) {
                model.BirthDate = values[BIRTH_DATE] != null ? Convert.ToDateTime(values[BIRTH_DATE]) : (DateTime?)null;
            }

            if(values.Contains(SEX)) {
                model.Sex = values[SEX] != null ? Convert.ToByte(values[SEX]) : (byte?)null;
            }

            if(values.Contains(PHONES)) {
                model.Phones = Convert.ToString(values[PHONES]);
            }

            if(values.Contains(EMAIL)) {
                model.Email = Convert.ToString(values[EMAIL]).Replace(" ", "");
            }

            if(values.Contains(REGISTRATION_ADDRESS)) {
                model.RegistrationAddress = Convert.ToString(values[REGISTRATION_ADDRESS]);
            }

            if(values.Contains(PARTICIPANT_CLASS)) {
                model.ParticipantClass = values[PARTICIPANT_CLASS] != null ? Convert.ToByte(values[PARTICIPANT_CLASS]) : (byte?)null;
            }

            if(values.Contains(USER_ID)) {
                model.UserID = values[USER_ID] != null ? new Guid(Convert.ToString(values[USER_ID])) : (Guid?)null;
            }

            if(values.Contains(PARENT_PERSON_ID)) {
                model.ParentPersonID = values[PARENT_PERSON_ID] != null ? Convert.ToInt32(values[PARENT_PERSON_ID]) : (int?)null;
            }

            if(values.Contains(FLAGS)) {
                model.Flags = model.Flags | Convert.ToInt32(values[FLAGS]);
            }

            if (values.Contains(CONFIRMED_DATA))
            {
                model.Flags = Convert.ToBoolean(values[CONFIRMED_DATA]) ? model.Flags | 1 : model.Flags ^ 1;
            }

            if (values.Contains(CONFIRMED_POLICY))
            {
                model.Flags = Convert.ToBoolean(values[CONFIRMED_POLICY]) ? model.Flags | 2 : model.Flags ^ 2;
            }

            if (values.Contains(IS_DELETED))
            {
                model.Flags = Convert.ToBoolean(values[IS_DELETED]) ? model.Flags | 128 : model.Flags ^ 128;
            }

            if (values.Contains(CREATE_DATE)) {
                model.CreateDate = Convert.ToDateTime(values[CREATE_DATE]);
            }

            if(values.Contains(UPDATE_DATE)) {
                model.UpdateDate = values[UPDATE_DATE] != null ? Convert.ToDateTime(values[UPDATE_DATE]) : (DateTime?)null;
            }

            if (values.Contains(NICK_NAME))
            {
                model.RegistrationAddress = Convert.ToString(values[NICK_NAME]);
            }

            if (values.Contains(SUBJECTS))
            {
                model.Subjects.Clear();
                //model.PersonTypeID = 4;
                var subs = JsonConvert.DeserializeObject<List<Subject>>(values[SUBJECTS].ToString());
                foreach (var i in subs)
                {
                    var subject = _context.Subjects.FirstOrDefault(s => s.SubjectID == i.SubjectID);
                    model.Subjects.Add(subject);
                }

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