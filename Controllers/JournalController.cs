using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DevExpress.DashboardCommon.DataProcessing;
using WebDtt.Models;
using File = WebDtt.Models.File;

namespace WebDtt.Controllers
{
    [Authorize]
    public class JournalController : Controller
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = {LazyLoadingEnabled = false}
        };
        // GET: Journal
        public ActionResult JournalView()
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var id = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());
            //var personId = _context.Persons.FirstOrDefault(p => p.UserID == id).PersonID;

            //ViewBag.UserID = id;
            return View();
        }

        public ActionResult UploadLessonMaterial()
        {
            var myFile = Request.Files["myFile"];
            var lessonId = int.Parse(Request["lessonId"]);
            var group = _context.Groups.FirstOrDefault(g =>
                g.GroupID == _context.Lessons.FirstOrDefault(l => l.LessonID == lessonId).GroupID);
            var personGroupId = 0;
            if (User.IsInRole("user"))
            {
                personGroupId = int.Parse(Request["personGroupId"]);
            }
            else
            {
                personGroupId = _context.PersonGroups
                    .FirstOrDefault(pg => (pg.Flags & 128) != 128 && pg.OrderID == null && pg.GroupID==group.GroupID).PersonGroupID;
                if (personGroupId == 0) 
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, 
                        $"Невозможно добавить файл! В группу {group.GroupName} нет ни одного назначенного преподавателя!");
            }

            string targetLocation;
            var d = new DirectoryInfo(Server.MapPath($"~/Files/LessonMaterials/"));
            if (!d.Exists) d.Create();
            targetLocation = d.CreateSubdirectory($@"Group{group.GroupID}\Lesson{lessonId}\PersonGroup{personGroupId}").FullName;

            var path = Path.Combine(targetLocation, myFile.FileName);
            try
            {
                if (System.IO.File.Exists(path)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Файл с таким именем уже существует!");
                
                //проверка на существование файла

                myFile.SaveAs(path);
                Session["currentFilePath"] = path;
            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Не удалось записать файл!");
            }

            var file = new File { FullPath = path, FileName = myFile.FileName, ContentType = myFile.ContentType };
            _context.Files.Add(file);

            var lessonMaterial = new LessonMaterial()
            {
                LessonID = lessonId,
                FileID = file.FileID,
                Flags = 0,
                CreateDate = DateTime.Now,
                PersonGroupID = personGroupId
            };
            _context.LessonMaterials.Add(lessonMaterial);

            _context.SaveChanges();

            return new EmptyResult();
        }

        public ActionResult DownLoadLessonMaterial(int lessonMaterialId)
        {
            var lessonMaterial = _context.LessonMaterials.FirstOrDefault(l => l.LessonMaterialID == lessonMaterialId);
            if(lessonMaterial==null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Файл не найден!");

            if (User.IsInRole("manager")) return RedirectToAction("GetFile", new {fileId = lessonMaterial.FileID});

            var groupId = _context.Groups.FirstOrDefault(g =>
                g.GroupID == _context.LessonMaterials.FirstOrDefault(l => l.LessonMaterialID == lessonMaterialId)
                    .PersonGroup.GroupID).GroupID;
            

            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userId = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            if (User.IsInRole("teacher") &&
                _context.PersonGroups.Any(p => p.Person.UserID == userId && p.GroupID == groupId))
                return RedirectToAction("GetFile", new { fileId = lessonMaterial.FileID });


            var groupIdofUser = _context.PersonGroups
                .Where(pg => pg.Person.UserID == userId)
                .Select(pg => pg.GroupID).ToList();

            var pgTeachers = new List<int>();

            foreach (var id in groupIdofUser)
            {
                pgTeachers.AddRange(_context.PersonGroups
                    .Where(pg => pg.GroupID == id && pg.Person.PersonTypeID == 2)
                    .Select(pg => pg.PersonGroupID).ToArray());
            }


            if(User.IsInRole("user") && 
               (_context.PersonGroups.Any(pg=>pg.Person.UserID==userId && pg.PersonGroupID==lessonMaterial.PersonGroupID) || 
                pgTeachers.Contains(lessonMaterial.PersonGroupID)))
                return GetFile(lessonMaterial.FileID);

                //return RedirectToAction("GetFile", new { fileId = lessonMaterial.FileID });

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest,"Недостаточно прав для скачивания файла!");
        }

        public FileResult GetFile(int fileId)
        {
            var file = _context.Files.FirstOrDefault(f => f.FileID == fileId);
            return file != null
                ? File(file.FullPath,
                    file.ContentType,
                    file.FileName)
                : null;
        }

    }
}