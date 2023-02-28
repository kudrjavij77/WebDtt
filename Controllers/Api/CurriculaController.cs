using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.InteropServices;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebDtt.Models;

namespace WebDtt.Controllers.Api
{
    [Route("api/Curricula/{action}", Name = "CurriculaApi")]
    [Authorize(Roles = "manager")]
    public class CurriculaController : ApiController
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };


        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var curriculum = _context.Curricula.Select(i => new {
                i.CurriculumID,
                i.CourseName,
                i.Class,
                i.Flags,
                i.SubjectID,
                i.Description
            });

            // If you work with a large amount of data, consider specifying the PaginateViaPrimaryKey and PrimaryKey properties.
            // In this case, keys and data are loaded in separate queries. This can make the SQL execution plan more efficient.
            // Refer to the topic https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "CurriculumID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Request.CreateResponse(DataSourceLoader.Load(curriculum, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new Curriculum();
            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            if (!ValidateCurricula(model))
                return Request.CreateErrorResponse(HttpStatusCode.Conflict,
                    "Курс с такими названием, классом и предметом уже существует!");

            var result = _context.Curricula.Add(model);
            _context.SaveChanges();
            return Request.CreateResponse(HttpStatusCode.Created, result.CurriculumID);


        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Curricula.FirstOrDefault(item => item.CurriculumID == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Curriculum not found");

            var values = JsonConvert.DeserializeObject<IDictionary>(form.Get("values"));
            PopulateModel(model, values);

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            if (!ValidateCurricula(model))
                return Request.CreateErrorResponse(HttpStatusCode.Conflict,
                    "Курс с такими названием, классом и предметом уже существует!");

            var error = ValidateCurriculaChanged(model);
            if (error != "")
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);

            _context.SaveChanges();
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private string ValidateCurriculaChanged(Curriculum model)
        {
            var message = "";
            var currSubject = _context.Subjects.FirstOrDefault(s => s.SubjectID == model.SubjectID);

            var groups = _context.Groups
                .Select(g=>new
                {
                    g.GroupID,
                    g.GroupName,
                    g.CurriculumID,
                    g.Flags,
                    g.PersonGroups,
                    Teachers = _context.Persons.Select(p => new
                    {
                        p.PersonTypeID,
                        p.PersonID,
                        p.Flags,
                        p.ParticipantClass,
                        //p.LastName,p.FirstName,p.Patronymic,
                        ViewShortFio = p.LastName + " " + p.FirstName + " " + p.Patronymic,
                        Subjects = _context.Subjects.Where(s=>s.Persons.Contains(p)).ToList()
                    }).Where(p => p.PersonTypeID == 2 &&
                                  (p.Flags & 128) != 128 && 
                                  g.PersonGroups.Contains(_context.PersonGroups.FirstOrDefault(x => x.GroupID == g.GroupID && 
                                                                                                    x.PersonID == p.PersonID && 
                                                                                                    (x.Flags & 128) != 128))).ToList(),
                    Students = _context.Persons.Where(p => p.PersonTypeID == 1 &&
                        (p.Flags & 128) != 128 && g.PersonGroups.Contains(_context.PersonGroups
                            .FirstOrDefault(x => x.GroupID == g.GroupID && x.PersonID == p.PersonID && (x.Flags & 128) != 128))).ToList()
                })
                .Where(g => g.CurriculumID == model.CurriculumID && (g.Flags&128)!=128).ToList();
            if (!groups.Any()) return message;
            foreach (var group in groups)
            {
                if (group.Students.Any())
                    return "Невозможно изменить курс. На курсе есть группа с детьми!";

                if (!group.Teachers.Any()) continue;

                foreach (var teacher in group.Teachers)
                {
                    if (teacher.ParticipantClass != null && teacher.ParticipantClass != model.Class)
                        message +=
                            $"Нельзя изменить класс, т.к. есть группа {group.GroupName}, " +
                            $"в которой преподаватель {teacher.ViewShortFio} не может проводить занятия для {model.Class}-го класса!\r\n";

                    if (!teacher.Subjects.Contains(currSubject))
                        message +=
                            $"Нельзя изменить предмет, т.к. есть группа {group.GroupName}, " +
                            $"в которой преподаватель {teacher.ViewShortFio} не может проводить занятия по предмету {currSubject.SubjectName}!\r\n";
                }
            }
            
            return message;
        }

        [HttpDelete]
        public void Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.Curricula.FirstOrDefault(item => item.CurriculumID == key);

            _context.Curricula.Remove(model);
            _context.SaveChanges();
        }

        private bool ValidateCurricula(Curriculum model)
        {
            var curriculums = _context.Curricula.Where(c =>
                c.CourseName == model.CourseName && c.Class == model.Class && c.SubjectID == model.SubjectID && c.Description==model.Description);
            return !curriculums.Any();
        }

        [HttpGet]
        public HttpResponseMessage SubjectsLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Subjects;
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        private void PopulateModel(Curriculum model, IDictionary values) {
            string CURRICULUM_ID = nameof(Curriculum.CurriculumID);
            string COURSE_NAME = nameof(Curriculum.CourseName);
            string CLASS = nameof(Curriculum.Class);
            string FLAGS = nameof(Curriculum.Flags);
            string SUBJECT_ID = nameof(Curriculum.SubjectID);
            string DESCRIPTION = nameof(Curriculum.Description);

            if(values.Contains(CURRICULUM_ID)) {
                model.CurriculumID = Convert.ToInt32(values[CURRICULUM_ID]);
            }

            if(values.Contains(COURSE_NAME)) {
                model.CourseName = Convert.ToString(values[COURSE_NAME]);
            }

            if(values.Contains(CLASS)) {
                model.Class = Convert.ToByte(values[CLASS]);
            }

            if(values.Contains(FLAGS)) {
                model.Flags = Convert.ToInt32(values[FLAGS]);
            }

            if(values.Contains(SUBJECT_ID)) {
                model.SubjectID = Convert.ToInt32(values[SUBJECT_ID]);
            }

            if (values.Contains(DESCRIPTION))
            {
                model.Description = Convert.ToString(values[DESCRIPTION]);
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