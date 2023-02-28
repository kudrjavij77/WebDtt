using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using DevExpress.Utils.Extensions;
using DevExpress.XtraEditors.Filtering.Templates;
using DevExpress.XtraScheduler.Native;
using WebDtt.Models;
using WebDtt.Models.Controllers;
using File = WebDtt.Models.File;

namespace WebDtt.Controllers
{
    [Authorize]
    public class CAnswerFileController : Controller
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };
        // GET: CAnswerFile
        [HttpPost]
        public ActionResult UploadCAnswerFile()
        {
            //// Learn to use the entire functionality of the dxFileUploader widget.
            //// http://js.devexpress.com/Documentation/Guide/UI_Widgets/UI_Widgets_-_Deep_Dive/dxFileUploader/

            var myFile = Request.Files["myFile"];
            var studentExamId = int.Parse(Request["key"]);

            var studentExam = _context.StudentExams
                .Include(s=>s.CAnswerFiles)
                .FirstOrDefault(s => s.StudentExamID == studentExamId);

            if (studentExam.FinishDateTime == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "5");
            }

            var stopChangedDateTime = studentExam.FinishDateTime + new TimeSpan(23, 59, 59);
            if (DateTime.Now > stopChangedDateTime)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "1");
            }

            if (DateTime.Now < studentExam.FinishDateTime || studentExam.FinishDateTime == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "2");
            }



            string targetLocation;
            var d = new DirectoryInfo(Server.MapPath($"~/Files/StudentExams/"));
            if(!d.Exists)
            {
                d.Create();
                targetLocation = d.CreateSubdirectory($@"{studentExamId}\CAnswerFiles").FullName;
            }
            else
            {
                var d1 = new DirectoryInfo($"{d.FullName}{studentExamId}/");
                //var a = 1;
                if (!d1.Exists)
                {
                    targetLocation = d1.CreateSubdirectory(@"CAnswerFiles").FullName;
                }
                else
                {
                    var d3 = new DirectoryInfo($"{d1.FullName}/CAnswerFiles/");
                    if (!d3.Exists)
                    {
                        d3.Create();
                        targetLocation = d3.FullName;
                    }
                    else
                    {
                        targetLocation = d3.FullName;
                    }

                }
            }


            try
            {
                var path = Path.Combine(targetLocation, myFile.FileName);
                if(System.IO.File.Exists(path)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "4");
                var newPageNumber = studentExam.CAnswerFiles.Count + 1;

                //проверка на существование файла

                myFile.SaveAs(path);
                Session["currentFilePath"] = path;
                var file = new File { FullPath = path, FileName = myFile.FileName, ContentType = myFile.ContentType };
                _context.Files.Add(file);

                var cAnswerFile = new CAnswerFile { PageNumber = newPageNumber, CreateDate = DateTime.Now, FileID = file.FileID, StudentExamID = studentExamId};
                _context.CAnswerFiles.Add(cAnswerFile);

                _context.SaveChangesAsync();
            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "3");
            }
            
            return new EmptyResult();
        }

        public ActionResult DownLoadFile(int id)
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            

            var cAnswerFile = _context.CAnswerFiles
                .Include(c=>c.StudentExam.Person)
                .FirstOrDefault(c => c.CAnswerFileID == id);

            if (cAnswerFile == null || (user.IsInRole("user") && cAnswerFile.StudentExam.Person.UserID != userid))
                return RedirectToAction("Login", "Account");

            var file = _context.Files
                .FirstOrDefault(f => f.FileID == cAnswerFile.FileID);

            return file != null
                ? File(file.FullPath,
                    file.ContentType,
                    file.FileName)
                : null;
        }

    }
}