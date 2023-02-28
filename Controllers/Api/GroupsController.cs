using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DevExpress.Data.Helpers;
using DevExpress.Web.ASPxScheduler.Rendering;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using WebDtt.Models;

namespace WebDtt.Controllers.Api
{
    [Route("api/Groups/{action}", Name = "Groups")]
    public class GroupsController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false}
        };

        [HttpGet]
        [Authorize(Roles = "manager")]
        public async Task<HttpResponseMessage> Get(DataSourceLoadOptions loadOptions)
        {
            var rsg = _context.Groups.Include(g=>g.PersonGroups.Select(pg=>pg.Person));

            var groups = _context.Groups.Select(g => new
            {
                g.GroupID,
                g.CurriculumID,
                g.EducationFormID,
                g.StationID,
                g.Flags,
                g.Capacity,
                g.Duration,
                g.StartDateTime,
                g.FinishDateTime,
                g.Price,
                g.GroupName,
                g.LessonDuration,
                g.PersonGroups,
                g.AuditoriumNumber,
                //g.DonateTypes,
                DonateTypes = _context.DonateTypes.Select(d => new
                {
                    d.DonateTypeID,
                    d.CountDonates,
                    d.FullTotal,
                    d.Flags,
                    DonateInfos = _context.DonateInfoes.Where(x => x.DonateTypeID == d.DonateTypeID),
                    ViewName = d.FullTotal + "(" + d.CountDonates + ")"
                }).Where(d => _context.Groups.FirstOrDefault(gr=>gr.GroupID==g.GroupID).DonateTypes.Contains(_context.DonateTypes.FirstOrDefault(dt=>dt.DonateTypeID==d.DonateTypeID))),
                TeachersOfGroup = g.PersonGroups.Select(pg=>pg.Person).Where(p => p.PersonTypeID == 2 && (p.Flags & 128) != 128),
                PersGroupIDs = _context.PersonGroups.Where(pg => pg.GroupID == g.GroupID).Select(pg => pg.PersonID),
                Teachers = _context.Persons.Select(p=>new
                {
                    p.PersonTypeID,
                    p.PersonID,
                    p.Flags,
                    p.LastName,
                    p.FirstName,
                    p.Patronymic,
                    ViewShortFio = p.LastName + " " +
                                   p.FirstName.Substring(0, 1) + " " +
                                   p.Patronymic.Substring(0, 1)
                }).Where(p => p.PersonTypeID == 2 &&
                              (p.Flags & 128) != 128 && g.PersonGroups.Contains(_context
                                  .PersonGroups
                                  .FirstOrDefault(x =>
                                      x.GroupID == g.GroupID && x.PersonID == p.PersonID &&
                                      (x.Flags & 128) != 128))),
                Students = _context.Persons.Where(p => p.PersonTypeID == 1 &&
                                                       (p.Flags & 128) != 128 && g.PersonGroups.Contains(_context
                                                           .PersonGroups
                                                           .FirstOrDefault(x =>
                                                               x.GroupID == g.GroupID && x.PersonID == p.PersonID &&
                                                               (x.Flags & 128) != 128)))
            });

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "GroupID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(await DataSourceLoader.LoadAsync(groups, loadOptions));
        }

        [HttpPost]
        [Authorize(Roles="manager")]
        public async Task<HttpResponseMessage> Post(FormDataCollection form) { 
            var model = new Group();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));
            if (model.StartDateTime.Date >= model.FinishDateTime.Date)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Дата начала обучения группы не может быть больше даты окончания обучения!");
            if ((model.FinishDateTime - model.StartDateTime).TotalHours < model.Duration)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Указанное количесвто часов больше, чем разница даты и времени конца и начала обучения!");

            var result = _context.Groups.Add(model);

            //проверяем наличие преподов в данных из формы
            if (values.Contains(nameof(Group.Teachers)))
            {
                var persGroupIDsFromForm = JsonConvert.DeserializeObject<List<Person>>(values[nameof(Group.Teachers)].ToString());
                //отсеиваем преподов, непрошедших валидации
                var goodTeachers = persGroupIDsFromForm.Where(i => ValidateTeacherBeforeToAddToGroup(model, i.PersonID)).ToList();
                if (!goodTeachers.Any())
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Выбранные преподаватели не могут проводить занятия в данной группе!");

                await _context.SaveChangesAsync();
                //добавляем новых преподов в группу
                foreach (var i in goodTeachers)
                {
                    _context.PersonGroups.Add(new PersonGroup()
                    {
                        GroupID = model.GroupID,
                        PersonID = i.PersonID,
                        CreateDate = DateTime.Now
                    });
                }
            }

            //_context.Entry(model).State = EntityState.Added;
            await _context.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.Created, result.GroupID);
        }

        

        [HttpPut]
        [Authorize(Roles = "manager")]
        public async Task<HttpResponseMessage> Put(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Groups.Include(g=>g.DonateTypes).FirstOrDefault(item => item.GroupID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Group not found");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            if (model.StartDateTime.Date >= model.FinishDateTime.Date)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Дата начала обучения группы не может быть больше даты окончания обучения!");

            if ((model.FinishDateTime-model.StartDateTime).TotalHours < model.Duration)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Указанное количесвто часов больше, чем разница даты и времени конца и начала обучения!");

            //проверяем наличие преподов в данных из формы
            if (values.Contains(nameof(Group.Teachers)))
            {
                //var personIDs = JsonConvert.SerializeObject(values[nameof(Group.PersonGroups)]);
                //var pGs = JsonConvert.DeserializeObject<List<PersonGroup>>(personIDs);
                var persGroupIDsFromForm = JsonConvert.DeserializeObject<List<Person>>(values[nameof(Group.Teachers)].ToString());
                //отсеиваем преподов, непрошедших валидации
                var goodTeachers = persGroupIDsFromForm.Where(i => ValidateTeacherBeforeToAddToGroup(model, i.PersonID)).ToList();
                if (!goodTeachers.Any())
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Выбранные преподаватели не могут проводить занятия в данной группе!");

                await _context.SaveChangesAsync();

                //дергаем преподов, которые уже привязаны к группе, удаляем их из PersonGroups
                var oldTeacher =
                    _context.PersonGroups.Where(x => x.Person.PersonTypeID == 2 && x.GroupID == model.GroupID);
                foreach (var personGroup in oldTeacher)
                {
                    _context.PersonGroups.Remove(personGroup);
                }

                //добавляем новых преподов в группу
                foreach (var i in goodTeachers)
                {
                    _context.PersonGroups.Add(new PersonGroup()
                    {
                        GroupID = model.GroupID,
                        PersonID = i.PersonID,
                        CreateDate = DateTime.Now
                    });
                }
            }
            var error = ValidateChangedGroup(model);
            if (error != "")
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);

            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [HttpDelete]
        [Authorize(Roles = "manager")]
        public async Task Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Groups
                .Include(g =>
                    g.PersonGroups.Where(pg =>
                        pg.Person.PersonTypeID == 1 & (pg.Flags & 128) != 128 & (pg.Flags & 4) != 4))
                .FirstOrDefault(item => item.GroupID == key);
            if (model != null && !model.PersonGroups.Any()) model.Flags = 128;
            await _context.SaveChangesAsync();
        }


        [HttpGet]
        public async Task<HttpResponseMessage> DonateTypesLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.DonateTypes.Select(d => new
            {
                d.DonateTypeID,
                d.CountDonates,
                d.FullTotal,
                d.Flags,
                DonateInfos = _context.DonateInfoes.Where(x => x.DonateTypeID == d.DonateTypeID),
                ViewName = d.FullTotal + "("+d.CountDonates+")"
            });
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }


        [HttpGet]
        public async Task<HttpResponseMessage> CurriculumLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Curricula.Select(c=>new
            {
                c.CurriculumID,
                c.CourseName,
                c.Class,
                c.SubjectID,
                Subject = _context.Subjects.FirstOrDefault(s=>s.SubjectID==c.SubjectID).SubjectName
            });
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> EducationFormsLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.EducationForms
                         orderby i.EducationFormName
                         select new {
                             Value = i.EducationFormID,
                             Text = i.EducationFormName
                         };
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> StationsLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Stations.Select(x=>new
            {
                x.StationName,
                x.StationCode,
                x.StationAddress,
                Value = x.StationID,
                Text = x.StationAddress
            });
            return Request.CreateResponse(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage GetGroupsForScheduler(DataSourceLoadOptions loadOptions)
        {
            var groups = _context.Groups.Select(g=>new
            {
                g.GroupID,
                g.CurriculumID,
                g.EducationFormID,
                EducationForm = _context.EducationForms.FirstOrDefault(e=>e.EducationFormID==g.EducationFormID).EducationFormName,
                Subject = _context.Subjects.FirstOrDefault(s => s.SubjectID == _context.Curricula.FirstOrDefault(c => c.CurriculumID == g.CurriculumID).SubjectID).SubjectName,
                g.StartDateTime,
                //Day = g.StartDateTime.DayOfWeek,
                EducationClass = _context.Curricula.FirstOrDefault(c=>c.CurriculumID==g.CurriculumID).Class,
                g.Duration,
                g.Price,
                g.GroupName,
                text = g.GroupName,
                startDate = g.StartDateTime,
                endDate = g.StartDateTime
            }).ToList();

            return Request.CreateResponse(DataSourceLoader.Load(groups, loadOptions));
        }

        private void PopulateModel(Group model, IDictionary values) {
            string GROUP_ID = nameof(Group.GroupID);
            string GROUP_NAME = nameof(Group.GroupName);
            string CAPACITY = nameof(Group.Capacity);
            string START_DATE_TIME = nameof(Group.StartDateTime);
            string FINISH_DATE_TIME = nameof(Group.FinishDateTime);
            string DURATION = nameof(Group.Duration);
            string PRICE = nameof(Group.Price);
            string FLAGS = nameof(Group.Flags);
            string STATION_ID = nameof(Group.StationID);
            string EDUCATION_FORM_ID = nameof(Group.EducationFormID);
            string CURRICULUM_ID = nameof(Group.CurriculumID);
            string DONATE_TYPES = nameof(Group.DonateTypes);
            string LESSON_DURATION = nameof(Group.LessonDuration);
            string AUDITORIUM_NUMBER = nameof(Group.AuditoriumNumber);

            if(values.Contains(GROUP_ID)) {
                model.GroupID = Convert.ToInt32(values[GROUP_ID]);
            }

            if(values.Contains(GROUP_NAME)) {
                model.GroupName = Convert.ToString(values[GROUP_NAME]);
            }

            if(values.Contains(CAPACITY)) {
                model.Capacity = Convert.ToInt32(values[CAPACITY]);
            }

            if(values.Contains(START_DATE_TIME)) {
                model.StartDateTime = Convert.ToDateTime(values[START_DATE_TIME]);
            }

            if(values.Contains(FINISH_DATE_TIME)) {
                model.FinishDateTime = Convert.ToDateTime(values[FINISH_DATE_TIME]);
            }

            if(values.Contains(DURATION)) {
                model.Duration = Convert.ToInt32(values[DURATION]);
            }

            if(values.Contains(PRICE)) {
                model.Price = Convert.ToInt32(values[PRICE]);
            }

            if(values.Contains(FLAGS)) {
                model.Flags = Convert.ToInt32(values[FLAGS]);
            }

            if(values.Contains(STATION_ID)) {
                model.StationID = Convert.ToInt32(values[STATION_ID]);
            }

            if(values.Contains(EDUCATION_FORM_ID)) {
                model.EducationFormID = Convert.ToInt32(values[EDUCATION_FORM_ID]);
            }

            if(values.Contains(CURRICULUM_ID)) {
                model.CurriculumID = Convert.ToInt32(values[CURRICULUM_ID]);
            }

            if (values.Contains(DONATE_TYPES))
            {
                var donateTypes = JsonConvert.DeserializeObject<List<DonateType>>(values[DONATE_TYPES].ToString());
                var verifDonTypes = new List<DonateType>();
                foreach (var type in donateTypes)
                {
                    var donateType = _context.DonateTypes.FirstOrDefault(x => x.DonateTypeID == type.DonateTypeID);
                    if(donateType != null && model.Price==donateType.FullTotal) verifDonTypes.Add(donateType);
                }

                if (verifDonTypes.Any())
                {
                    model.DonateTypes.Clear();
                    foreach (var verifDonType in verifDonTypes)
                    {
                        model.DonateTypes.Add(verifDonType);
                    }
                }

            }

            if (values.Contains(LESSON_DURATION))
            {
                model.LessonDuration = Convert.ToByte(values[LESSON_DURATION]);
            }

            if (values.Contains(AUDITORIUM_NUMBER))
            {
                model.AuditoriumNumber = Convert.ToInt32(values[AUDITORIUM_NUMBER]);
            }

        }

        private string ValidateChangedGroup(Group model)
        {
            var message = "";
            //if (model.StartDateTime.Date >= model.FinishDateTime.Date)
            //    message += "Дата начала обучения группы не может быть больше даты окончания обучения!\r\n";

            //if ((model.FinishDateTime - model.StartDateTime).TotalHours < model.Duration)
            //    message += "Указанное количесвто часов больше, чем разница даты и времени конца и начала обучения!;\r\n";

            var teachers = _context.Persons.Include(p => p.Subjects)
                .Where(p => _context.PersonGroups.Where(pg => pg.GroupID == model.GroupID).Select(pg => pg.Person)
                    .Contains(p) && p.PersonTypeID == 2);


            var curricula = _context.Curricula.FirstOrDefault(c => c.CurriculumID == model.CurriculumID);
            var currSubject = _context.Subjects.FirstOrDefault(cs => cs.SubjectID == curricula.SubjectID);

            foreach (var teacher in teachers)
            {
                if (teacher.ParticipantClass != null && teacher.ParticipantClass != curricula.Class)
                    message += $"Преподаватель {teacher.ViewShortFio} не может проводить занятия для {curricula.Class}-го класса!\r\n";

                if (!teacher.Subjects.Contains(currSubject))
                    message += $"Преподаватель {teacher.ViewShortFio} не может проводить занятия по предмету {currSubject.SubjectName}!\r\n";
            }

            return message;
        }
        private bool ValidateTeacherBeforeToAddToGroup(Group model, int teacherId)
        {
            var curricula = _context.Curricula.FirstOrDefault(c => c.CurriculumID == model.CurriculumID);
            if (curricula == null) return false;
            var curriculaSubject = _context.Subjects.FirstOrDefault(s =>
                s.SubjectID == _context.Curricula.FirstOrDefault(c => c.CurriculumID == model.CurriculumID).SubjectID);
            var teacherPerson = _context.Persons.Include(p => p.Subjects)
                .FirstOrDefault(p => p.PersonID == teacherId && p.PersonTypeID == 2 && (p.Flags & 128) != 128);
            if (teacherPerson == null) return false;
            if (teacherPerson.ParticipantClass != null & curricula.Class != teacherPerson.ParticipantClass)
                return false;
            return teacherPerson.Subjects.Contains(curriculaSubject);
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