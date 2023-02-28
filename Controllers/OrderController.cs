using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DevExpress.DashboardWeb;
using DevExpress.RichEdit.Export;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using Newtonsoft.Json;
using WebDtt.Models;
using WebDtt.Models.Extentions;
using WebDtt.Reports;
using File = WebDtt.Models.File;
using Settings = WebDtt.Properties.Settings;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.UI.WebControls;
using DevExpress.Data.Helpers;
using DevExpress.Utils.Extensions;
using DevExpress.Web.Internal;
using WebDtt.Models.Dto;

namespace WebDtt.Controllers
{
    [Authorize(Roles="operator,manager,user")]
    public class OrderController : Controller
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        [Authorize(Roles = "operator,manager")]
        [HttpGet]
        public ActionResult AllOrders()
        {
            return View();
        }

        [Authorize(Roles = "operator,manager")]
        [HttpGet]
        public ActionResult AllOrdersKpc()
        {
            return View();
        }

        // GET: Order
        [Authorize(Roles = "user")]
        [HttpGet]
        public ActionResult OrdersForUser()
        {
            return View();
        }

        [Authorize(Roles = "user")]
        [HttpGet]
        public ActionResult NewUserOrderDtt()
        {
            return View();
        }

        [Authorize(Roles = "user")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewUserOrderDtt(Order model)
        {
            var student = _context.Persons
                .Include(p => p.User)
                .FirstOrDefault(p => p.PersonID == model.StudentPersonID);
            if (!ModelState.IsValid) return View(model);

            model.OrderTypeID = 1;
            model.CreateDate = DateTime.Now;
            _context.Orders.Add(model);

            var stExam = _context.StudentExams.Where(s => s.Flags < 4 && s.PersonID == model.StudentPersonID).Select(s=>s.ExamID);

            if (stExam.Intersect(model.Exams).Any())
            {
                ViewBag.ErrorMessage = $"У вас уже есть действующий договор на некоторые выбранные экзамены";
                return View(model);
            }


            if (student != null && student.ParentPersonID == null)
            {
                student.ParentPersonID = model.CreatorPersonID;
                _context.Entry(student).State = EntityState.Modified;
            }

            foreach (var examId in model.Exams)
            {
                _context.StudentExams.Add(new StudentExam()
                {
                    PersonID = model.StudentPersonID,
                    ExamID = examId,
                    OrderID = model.OrderID
                });
            }

            await _context.SaveChangesAsync();

            var order = _context.Orders
                .Include(o => o.OrderType)
                .Include(o => o.Person.PersonType)
                .Include(o => o.Person1.PersonType)
                .FirstOrDefault(o => o.OrderID == model.OrderID);
            var stExams = _context.StudentExams
                .Include(s => s.Exam.Subject)
                .Where(st => st.OrderID == model.OrderID);
            foreach (var studentExam in stExams) { order?.StudentExams.Add(studentExam); }

            var targetLocation = Server.MapPath($"~/Files/Orders/{order.OrderID}-{order.OrderType.OrderTypeName}");
            DirectoryInfo d = new DirectoryInfo(targetLocation);
            if (!d.Exists) d.Create();

            var orderFileName = $"Договор №{order.OrderID}.pdf";
            var orderPath = Path.Combine(targetLocation, orderFileName);
            var file = new File { FullPath = orderPath, FileName = orderFileName };
            _context.Files.Add(file);
            var receiptFileName = $"Реквизиты для оплаты договора №{order.OrderID}.pdf";
            var receiptPath = Path.Combine(targetLocation, receiptFileName);
            var file1 = new File { FullPath = receiptPath, FileName = receiptFileName };
            _context.Files.Add(file1);
            await _context.SaveChangesAsync();

            var fileMapping = new FileMapping
            {
                FileID = file.FileID,
                ObjectID = order.OrderID,
                ObjectName = "Order",
                FriendlyDescription = "OrderBlank"
            };
            _context.FileMappings.Add(fileMapping);
            var fileMapping1 = new FileMapping
            {
                FileID = file1.FileID,
                ObjectID = order.OrderID,
                ObjectName = "Order",
                FriendlyDescription = "ReceiptBlank"
            };
            _context.FileMappings.Add(fileMapping1);
            await _context.SaveChangesAsync();

            if (student != null)
            {
                EmailServiceSend emailService = new EmailServiceSend();
                await emailService.CreateNewOrderMailMessage(student.User.Email, order, model.ExamType, orderPath, receiptPath);
            }

            ViewBag.Message = "Valid";
            return RedirectToAction("OrdersForUser");

        }

        public ActionResult OrderDtt(int id)
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            Order order = _context.Orders
                .Include(o => o.Person1.User)
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null || (user.IsInRole("user") && order.Person1.User.UserID != userid))
                return RedirectToAction("Login", "Account");

            var fileMappingOrderBlank = _context.FileMappings
                .FirstOrDefault(f => f.ObjectID == id
                                     && f.ObjectName == "Order"
                                     && f.FriendlyDescription == "OrderBlank");
            var fileOrderBlank = _context.Files
                .FirstOrDefault(f => f.FileID == fileMappingOrderBlank.FileID);

            return fileOrderBlank != null
                ? File(fileOrderBlank.FullPath, 
                    "application/pdf",
                    fileOrderBlank.FileName)
                : null;
        }

        public ActionResult DownloadMomBlank(int id)
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            Order order = _context.Orders
                .Include(o => o.Person1.User)
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null || (user.IsInRole("user") && order.Person1.User.UserID != userid))
                return RedirectToAction("Login", "Account");

            var pg = _context.PersonGroups.FirstOrDefault(p => p.OrderID == order.OrderID);
            if (pg == null || (pg.Flags & 1) != 1 || (order.Flags & 16) != 16)
                return null;

            var momBlFileMapping = _context.FileMappings.FirstOrDefault(m =>
                m.ObjectID == order.OrderID && m.ObjectName == "Order" && m.FriendlyDescription == "MomBlank");

            var file = _context.Files.FirstOrDefault(f => f.FileID == momBlFileMapping.FileID);

            return file != null
                ? File(file.FullPath,
                    "application/pdf",
                    file.FileName)
                : null;

        }

        public ActionResult ReceiptDtt(int id)
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            Order order = _context.Orders
                .Include(o => o.Person1.User)
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null || (user.IsInRole("user") && order.Person1.User.UserID != userid))
                return RedirectToAction("Login", "Account");

            var fileMappingReceiptBlank = _context.FileMappings
                .FirstOrDefault(f => f.ObjectID == id
                                     && f.ObjectName == "Order"
                                     && f.FriendlyDescription == "ReceiptBlank");
            var fileReceiptBlank = _context.Files
                .FirstOrDefault(f => f.FileID == fileMappingReceiptBlank.FileID);

            return fileReceiptBlank != null
                ? File(fileReceiptBlank.FullPath,
                    "application/pdf",
                    fileReceiptBlank.FileName)
                : null;
        }

        public ActionResult ReceiptKpc(int orderId, int donateInfoId)
        {
            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            var order = _context.Orders
                .Include(o => o.Person1.User)
                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null || (user.IsInRole("user") && order.Person1.User.UserID != userid))
                return RedirectToAction("Login", "Account");

            var donInfo = _context.DonateInfoes.FirstOrDefault(d => d.DonateInfoID == donateInfoId);
            var frDescr = $"ReceiptBlank{donInfo.DonateNumber}";

            var fileMappingReceiptBlank = _context.FileMappings
                .FirstOrDefault(f => f.ObjectID == orderId
                                     && f.ObjectName == "Order"
                                     && f.FriendlyDescription == frDescr);
            var fileReceiptBlank = _context.Files
                .FirstOrDefault(f => f.FileID == fileMappingReceiptBlank.FileID);

            return fileReceiptBlank != null
                ? File(fileReceiptBlank.FullPath,
                    "application/pdf",
                    fileReceiptBlank.FileName)
                : null;
        }

        [Authorize(Roles = "operator,manager")]
        public FileResult DownLoadBill(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderType)
                .FirstOrDefault(o => o.OrderID == id);

            var startPath = Server.MapPath($"~/Files/Orders/{order.OrderID}-{order.OrderType.OrderTypeName}/Receipts/");
            var dir = new DirectoryInfo(startPath);
            if (!dir.Exists) return null;
            
            var zipPath = Server.MapPath($"~/Files/temp/{order.OrderID}-{order.OrderType.OrderTypeName}-Receipts.zip");
            var f = new FileInfo(zipPath);
            if(f.Exists) f.Delete();

            ZipFile.CreateFromDirectory(startPath,zipPath);

            return File(zipPath, "application/zip", $"{order.OrderID}-{order.OrderType.OrderTypeName}-Receipts.zip");

        }

        [Authorize(Roles = "operator,manager")]
        public FileResult DownloadSpravka(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderID == id);
            //TODO: заебенить: ФИО законного представителя,
            //серию и номер документа, кем и когда выдан,
            //ФИО слушателя, форма обучения, № договора + дата создания,
            //Стоимость обучения(возможно со скидкой), разбить оплаты на периоды.
            var donateType = _context.DonateTypes.FirstOrDefault(dt => dt.DonateTypeID == order.DonateTypeID);
            order.DonateType = donateType;
            var creator = _context.Persons.FirstOrDefault(p => p.PersonID == order.CreatorPersonID);
            var student = _context.Persons.FirstOrDefault(p => p.PersonID == order.StudentPersonID);
            var persGroup =
                _context.PersonGroups.FirstOrDefault(pg => pg.PersonID == student.PersonID & pg.OrderID == id);
            var group = _context.Groups.FirstOrDefault(g => g.GroupID == persGroup.GroupID);
            var edType = _context.EducationForms.FirstOrDefault(e => e.EducationFormID == group.EducationFormID);
            var donates = _context.DonateStatus.Where(ds => ds.OrderID == id & ds.Flags==2).ToList();
            var lessons = _context.Lessons.Where(l => l.GroupID == group.GroupID & l.Flags == 0)
                .OrderBy(l => l.LessonDate).ToList();

            var dataSource = new SpravkaDto(order, creator, student, persGroup, group, donates, edType.EducationFormName, (order.Flags & 4) != 0,
                lessons);


            var localPath = Server.MapPath($"~/Files/temp/{order.OrderID}.docx");

            using (var reminderBlank = new Spravka() { DataSource = dataSource })
            {

                //TODO: нарисовать отчет
                reminderBlank.ExportToDocx(localPath, new DocxExportOptions()
                {
                    ExportMode = DocxExportMode.SingleFile
                });
            }
            
            return File(new FileStream(localPath, FileMode.Open, FileAccess.Read),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"Справка для договора {order.OrderNumber} от {order.CreateDate.ToShortDateString()}.docx");
        }

        [Authorize(Roles = "user,operator,manager")]
        public ActionResult DownLoadBillKpc(int orderId, int fileId)
        {

            var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());

            var order = _context.Orders
                .Include(o => o.Person1.User)
                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null || (user.IsInRole("user") && order.Person1.User.UserID != userid))
                return RedirectToAction("Login", "Account");

            var fileReceiptBlank = _context.Files
                .FirstOrDefault(f => f.FileID == fileId);

            return fileReceiptBlank != null
                ? File(fileReceiptBlank.FullPath,
                    "application/pdf",
                    fileReceiptBlank.FileName)
                : null;

        }
        
        [HttpPost]
        public ActionResult Upload()
        {
            // Learn to use the entire functionality of the dxFileUploader widget.
            // http://js.devexpress.com/Documentation/Guide/UI_Widgets/UI_Widgets_-_Deep_Dive/dxFileUploader/

            var myFile = Request.Files["myFile"];
            var orderId = int.Parse(Request["key"]);
            var order = _context.Orders.Include(o => o.OrderType)
                .FirstOrDefault(o => o.OrderID == orderId);
            if (order == null) return new EmptyResult();

            switch (order.OrderTypeID)
            {
                case 1:
                {
                    var targetLocation =
                        Server.MapPath($"~/Files/Orders/{order.OrderID}-{order.OrderType.OrderTypeName}/Receipts/");
                    DirectoryInfo d = new DirectoryInfo(targetLocation);
                    if (!d.Exists) d.Create();

                    try
                    {
                        var path = Path.Combine(targetLocation, myFile.FileName);
                        //переделать изменение имени файла при добавлении 
                        int i = 0;
                        while (System.IO.File.Exists(path))
                        {
                            i++;
                            path += i;
                        }

                        myFile.SaveAs(path);
                        Session["currentFilePath"] = path;
                        var file = new File
                            {FullPath = path, FileName = myFile.FileName, ContentType = myFile.ContentType};
                        _context.Files.Add(file);

                        var fileMapping = new FileMapping
                        {
                            FileID = file.FileID,
                            ObjectID = order.OrderID,
                            ObjectName = order.GetType().Name.Split('_').First(),
                            FriendlyDescription = "ReceiptBill"
                        };
                        _context.FileMappings.Add(fileMapping);

                        //var currentOrder = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);
                        _context.Entry(order).State = EntityState.Modified;
                        order.ReceiptFileID = file.FileID;
                        if (order.Flags == 0) order.Flags = 1;
                        order.UpdateDate = DateTime.Now;
                        _context.SaveChanges();
                    }
                    catch
                    {
                        Response.StatusCode = 400;
                    }

                    break;
                }

                case 2:
                {
                    var targetLocation = Server.MapPath($"~/Files/Orders/{order.OrderID}-{order.OrderType.OrderTypeName}/Receipts/");
                    DirectoryInfo d = new DirectoryInfo(targetLocation);
                    if (!d.Exists) d.Create();

                    try
                    {
                        var path = Path.Combine(targetLocation, myFile.FileName);
                        //переделать изменение имени файла при добавлении 
                        int i = 0;
                        while (System.IO.File.Exists(path))
                        {
                            i++;
                            path += i;
                        }

                        myFile.SaveAs(path);
                        Session["currentFilePath"] = path;
                        var file = new File { FullPath = path, FileName = myFile.FileName, ContentType = myFile.ContentType };
                        _context.Files.Add(file);

                        var donate = new DonateStatu()
                            {
                                OrderID = order.OrderID,
                                FileID = file.FileID,
                                DonateDate = DateTime.Now,
                                CreateDate = DateTime.Now,
                                PartOfDonate = 0,
                                Total = 0,
                                Flags = 1
                            };
                        _context.DonateStatus.Add(donate);
                        _context.SaveChanges();

                        var fileMapping = new FileMapping
                        {
                            FileID = file.FileID,
                            ObjectID = donate.DonateID,
                            ObjectName = donate.GetType().Name.Split('_').First(),
                            FriendlyDescription = "ReceiptBill"
                        };
                        _context.FileMappings.Add(fileMapping);

                        //var currentOrder = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);
                        _context.Entry(order).State = EntityState.Modified;
                        //order.ReceiptFileID = file.FileID;
                        if (order.Flags == 0) order.Flags = 1;
                        order.UpdateDate = DateTime.Now;
                        _context.SaveChanges();
                    }
                    catch
                    {
                        Response.StatusCode = 400;
                    }
                    break;
                }
            }

            

            return new EmptyResult();
        }
        
        public ActionResult ReportViewer(int id)
        {
            var order = _context.Orders
                .Include(o=>o.OrderType)
                .Include(o=>o.Person.PersonType)
                .Include(o=>o.Person1.PersonType)
                .FirstOrDefault(o => o.OrderID == id);
            var stExams = _context.StudentExams
                .Include(s=>s.Exam.Subject)
                .Where(st => st.OrderID == id);

            foreach (var studentExam in stExams) { order?.StudentExams.Add(studentExam); }

            return View(new OrderDtt {DataSource = order});
        }

        [Authorize(Roles = "user")]
        [HttpGet]
        public ActionResult NewUserOrderKpc()
        {
            return View();
        }

        [Authorize(Roles = "user")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewUserOrderKpc(Order model)
        {
            if (!ModelState.IsValid) return View(model);
            var personGroupsOfUser = _context.PersonGroups
                .Where(pg => pg.PersonID == model.StudentPersonID && (pg.Flags & 4) != 4 && (pg.Flags & 128) != 128)
                .Select(pg => pg.GroupID).ToList();

            if (model.GroupID != null && personGroupsOfUser.Any() && personGroupsOfUser.Contains((int) model.GroupID))
            {
                ViewBag.ErrorMessage =
                    $"У вас уже есть действующий договор в выбранную группу.";
                return View(model);
            }

            var group = _context.Groups
                .Include(g => g.DonateTypes)
                //.Include(g=>g.PersonGroups.Where(pg=>pg.OrderID!=null && (pg.Flags & 4) != 4 && (pg.Flags & 128) != 128))
                .FirstOrDefault(g => g.GroupID == model.GroupID);

            if(group != null && ( group.Flags & 1) == 1)
            {
                ViewBag.ErrorMessage = $"В выбранную группу набор окончен! Выберите другую группу.";
                return View(model);
            }


            foreach (var i in personGroupsOfUser)
            {
                var gr = _context.Groups.FirstOrDefault(g => g.GroupID == i);
                if (group != null && (gr != null && gr.FinishDateTime > group.StartDateTime && gr.StartDateTime.DayOfWeek == group.StartDateTime.DayOfWeek))
                {
                    ViewBag.ErrorMessage = $"У вас уже есть действующий договор в другую группу в выбранный день.";
                    return View(model);
                }
            }

            var personGroups = _context.PersonGroups
                .Where(pg =>
                    pg.OrderID != null && (pg.Flags & 4) != 4 && (pg.Flags & 128) != 128 &&
                    pg.GroupID == model.GroupID);

            if (group != null && personGroups.Count() >= group.Capacity)
            {
                ViewBag.ErrorMessage =
                    $"В группе {group.GroupName} не осталось свободных мест. Выберите другую группу.";
                return View(model);
            }

            var course = _context.Groups.Include(g=>g.Curriculum).FirstOrDefault(g => g.GroupID == model.GroupID).Curriculum;
            var persGroups = _context.PersonGroups
                .Select(p => new
                {
                    p.OrderID,
                    p.Flags,
                    p.GroupID,
                    p.PersonID,
                    p.Group.Curriculum.SubjectID
                })
                .Where(p => p.OrderID != null && (p.Flags & 4) != 4 && (p.Flags & 128) != 128 && p.PersonID == model.StudentPersonID).ToList();


            ////TODO: запрет на регистрацию на один и тот же предмет
            //if (persGroups.Any(i => i.SubjectID == course.SubjectID))
            //{
            //    ViewBag.ErrorMessage =
            //        $"У вас уже есть действующий договор на выбранный предмет!";
            //    return View(model);
            //}

            if (model.MomBank && group != null)
            {
                model.CountDonates = 1;
                model.DonateTypeID = group.DonateTypes.FirstOrDefault(d => d.CountDonates == 1).DonateTypeID;
            }
            else
            {
                var donType = _context.DonateTypes.FirstOrDefault(dt => dt.DonateTypeID == model.DonateTypeID);
                if (donType != null)
                {
                    model.CountDonates = donType.CountDonates;
                    model.DonateTypeID = donType.DonateTypeID;
                }
            }

            model.OrderTypeID = 2;
            model.CreateDate = DateTime.Now;
            model.Discount = 0;
            model.Flags = model.MomBank ? 16 : 0;
            _context.Orders.Add(model);
            if (model.GroupID != null)
            {
                var flag = model.MomBank ? 1 : 0;
                _context.PersonGroups.Add(new PersonGroup()
                {
                    GroupID = (int) model.GroupID,
                    PersonID = model.StudentPersonID,
                    OrderID = model.OrderID,
                    CreateDate = DateTime.Now,
                    Flags = flag
                });
            }

            await _context.SaveChangesAsync();

            //выбор созданного экземпляра класса Order + OrderType, PersonGroup, Student, Parent, DonateType, DonateInfos, Group
            var o = _context.Orders.FirstOrDefault(or => or.OrderID == model.OrderID);
            if (o?.GroupID == null || o.DonateTypeID == null || o.CountDonates == null)
            {
                ViewBag.ErrorMessage =
                    $"Произошла ошибка! Order don't save to DB!";
                return View(model);
            }

            var order = new WebDtt.Models.Dto.OrderKpc()
            {
                OrderID = o.OrderID,
                OrderTypeID = o.OrderTypeID,
                OrderType = _context.OrderTypes.FirstOrDefault(ot => ot.OrderTypeID == o.OrderTypeID),
                CreatorPersonID = o.CreatorPersonID,
                CreatorPerson = _context.Persons.Include(p => p.PersonType)
                    .FirstOrDefault(p => p.PersonID == o.CreatorPersonID),
                StudentPersonID = o.StudentPersonID,
                StudentPerson = _context.Persons.Include(p => p.PersonType)
                    .FirstOrDefault(p => p.PersonID == o.StudentPersonID),
                GroupID = (int) o.GroupID,
                Group = _context.Groups.Include(g => g.Curriculum.Subject)
                    .Include(g => g.Station).Include(g=>g.EducationForm)
                    .FirstOrDefault(g => g.GroupID == o.GroupID),
                DonateTypeID = (int) o.DonateTypeID,
                DonateType = _context.DonateTypes.FirstOrDefault(dt => dt.DonateTypeID == o.DonateTypeID),
                DonateInfos = _context.DonateInfoes.Where(di => di.DonateTypeID == o.DonateTypeID)
                    .OrderBy(di => di.DonateNumber).ToList(),
                CountDonates = (int) o.CountDonates,
                CreateDate = o.CreateDate,
                Flags = o.Flags,
                MomBlank = (_context.PersonGroups.FirstOrDefault(p => p.OrderID == o.OrderID && p.PersonID == o.StudentPersonID).Flags & 1) == 1 || (o.Flags & 16) == 16,
                Discount = o.Discount
            };

            //создание каталога для хранения файла договора и реквизитов для оплаты
            var targetLocation = Server.MapPath($"~/Files/Orders/{order.OrderID}-{order.OrderType.OrderTypeName}");
            DirectoryInfo dir = new DirectoryInfo(targetLocation);
            if (!dir.Exists) dir.Create();

            //добавление в каталог файлов репортов договора, реквизитов, мат. капитал
            //создание в бд экземпляров класса FileMapping для файлов договора и реквизитов
            var orderFileName = $"Договор №{order.OrderID}_{order.Group.GroupName}.pdf";
            var orderPath = Path.Combine(targetLocation, orderFileName);
            var orderFile = new File {FileName = orderFileName, FullPath = orderPath};
            _context.Files.Add(orderFile);
            await _context.SaveChangesAsync();
            var fileMapping = new FileMapping
            {
                FileID = orderFile.FileID,
                ObjectID = order.OrderID,
                ObjectName = "Order",
                FriendlyDescription = "OrderBlank"
            };
            _context.FileMappings.Add(fileMapping);

            var receiptPaths = new List<string>();
            foreach (var donateInfo in order.DonateInfos)
            {
                var receiptFileName =
                    $"Реквизиты для оплаты договора №{order.OrderID}_{order.Group.GroupName}(этап {donateInfo.DonateNumber}).pdf";
                var receiptPath = Path.Combine(targetLocation, receiptFileName);
                var receiptFile = new File {FileName = receiptFileName, FullPath = receiptPath};
                _context.Files.Add(receiptFile);
                await _context.SaveChangesAsync();

                var fileMapping1 = new FileMapping
                {
                    FileID = receiptFile.FileID,
                    ObjectID = order.OrderID,
                    ObjectName = "Order",
                    FriendlyDescription = $"ReceiptBlank{donateInfo.DonateNumber}"
                };
                _context.FileMappings.Add(fileMapping1);
                await _context.SaveChangesAsync();

                receiptPaths.Add(receiptPath);
            }

            string momBlankPathToEmail = null;
            if (model.MomBank)
            {
                var momBlankFileName =
                    $"Дополнительное соглашение к договору №{order.OrderID}_{order.Group.GroupName}.pdf";
                var momBlankPath = Path.Combine(targetLocation, momBlankFileName);
                var momBlankFile = new File {FileName = momBlankFileName, FullPath = momBlankPath};
                _context.Files.Add(momBlankFile);
                await _context.SaveChangesAsync();

                var fileMappingMomBlank = new FileMapping
                {
                    FileID = momBlankFile.FileID,
                    ObjectID = order.OrderID,
                    ObjectName = "Order",
                    FriendlyDescription = "MomBlank"
                };
                _context.FileMappings.Add(fileMappingMomBlank);
                await _context.SaveChangesAsync();

                momBlankPathToEmail = momBlankPath;
            }

            var reminderFileName = "ПАМЯТКА РОДИТЕЛЯМ И СЛУШАТЕЛЯМ.pdf";
            var reminderPath = Path.Combine(targetLocation, reminderFileName);
            var reminderFile = new File {FileName = reminderFileName, FullPath = reminderPath};
            _context.Files.Add(reminderFile);
            await _context.SaveChangesAsync();

            var fileMappingReminderBlank = new FileMapping
            {
                FileID = reminderFile.FileID,
                ObjectID = order.OrderID,
                ObjectName = "Order",
                FriendlyDescription = "ReminderBlank"
            };
            _context.FileMappings.Add(fileMappingReminderBlank);
            await _context.SaveChangesAsync();

            //отправка на почту письма об успешном создании договора с инструкцией об оплате + вложения (договор, реквизиты, шаблон мат.капитала)
            var student = _context.Persons.Include(p => p.User)
                .FirstOrDefault(p => p.PersonID == order.StudentPersonID);

            //order.OrderNumber = $"{order.OrderID}/{order.Group.GroupName}";
            foreach (var info in order.DonateInfos.Where(info => info.DonateNumber == 1))
            {
                info.EndDate = order.CreateDate.AddDays(5);
            }

            EmailServiceSend emailServiceSend = new EmailServiceSend();
            await emailServiceSend.CreateNewOrderKpcMailMessage(student.User.Email, order, orderPath,
                receiptPaths, momBlankPathToEmail, reminderPath);


            ViewBag.Message = "Valid";
            return RedirectToAction("OrdersForUser");
        }

        [Authorize]
        public async Task<ActionResult> ReCreateAndReSendOrderReports(int orderId)
        {
            var o = _context.Orders.FirstOrDefault(or => or.OrderID == orderId);
            if (o == null || o.DonateTypeID == null || o.CountDonates == null)
            {
                ViewBag.ErrorMessage =
                    $"Произошла ошибка! Order don't save to DB!";
                return null;
            }

            var order = new WebDtt.Models.Dto.OrderKpc()
            {
                OrderID = o.OrderID,
                OrderTypeID = o.OrderTypeID,
                OrderType = _context.OrderTypes.FirstOrDefault(ot => ot.OrderTypeID == o.OrderTypeID),
                CreatorPersonID = o.CreatorPersonID,
                CreatorPerson = _context.Persons.Include(p => p.PersonType)
                    .FirstOrDefault(p => p.PersonID == o.CreatorPersonID),
                StudentPersonID = o.StudentPersonID,
                StudentPerson = _context.Persons.Include(p => p.PersonType)
                    .FirstOrDefault(p => p.PersonID == o.StudentPersonID),
                GroupID = _context.PersonGroups.FirstOrDefault(pg=>pg.OrderID != null && pg.OrderID==o.OrderID).GroupID,
                Group = _context.Groups.Include(g => g.Curriculum.Subject)
                    .Include(g => g.Station).Include(g => g.EducationForm)
                    .FirstOrDefault(g => g.GroupID == _context.PersonGroups.FirstOrDefault(pg => pg.OrderID != null && pg.OrderID == o.OrderID).GroupID),
                DonateTypeID = (int)o.DonateTypeID,
                DonateType = _context.DonateTypes.FirstOrDefault(dt => dt.DonateTypeID == o.DonateTypeID),
                DonateInfos = _context.DonateInfoes.Where(di => di.DonateTypeID == o.DonateTypeID)
                    .OrderBy(di => di.DonateNumber).ToList(),
                CountDonates = (int)o.CountDonates,
                CreateDate = o.CreateDate,
                Flags = o.Flags,
                MomBlank = (_context.PersonGroups.FirstOrDefault(p => p.OrderID == o.OrderID && p.PersonID == o.StudentPersonID).Flags & 1) == 1 || (o.Flags & 16) == 16,
                Discount = o.Discount
            };

            

            //создание каталога для хранения файла договора и реквизитов для оплаты
            var targetLocation = Server.MapPath($"~/Files/Orders/{order.OrderID}-{order.OrderType.OrderTypeName}");
            DirectoryInfo dir = new DirectoryInfo(targetLocation);
            if (!dir.Exists) dir.Create();

            //добавление в каталог файлов репортов договора, реквизитов, мат. капитал
            //создание в бд экземпляров класса FileMapping для файлов договора и реквизитов
            var orderFileName = $"Договор №{order.OrderID}_{order.Group.GroupName}.pdf";
            var orderPath = Path.Combine(targetLocation, orderFileName);
            var orderFile = _context.Files.FirstOrDefault(f => f.FileName == orderFileName && f.FullPath == orderPath);
            if (orderFile == null)
            {
                orderFile = new File { FileName = orderFileName, FullPath = orderPath };
                _context.Files.Add(orderFile);
                await _context.SaveChangesAsync();
                
            }

            var fileMapping = _context.FileMappings
                .FirstOrDefault(f => f.FileID == orderFile.FileID && f.ObjectID == order.OrderID &&
                                     f.ObjectName == "Order" && f.FriendlyDescription == "OrderBlank");
            if (fileMapping == null)
            {
                fileMapping = new FileMapping
                {
                    FileID = orderFile.FileID,
                    ObjectID = order.OrderID,
                    ObjectName = "Order",
                    FriendlyDescription = "OrderBlank"
                };
                _context.FileMappings.Add(fileMapping);
            }


            var receiptPaths = new List<string>();
            foreach (var donateInfo in order.DonateInfos)
            {
                var receiptFileName =
                    $"Реквизиты для оплаты договора №{order.OrderID}_{order.Group.GroupName}(этап {donateInfo.DonateNumber}).pdf";
                var receiptPath = Path.Combine(targetLocation, receiptFileName);
                var receiptFile =
                    _context.Files.FirstOrDefault(f => f.FileName == receiptFileName && f.FullPath == receiptPath);
                if (receiptFile == null)
                {
                    receiptFile = new File { FileName = receiptFileName, FullPath = receiptPath };
                    _context.Files.Add(receiptFile);
                    await _context.SaveChangesAsync();
                }

                var strFriendly = $"ReceiptBlank{donateInfo.DonateNumber}";

                var fileMapping1 = _context.FileMappings
                    .FirstOrDefault(f => f.FileID == receiptFile.FileID && f.ObjectID == order.OrderID &&
                                         f.ObjectName == "Order" && f.FriendlyDescription == strFriendly);
                if (fileMapping1 == null)
                {
                    fileMapping1 = new FileMapping
                    {
                        FileID = receiptFile.FileID,
                        ObjectID = order.OrderID,
                        ObjectName = "Order",
                        FriendlyDescription = $"ReceiptBlank{donateInfo.DonateNumber}"
                    };
                    _context.FileMappings.Add(fileMapping1);
                    await _context.SaveChangesAsync();
                }

                receiptPaths.Add(receiptPath);
            }

            string momBlankPathToEmail = null;
            if (order.MomBlank)
            {
                var momBlankFileName =
                    $"Дополнительное соглашение к договору №{order.OrderID}_{order.Group.GroupName}.pdf";
                var momBlankPath = Path.Combine(targetLocation, momBlankFileName);
                var momBlankFile = _context.Files.FirstOrDefault(f => f.FileName == momBlankFileName && f.FullPath == momBlankPath);
                if(momBlankFile==null)
                {
                    momBlankFile = new File {FileName = momBlankFileName, FullPath = momBlankPath};
                    _context.Files.Add(momBlankFile);
                    await _context.SaveChangesAsync();
                }

                var fileMappingMomBlank = _context.FileMappings
                    .FirstOrDefault(f => f.FileID == momBlankFile.FileID && f.ObjectID == order.OrderID &&
                                         f.ObjectName == "Order" && f.FriendlyDescription == "MomBlank");
                if (fileMappingMomBlank == null)
                {
                    fileMappingMomBlank = new FileMapping
                    {
                        FileID = momBlankFile.FileID,
                        ObjectID = order.OrderID,
                        ObjectName = "Order",
                        FriendlyDescription = "MomBlank"
                    };
                    _context.FileMappings.Add(fileMappingMomBlank);
                    await _context.SaveChangesAsync();
                }

                momBlankPathToEmail = momBlankPath;
            }

            var reminderFileName = "ПАМЯТКА РОДИТЕЛЯМ И СЛУШАТЕЛЯМ.pdf";
            var reminderPath = Path.Combine(targetLocation, reminderFileName);
            var reminderFile = _context.Files.FirstOrDefault(f => f.FileName == reminderFileName && f.FullPath == reminderPath);
            if (reminderFile == null)
            {
                reminderFile = new File { FileName = reminderFileName, FullPath = reminderPath };
                _context.Files.Add(reminderFile);
                await _context.SaveChangesAsync();
            }
            
            var fileMappingReminderBlank = _context.FileMappings
                .FirstOrDefault(f => f.FileID == reminderFile.FileID && f.ObjectID == order.OrderID &&
                                     f.ObjectName == "Order" && f.FriendlyDescription == "ReminderBlank");
            if (fileMappingReminderBlank == null)
            {
                fileMappingReminderBlank = new FileMapping
                {
                    FileID = reminderFile.FileID,
                    ObjectID = order.OrderID,
                    ObjectName = "Order",
                    FriendlyDescription = "ReminderBlank"
                };
                _context.FileMappings.Add(fileMappingReminderBlank);
                await _context.SaveChangesAsync();
            }

            //отправка на почту письма об успешном создании договора с инструкцией об оплате + вложения (договор, реквизиты, шаблон мат.капитала)
            var student = _context.Persons.Include(p => p.User)
                .FirstOrDefault(p => p.PersonID == order.StudentPersonID);

            //order.OrderNumber = $"{order.OrderID}/{order.Group.GroupName}";
            foreach (var info in order.DonateInfos)
            {
                if(info.DonateNumber==1)info.EndDate = order.CreateDate.AddDays(5);
                info.Total = info.Total * (100 - order.Discount) / 100;
                info.TotalString = RusNumber.RusSpelledOut(info.Total, true);
            }

            order.DonateType.FullTotal = order.DonateType.FullTotal * (100 - order.Discount) / 100;
            order.DonateType.FullTotalString = RusNumber.RusSpelledOut(order.DonateType.FullTotal, true);

            EmailServiceSend emailServiceSend = new EmailServiceSend();
            await emailServiceSend.CreateNewOrderKpcMailMessage(student.User.Email, order, orderPath,
                receiptPaths, momBlankPathToEmail, reminderPath);


            ViewBag.Message = "Valid";
            return RedirectToAction(User.IsInRole("user") ? "OrdersForUser" : "AllOrdersKpc");
        }
    }
}