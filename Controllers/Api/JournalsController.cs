using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DevExpress.DirectX.Common.DirectWrite;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using WebDtt.Models;

namespace WebDtt.Controllers.Api
{
    [Authorize]
    public class JournalsController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = {LazyLoadingEnabled = false}
        };

        [HttpGet]
        public async Task<HttpResponseMessage> Groups()
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ??
                                throw new InvalidOperationException());
            

            if (user.IsInRole("manager"))
            {
                var data = await _context.Groups.Where(g => g.PersonGroups
                        .Any(pg => (pg.Flags & 132) == 0) && (g.Flags&128)!=128)
                    .Select(g1 => new {g1.GroupID, g1.GroupName}).ToListAsync();
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }

            if (user.IsInRole("teacher"))
            {
                var person = await _context.Persons.FirstOrDefaultAsync(p => p.UserID == id && p.PersonTypeID == 2);

                var data = await _context.Groups
                    .Where(g => g.PersonGroups
                        .Any(pg => pg.PersonID == person.PersonID && (pg.Flags & 132) == 0) && (g.Flags & 128) != 128)
                    .Select(g1 => new {g1.GroupID, g1.GroupName}).ToListAsync();
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }

            if (user.IsInRole("user"))
            {
                return null;
            }

            return null;

        }

        [HttpGet]
        public async Task<HttpResponseMessage> Lessons(DataSourceLoadOptions loadOptions,int groupId)
        {
            var data = _context.Lessons.Where(l => l.GroupID == groupId)
                .Select(l1 => new
                {
                    l1.LessonID,
                    l1.LessonDate,
                    l1.Flags,
                    l1.Duration
                });
            return Request.CreateResponse(HttpStatusCode.OK, await DataSourceLoader.LoadAsync(data, loadOptions));
        }

        [HttpGet]
        [Authorize(Roles = "user")]
        public async Task<HttpResponseMessage> GetLessonsForUser()
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ??
                                throw new InvalidOperationException());

            var sql = await _context.Lessons
                .Where(l => (l.Flags & 128) == 0)
                .Join(_context.Groups,
                    l => l.GroupID,
                    g => g.GroupID,
                    (l, g) => new
                    {
                        l.GroupID,
                        l.LessonID,
                        l.Flags,
                        l.LessonDate,
                        g.CurriculumID,
                        g.GroupName,
                        g.Curriculum.Subject.SubjectName,
                        g.Station.StationAddress
                    }).Join(_context.PersonGroups
                        .Where(pg => pg.OrderID != null
                                     && ((pg.Order.Flags & 32) != 32 || (pg.Flags & 32) != 32)
                                     && pg.Person.UserID == id),
                    lg => lg.GroupID,
                    pg => pg.GroupID,
                    (lg, pg) => new
                    {
                        KeyID = lg.LessonID + " " + pg.PersonGroupID,
                        lg.GroupName,
                        lg.LessonID,
                        lg.SubjectName,
                        FIO = pg.Person.LastName + " " + pg.Person.FirstName + " " + pg.Person.Patronymic,
                        LessonFlags = lg.Flags,
                        lg.LessonDate,
                        pg.CreateDate,
                        pg.EndEducationDateTime,
                        pg.DeleteDate,
                        pg.PersonGroupID,
                        lg.GroupID,
                        lg.StationAddress,
                        PersonGroupFlags = pg.Flags
                    })
                .Where(x =>
                    x.CreateDate <= x.LessonDate && (
                        (x.PersonGroupFlags & 132) == 0 ||
                        ((x.PersonGroupFlags & 4) == 4 && x.EndEducationDateTime > x.LessonDate) ||
                        ((x.PersonGroupFlags & 128) == 128 && x.DeleteDate > x.LessonDate)
                    ))
                .OrderByDescending(x=>x.LessonDate).ToListAsync();

            return Request.CreateResponse(HttpStatusCode.OK, sql);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> InsertLesson(FormDataCollection form, int groupId)
        {
            var model = new Lesson()
            {
                GroupID = groupId,
                CreateDate = DateTime.Now
            };
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() {DateTimeZoneHandling = DateTimeZoneHandling.Local});

            PopulateLessonModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var message = ValidateLesson(model);
            if (message != "")
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);

            _context.Lessons.Add(model);

            var journalField = new JournalField()
            {
                FieldName = "Явка",
                FieldDescription = "Присутствие слушателя на занятии",
                MaxValue = 1,
                Flags = 0,
                CreateDate = DateTime.Now,
                LessonID = model.LessonID
            };

            _context.JournalFields.Add(journalField);

            await _context.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpPut]
        public async Task<HttpResponseMessage> UpdateLesson(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var model = await _context.Lessons.FirstOrDefaultAsync(item => item.LessonID == key);
            if (model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Занятие не найдено");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() {DateTimeZoneHandling = DateTimeZoneHandling.Local});
            PopulateLessonModel(model, values);
            model.UpdateDate = DateTime.Now;
            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var message = ValidateLesson(model);
            if (message != "")
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);

            await _context.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [HttpGet]
        public async Task<HttpResponseMessage> Topics(int groupId, int lessonId)
        {
            var curriculumId = _context.Groups.FirstOrDefault(g => g.GroupID == groupId).CurriculumID;
            if (_context.Groups == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Не найдена ни одна группа по переданному ключу!");

            var topicIdsFromLessons = _context.Topics.Where(t =>
                    t.Lessons.Contains(_context.Lessons.FirstOrDefault(l => l.LessonID == lessonId)))
                .Select(t => t.TopicID);

            object data;

            if(!User.IsInRole("user"))
            {
                data = await _context.Topics
                    .Select(x => new
                    {
                        x.TopicID,
                        x.CurriculumID,
                        x.TopicName,
                        x.Description,
                        x.Flags,
                        InLesson = topicIdsFromLessons.Contains(x.TopicID)
                    })
                    .Where(t => t.CurriculumID == curriculumId)
                    .ToListAsync();
            }
            else
            {
                data = await _context.Topics
                    .Select(x => new
                    {
                        x.TopicID,
                        x.CurriculumID,
                        x.TopicName,
                        x.Description,
                        x.Flags,
                        InLesson = topicIdsFromLessons.Contains(x.TopicID)
                    })
                    .Where(t => t.CurriculumID == curriculumId && t.InLesson)
                    .ToListAsync();
            }

            return Request.CreateResponse(HttpStatusCode.OK, data);

        }

        [HttpPost]
        public async Task<HttpResponseMessage> InsertTopic(FormDataCollection form, int lessonId)
        {
            var group = _context.Groups.FirstOrDefault(g =>
                g.GroupID == _context.Lessons.FirstOrDefault(l => l.LessonID == lessonId).GroupID);

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() {DateTimeZoneHandling = DateTimeZoneHandling.Local});

            var model = new Topic();
            if (group != null) model.CurriculumID = group.CurriculumID;

            PopulateTopicModel(model, values, lessonId);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            _context.Topics.Add(model);

            await _context.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpPut]
        public async Task<HttpResponseMessage> UpdateTopic(FormDataCollection form, int lessonId)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var model = await _context.Topics.Include(t => t.Lessons).FirstOrDefaultAsync(item => item.TopicID == key);
            if (model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Тема не найдена");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() {DateTimeZoneHandling = DateTimeZoneHandling.Local});
            PopulateTopicModel(model, values, lessonId);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            await _context.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [HttpGet]
        public async Task<HttpResponseMessage> LessonMaterials(int groupId, int lessonId, int? personGroupId)
        {
            object data;

            if (personGroupId!=null)
            {
                data = await _context.LessonMaterials
                    .Select(l => new
                    {
                        l.LessonMaterialID,
                        l.LessonID,
                        l.PersonGroupID,
                        l.FileID,
                        l.Flags,
                        l.CreateDate,
                        FileName = _context.Files.FirstOrDefault(f => f.FileID == l.FileID).FileName
                    })
                    .Where(l => l.LessonID == lessonId && l.PersonGroupID == personGroupId && (l.Flags&128)!=128).ToListAsync();
            }
            else
            {
                var techersIds = _context.PersonGroups
                    .Where(pg => pg.GroupID == groupId && pg.Person.PersonTypeID == 2)
                    .Select(pg => pg.PersonGroupID);

                data = await _context.LessonMaterials
                    .Select(l => new
                    {
                        l.LessonMaterialID,
                        l.LessonID,
                        l.PersonGroupID,
                        l.FileID,
                        l.Flags,
                        l.CreateDate,
                        FileName = _context.Files.FirstOrDefault(f => f.FileID == l.FileID).FileName
                    })
                    .Where(l => l.LessonID == lessonId && techersIds.Contains(l.PersonGroupID) && (l.Flags & 128) != 128).ToListAsync();
            }

            return Request.CreateResponse(HttpStatusCode.OK, data);
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteLessonMaterial(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var model = await _context.LessonMaterials.FirstOrDefaultAsync(l=>l.LessonMaterialID == key);
            if(model==null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                $"Материал занятия не найден!");

            if((model.Flags&128)==128) return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                $"Материал занятия уже удален!");

            var userIdFromModel = _context.PersonGroups.Include(pg=>pg.Person).FirstOrDefault(pg => pg.PersonGroupID == model.PersonGroupID)
                .Person.UserID;

            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            if (userid != userIdFromModel && user.IsInRole("user"))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    $"Невозможно удалить файл, так как он не принадлежит пользователю");

            var file = _context.Files.FirstOrDefault(f => f.FileID == model.FileID);

            if (file == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                $"Запись о файле в базе не найдена!");

            model.Flags |= 128;
            model.DeleteDate = DateTime.Now;
            
            var fInfo = new FileInfo(file.FullPath);
            var d = new DirectoryInfo(Path.Combine(file.FullPath.Replace(file.FileName, "Deleted")));
            if (!d.Exists) d.Create();
            var delFile = Path.Combine(d.FullName, file.FileName);

            if (new FileInfo(delFile).Exists)
            {
                fInfo.Delete();
                _context.Files.Remove(file);
                _context.LessonMaterials.Remove(model);
            }
            else
            {
                fInfo.MoveTo(delFile);
                file.FullPath = delFile;
            }

            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [HttpGet]
        public async Task<HttpResponseMessage> LessonAddons(int lessonId)
        {
            var data = await _context.LessonAddons.Select(l => new
            {
                l.LessonAddonID,
                l.LessonID,
                l.AddonShortName,
                l.AddonDescription,
                l.LinkAddress
            }).Where(l => l.LessonID == lessonId).ToListAsync();
            return Request.CreateResponse(HttpStatusCode.OK, data);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> InsertLessonAddon(FormDataCollection form, int lessonId)
        {
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() {DateTimeZoneHandling = DateTimeZoneHandling.Local});

            var model = new LessonAddon()
            {
                LessonID = lessonId
            };

            PopulateLessonAddonModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            _context.LessonAddons.Add(model);
            await _context.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpPut]
        public async Task<HttpResponseMessage> UpdateLessonAddon(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var model = await _context.LessonAddons.FirstOrDefaultAsync(item => item.LessonAddonID == key);
            if (model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Ссылка не найдена");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() {DateTimeZoneHandling = DateTimeZoneHandling.Local});

            PopulateLessonAddonModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            await _context.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteLessonAddon(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var model = await _context.LessonAddons.FirstOrDefaultAsync(item => item.LessonAddonID == key);

            if (model != null) _context.LessonAddons.Remove(model);
            await _context.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [HttpGet]
        public async Task<HttpResponseMessage> JournalFields(int lessonId)
        {
            var data = await _context.JournalFields
                .Where(l => l.LessonID == lessonId && (l.Flags & 128) != 128)
                .Select(jf => new
                {
                    jf.JournalFieldID,
                    jf.FieldName,
                    jf.FieldDescription,
                    jf.Flags,
                    jf.MaxValue
                }).ToListAsync();
            return Request.CreateResponse(HttpStatusCode.OK, data);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetFields(int lessonId)
        {
            var data = await _context.JournalFields
                .Where(l => l.LessonID == lessonId)
                .Select(jf => new
                {
                    jf.JournalFieldID,
                    jf.FieldName,
                    jf.FieldDescription,
                    jf.Flags,
                    jf.MaxValue,
                    ForPrint = (jf.Flags & 2) == 2,
                    UserVisability = (jf.Flags & 1) != 1
                }).ToListAsync();
            return Request.CreateResponse(HttpStatusCode.OK, data);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> InsertField(FormDataCollection form, int lessonId)
        {
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() {DateTimeZoneHandling = DateTimeZoneHandling.Local});

            var model = new JournalField()
            {
                LessonID = lessonId,
                CreateDate = DateTime.Now
            };

            PopulateFieldModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var message = ValidateField(model);
            if (message != "") return Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);

            _context.JournalFields.Add(model);
            await _context.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpPut]
        public async Task<HttpResponseMessage> UpdateField(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var model = await _context.JournalFields.FirstOrDefaultAsync(item => item.JournalFieldID == key);
            if (model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Колонка не найдена");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"),
                new JsonSerializerSettings() {DateTimeZoneHandling = DateTimeZoneHandling.Local});

            PopulateFieldModel(model, values);
            model.UpdateDate = DateTime.Now;
            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var message = ValidateField(model);
            if (message != "")
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);

            await _context.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [HttpGet]
        public async Task<HttpResponseMessage> GetJournalFieldValues(int lessonId, int groupId)
        {
            var lDate = _context.Lessons.FirstOrDefault(l => l.LessonID == lessonId).LessonDate;
            var groupStartTime = _context.Groups.FirstOrDefault(g => g.GroupID == groupId).StartDateTime.TimeOfDay;

            var data = await _context.PersonGroups
                .Where(pg => pg.OrderID != null && pg.GroupID == groupId && pg.CreateDate <= lDate &&
                             (
                                 (pg.Flags & 164) == 0 ||
                                 (pg.Flags & 4) == 4 && pg.EndEducationDateTime > lDate ||
                                 (pg.Flags & 128) == 128 && pg.DeleteDate > lDate
                             )
                )
                .Join(_context.Persons,
                    pg => pg.PersonID,
                    p => p.PersonID,
                    (pg, p) => new
                    {
                        pg.PersonGroupID,
                        p.PersonID,
                        pg.GroupID,
                        FIO = p.LastName + " " + p.FirstName + " " + p.Patronymic
                    })
                .Join(_context.Lessons.Where(l => l.LessonID == lessonId),
                    pg1 => pg1.GroupID,
                    l => l.GroupID,
                    (pg1, l) => new
                    {
                        pg1.PersonID,
                        pg1.PersonGroupID,
                        pg1.FIO,
                        l.LessonID
                    }
                )
                .Join(_context.JournalFields,
                    l1 => l1.LessonID,
                    jf => jf.LessonID,
                    (l1, jf) => new
                    {
                        l1.PersonGroupID,
                        l1.PersonID,
                        l1.FIO,
                        jf.FieldName,
                        jf.JournalFieldID,
                        JFV = jf.JournalFieldValues.FirstOrDefault(jfv => jfv.PersonGroupID == l1.PersonGroupID),
                        IsLessonMaterials = _context.LessonMaterials.Any(i =>
                            i.PersonGroupID == l1.PersonGroupID && i.LessonID == l1.LessonID)
                    })
                .OrderBy(o => o.PersonGroupID).ToListAsync();
            return Request.CreateResponse(HttpStatusCode.OK, data);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetJournalFieldValuesForPersonGroup(int lessonId, int personGroupId)
        {
            var data = await _context.JournalFields.Where(jf => jf.LessonID == lessonId && (jf.Flags & 129) == 0)
                .Select(x=>new
                {
                    x.JournalFieldID,
                    x.FieldName,
                    x.FieldDescription,
                    x.MaxValue,
                    Value = _context.JournalFieldValues
                        .FirstOrDefault(jfv=>jfv.PersonGroupID==personGroupId && jfv.JournalFieldID==x.JournalFieldID) 

                })
                .ToListAsync();

            return Request.CreateResponse(HttpStatusCode.OK, data);
        }

        [HttpPut]
        public async Task<HttpResponseMessage> UpdateJournalValue(int personGroupId, int journalFieldId, int? value)
        {
            var item = await _context.JournalFieldValues.FirstOrDefaultAsync(j =>
                j.PersonGroupID == personGroupId && j.JournalFieldID == journalFieldId);
            var jf = await _context.JournalFields.FirstOrDefaultAsync(j => j.JournalFieldID == journalFieldId);

            if (jf != null && value != null && jf.MaxValue < value)
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    "Поле не может содержать значение больше максимального");

            if (value != null)
                if (item != null)
                {
                    _context.Entry(item).State = EntityState.Modified;
                    item.Value = value.Value;
                    item.UpdateDate = DateTime.Now;
                }
                else
                {
                    var i = new JournalFieldValue()
                    {
                        PersonGroupID = personGroupId,
                        JournalFieldID = journalFieldId,
                        Value = value.Value,
                        Flags = 0,
                        CreateDate = DateTime.Now
                    };
                    _context.JournalFieldValues.Add(i);
                }
            else if (item != null)
            {
                _context.JournalFieldValues.Remove(item);
            }

            await _context.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.OK);
        }



        private void PopulateLessonModel(Lesson model, IDictionary values)
        {
            string LESSON_DATE = nameof(Lesson.LessonDate);
            string DURATION = nameof(Lesson.Duration);
            string FLAGS = nameof(Lesson.Flags);

            if (values.Contains(LESSON_DATE))
            {
                model.LessonDate = Convert.ToDateTime(values[LESSON_DATE]);
            }

            if (values.Contains(DURATION))
            {
                model.Duration = Convert.ToInt32(values[DURATION]);
            }

            if (values.Contains(FLAGS))
            {
                model.Flags = Convert.ToInt32(values[FLAGS]);
            }
        }

        private string ValidateLesson(Lesson model)
        {
            var message = "";
            var group = _context.Groups.FirstOrDefault(g => g.GroupID == model.GroupID);
            if (group == null) return "Не удалось найти группу!";

            if (_context.Lessons.Any(l =>
                l.GroupID == model.GroupID && l.LessonDate == model.LessonDate && l.LessonID != model.LessonID))
                message += $"В группе {group.GroupName} уже есть занятие с датой {model.LessonDate}!\r\n";

            if (model.Duration != group.LessonDuration)
                message +=
                    $"Продолжительность занятия не соответствует установленной продолжительности занятия в группе {group.GroupName}! " +
                    $"(Должно равняться {group.LessonDuration} а.ч.)\r\n";

            return message;
        }


        private void PopulateTopicModel(Topic model, IDictionary values, int lessonId)
        {
            string TOPIC_NAME = nameof(Topic.TopicName);
            string DESCRIPTION = nameof(Topic.Description);
            string FLAGS = nameof(Topic.Flags);
            string IN_LESSON = "InLesson";

            if (values.Contains(TOPIC_NAME))
            {
                model.TopicName = Convert.ToString(values[TOPIC_NAME]);
            }

            if (values.Contains(DESCRIPTION))
            {
                model.Description = Convert.ToString(values[DESCRIPTION]);
            }

            if (values.Contains(FLAGS))
            {
                model.Flags = Convert.ToInt32(values[FLAGS]);
            }

            if (values.Contains(IN_LESSON))
            {
                var value = Convert.ToBoolean(values[IN_LESSON]);
                var lesson = _context.Lessons.FirstOrDefault(l => l.LessonID == lessonId);
                if (value)
                {
                    if (!model.Lessons.Contains(lesson)) model.Lessons.Add(lesson);
                }
                else
                {
                    if (model.Lessons.Contains(lesson)) model.Lessons.Remove(lesson);
                }
            }
        }


        private void PopulateLessonAddonModel(LessonAddon model, IDictionary values)
        {
            string ADDON_SHORT_NAME = nameof(LessonAddon.AddonShortName);
            string ADDON_DESCRIPTION = nameof(LessonAddon.AddonDescription);
            string LINK_ADDRESS = nameof(LessonAddon.LinkAddress);
            string START_DATE = nameof(LessonAddon.StartDate);
            string START_TIME = nameof(LessonAddon.StartTime);
            string FLAGS = nameof(LessonAddon.Flags);

            if (values.Contains(ADDON_SHORT_NAME))
            {
                model.AddonShortName = Convert.ToString(values[ADDON_SHORT_NAME]);
            }

            if (values.Contains(ADDON_DESCRIPTION))
            {
                model.AddonDescription = Convert.ToString(values[ADDON_DESCRIPTION]);
            }

            if (values.Contains(LINK_ADDRESS))
            {
                model.LinkAddress = Convert.ToString(values[LINK_ADDRESS]);
            }

            if (values.Contains(START_DATE))
            {
                model.StartDate = values[START_DATE] != null
                    ? Convert.ToDateTime(values[START_DATE]).Date
                    : (DateTime?) null;
            }

            if (values.Contains(START_TIME))
            {
                var t = Convert.ToDateTime(values[START_TIME]);
                model.StartTime = new TimeSpan(t.Hour, t.Minute, 00);
            }

            if (values.Contains(FLAGS))
            {
                model.Flags = Convert.ToInt32(values[FLAGS]);
            }

        }


        private void PopulateFieldModel(JournalField model, IDictionary values)
        {
            string FIELD_NAME = nameof(JournalField.FieldName);
            string FIELD_DESCRIPTION = nameof(JournalField.FieldDescription);
            string MAX_VALUE = nameof(JournalField.MaxValue);
            string FLAGS = nameof(JournalField.Flags);
            string FOR_PRINT = "ForPrint";
            string USER_VISSABILITY = "UserVisability";


            if (values.Contains(FIELD_NAME))
            {
                model.FieldName = Convert.ToString(values[FIELD_NAME]);
            }

            if (values.Contains(FIELD_DESCRIPTION))
            {
                model.FieldDescription = Convert.ToString(values[FIELD_DESCRIPTION]);
            }

            if (values.Contains(MAX_VALUE))
            {
                model.MaxValue = Convert.ToInt32(values[MAX_VALUE]);
            }

            if (values.Contains(FLAGS))
            {
                model.Flags |= Convert.ToInt32(values[FLAGS]);
            }

            if (values.Contains(FOR_PRINT))
            {
                var foo = Convert.ToBoolean(values[FOR_PRINT]);
                if (foo)
                {
                    model.Flags |= 2;
                }
                else
                {
                    model.Flags ^= 2;
                }
            }

            if (values.Contains(USER_VISSABILITY))
            {
                var foo = Convert.ToBoolean(values[USER_VISSABILITY]);
                if (foo)
                {
                    model.Flags ^= 1;
                }
                else
                {
                    model.Flags |= 1;
                }
            }
        }
    



        private string ValidateField(JournalField model)
        {
            var message = "";

            var lessonFields = _context.JournalFields.Where(j =>
                j.LessonID == model.LessonID && j.JournalFieldID != model.JournalFieldID).Select(j=>j.FieldName);
            if (lessonFields.Contains(model.FieldName))
                message +=
                    $"В занятии от {(_context.Lessons.FirstOrDefault(l => l.LessonID == model.LessonID).LessonDate).Date} " +
                    $"уже есть колонка в журнале с именем {model.FieldName}!";

            return message;
        }


        private string GetFullErrorMessage(ModelStateDictionary modelState)
        {
            var messages = new List<string>();

            foreach (var entry in modelState)
            {
                foreach (var error in entry.Value.Errors)
                    messages.Add(error.ErrorMessage);
            }

            return String.Join(" ", messages);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
