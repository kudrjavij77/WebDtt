using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc.Html;
using DevExpress.DataProcessing;
using DevExpress.PivotGrid.ServerMode.Queryable;
using DevExpress.RichEdit.Export;
using DevExpress.XtraReports.Web.ClientControls.DataContracts;
using WebDtt.Models.Controllers;
using WebDtt.Models.CustomTypes;
using WebDtt.Reports;
using Settings = WebDtt.Properties.Settings;

namespace WebDtt.Models.Extentions
{
    public class EmailServiceSend
    {
      
        public async Task EmailConfirmedSend(int statusCode, User user, string callbackUrl)
        {

            MailMessage message = new MailMessage
            {
                Subject = $"Подтверждение учетной записи",
                Body = $"<html><head><title>Подтверждение Email</title></head>" +
                       $"<body>Подтвердите вашу учетную запись, пройдя по ссылке:  <a href=\"" + callbackUrl +
                       "\">подтвердить Email</a>" +
                       $"</body>" +
                       $"<footer>Пожалуйста, не отвечайте на это письмо.</footer></html>",
                IsBodyHtml = true
            };

            message.From = new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ");
            message.To.Add(new MailAddress(user.Email, "Дорогой друг"));
            await SendMail(message);
        }

        public async Task EmailForgotPasswordSend(int statuscode, User user, string callbackUrl)
        {
            MailMessage message = new MailMessage
            {
                Subject = $"Сброс пароля",
                Body = $"<html><head><title>Сброс пароля</title></head>" +
                       $"<body>Для сброса пароля, перейдите по ссылке:  <a href=\"" + callbackUrl +
                       "\">сбросить</a>" +
                       $"</body>" +
                       $"<footer>Пожалуйста, не отвечайте на это письмо.</footer></html>",
                IsBodyHtml = true
            };

            message.From = new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ");
            message.To.Add(new MailAddress(user.Email, "Дорогой друг"));
            await SendMail(message);


        }
        public async Task CreateExpert(Person expert, string callbackUrl)
        {
            MailMessage message = new MailMessage
            {
                Subject = "Учетная запись эксперта для сайта ДТТ",
                Body = $"<html><head><title>Добрый день, {expert.FirstName} {expert.Patronymic}</title></head>" +
                       $"<body><p>Для вас была создана учетная запись на сайте ДТТ для проверки работ добровольного тренировочного тестирования.</p>" +
                       $"<p>Для входа используйте:</p>" +
                       $"<p><b>Логин (Email): </b> {expert.Email}</p>" +
                       $"<p><b>Пароль: </b> {expert.Email.Substring(0, 3)}@Expert</p>" +
                       $"<p>Чтобы приступить к проверке работ, перейдите по ссылке: <a href=\"" + callbackUrl + "\">приступить к проверке</a></p></body>" +
                       $"<footer>Пожалуйста, не отвечайте на это письмо.</footer></html>",
                IsBodyHtml = true
            };

            message.From=new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ");
            message.To.Add(new MailAddress(expert.Email, $"{expert.FirstName} {expert.Patronymic}"));
            await SendMail(message);
        }

        public async Task CreateTeacher(Person expert, string callbackUrl)
        {
            MailMessage message = new MailMessage
            {
                Subject = "Учетная запись преподавателя для сайта ДТТ",
                Body = $"<html><head><title>Добрый день, {expert.FirstName} {expert.Patronymic}</title></head>" +
                       $"<body><p>Для вас была создана учетная запись преподавателя на сайте ДТТ.</p>" +
                       $"<p>Для входа используйте:</p>" +
                       $"<p><b>Логин (Email): </b> {expert.Email}</p>" +
                       $"<p><b>Пароль: </b> {expert.Email.Substring(0, 3)}@Teacher</p>" +
                       $"<p>Чтобы войти в личный кабинет, перейдите по ссылке: <a href=\"" + callbackUrl + "\">перейти</a></p></body>" +
                       $"<footer>Пожалуйста, не отвечайте на это письмо.</footer></html>",
                IsBodyHtml = true
            };

            message.From = new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ");
            message.To.Add(new MailAddress(expert.Email, $"{expert.FirstName} {expert.Patronymic}"));
            await SendMail(message);
        }

        public async Task CreateNewOrderMailMessage(string sendTo, Order order, int examType, string orderPath, string receiptPath)
        {
            using (MailMessage message = new MailMessage
            {
                Subject = $"Создан новый договор",
                Body = $"<html><head><title>Договор №{order.OrderNumber}</title></head>" +
                       $"<body><h4>Сформирован новый договор №{order.OrderNumber}</h4>" +
                       $"<p>Во вложениях прикреплены документ договора и реквизиты для оплаты услуги по договору.</p>" +
                       $"<p>После оплаты договора необходимо прикрепить файл (изображение, скриншот, скан, ...), подтверждающий оплату.</p>" +
                       $"<p>В разделе 'Договоры' необходимо выбрать оплаченный договор из списка, развернуть подробную информацию, на вкладке 'Документы' выбрать или " +
                       $"перетащить файл, подтверждающий оплату.</p>" +
                       $"<p>Следить за актуальной информацией по исполнению настоящего договора Вы можете в личном кабинете в разделе ДОГОВОРЫ.</p>" +
                       $"</body>" +
                       $"<footer>Пожалуйста, не отвечайте на это письмо.</footer></html>",
                IsBodyHtml = true,
                //Attachments = {orderPdf, receiptPdf},
                From = new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ")
            })
            {
                //в зависимости от типа экзамена (очка - 1, полудистант -2, дистант - 3, тест - 4) выбирается нужный репорт для договора
                switch (examType)
                {
                    case 1:
                    {
                        using (var orderReport = new OrderDttOchka() { DataSource = order })
                        {
                            orderReport.ExportToPdf(orderPath);
                        }
                        message.Attachments.Add(new Attachment(orderPath));
                        break;
                    }
                    case 3:
                    {
                        using (var orderReport = new OrderDtt() { DataSource = order })
                        {
                            orderReport.ExportToPdf(orderPath);
                        }
                        message.Attachments.Add(new Attachment(orderPath));
                        break;
                    }
                }

                using (var receiptReport = new ReceiptDtt() { DataSource = order })
                {
                    receiptReport.ExportToPdf(receiptPath);
                }
                message.Attachments.Add(new Attachment(receiptPath));

                message.To.Add(new MailAddress(sendTo, "Дорогой друг"));
                await SendMail(message);
            }

            //System.IO.File.Delete(orderPath);
            //System.IO.File.Delete(receiptPath);
        }

        public async Task CreateNewOrderKpcMailMessage(string sendTo, WebDtt.Models.Dto.OrderKpc order, string orderPath,
            List<string> receiptPaths, string momBlankPath, string reminderPath)
        {
            using (MailMessage message = new MailMessage
            {
                Subject = $"Создан новый договор",
                Body = $"<html><head><title>Договор №{order.OrderNumber}</title></head>" +
                       $"<body><h4>Сформирован новый договор №{order.OrderNumber}</h4>" +
                       $"<p>Во вложениях прикреплены следующие документы: договор, реквизиты для оплаты (если оплата в несколько этапов - на каждый этап отдельный файл с указанием суммы), памятка слушателям.</p>" +
                       $"<br><p><b>При оплате просим ОБЯЗАТЕЛЬНО указывать ФИО слушателя и номер договора.</b></p><br>" +
                       $"<p>После оплаты договора необходимо прикрепить файл с подтверждением произведенного платежа (изображение, скриншот, скан и т.д.). " +
                       $"Для этого в личном кабинете слушателя в разделе 'Договоры' необходимо выбрать соответствующий договор из списка, " +
                       $"развернуть подробную информацию (стрелочка слева от договора), в вкладке 'Оплата' выбрать или перетащить файл, " +
                       $"подтверждающий оплату.</p>" +
                       $"<p>По всем вопросам Вы можете обратиться по телефону 8-909-587-34-38.</p>" +
                       $"<br><p>С уважением,</p><p>СПбЦОКОиИТ</p><p>576-34-38</p>" +
                       $"</body>" +
                       $"<footer>Пожалуйста, не отвечайте на это письмо.</footer></html>",
                IsBodyHtml = true,
                //Attachments = {orderPdf, receiptPdf},
                From = new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ")
            })
            {
                using (var reminderBlank = new Reminder())
                {
                    reminderBlank.ExportToPdf(reminderPath);
                }
                message.Attachments.Add(new Attachment(reminderPath));

                using (var orderReport = new OrderKpc() { DataSource = order })
                {
                    orderReport.ExportToPdf(orderPath);
                }
                message.Attachments.Add(new Attachment(orderPath));

                var i = 0;
                foreach (var path in receiptPaths)
                {
                    var donateInfo = new Dto.ReceiptKpc()
                    {
                        OrderNumber = order.OrderNumber,
                        StudentPerson = order.StudentPerson,
                        DonateInfo = order.DonateInfos[i]
                    };
                    using (var receiptReport = new ReceiptKpc() { DataSource = donateInfo })
                    {
                        receiptReport.ExportToPdf(path);
                    }
                    message.Attachments.Add(new Attachment(path));
                    i++;
                }

                if (momBlankPath != null)
                {
                    using (var momBlankReport = new OptionalyOrderKpc() { DataSource = order })
                    {
                        momBlankReport.ExportToPdf(momBlankPath);
                    }
                    message.Attachments.Add(new Attachment(momBlankPath));
                }

                message.To.Add(new MailAddress(sendTo, "Дорогой друг"));
                await SendMail(message);
            }

                
        }

        public async Task ChangeStatusOrder(string sendTo, Order order)
        {
            //var status = Enum.GetName(typeof(OrderFlags), order.Flags);
            using (MailMessage message = new MailMessage
            {
                Subject = $"Изменение статуса договора",
                Body = $"<html><head><title>Договор №{order.OrderNumber}</title></head>" +
                       $"<body><h4>Изменен статус договора №{order.OrderNumber}</h4>" +
                       //$"<p>Новый статус договора №{order.OrderNumber} - {Enum.GetName(typeof(OrderFlags), order.Flags)}</p>" +
                       $"<p>Следить за актуальной информацией по исполнению настоящего договора Вы можете в личном кабинете в разделе ДОГОВОРЫ.</p>" +
                       $"</body>" +
                       $"<footer>Пожалуйста, не отвечайте на это письмо.</footer></html>",
                IsBodyHtml = true,
                //Attachments = {orderPdf, receiptPdf},
                From = new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ")
            })
            {
                
                message.To.Add(new MailAddress(sendTo, "Дорогой друг"));
                await SendMail(message);
            }
        }

        public async Task SendKimAfterTestToStudent(Order order, User user, Person student, string pathToKIM)
        {
            using (MailMessage message = new MailMessage
            {
                Subject = $"Материалы тренировочного тестирования",
                Body = $"<html><head><title>КИМ</title></head>" +
                       $"<body><h4>Пройдено тренировочное тестирование в рамках договора №{order.OrderNumber}</h4>" +
                       $"<p>Участник: {student.LastName} {student.FirstName} {student.Patronymic}</p>" +
                       $"<p>Во вложении находятся материалы по пройденному экзамену.</p>" +
                       $"</body>" +
                       $"<footer>Пожалуйста, не отвечайте на это письмо.</footer></html>",
                IsBodyHtml = true,
                //Attachments = {orderPdf, receiptPdf},
                From = new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ")
            })
            {
                message.Attachments.Add(new Attachment(pathToKIM));
                message.To.Add(new MailAddress(user.Email, "Дорогой друг"));
                await SendMail(message);
            }
        }

        public async Task SendLinkToBroadCast(Order order, User user, Person student, Exam exam, ExamAddon link)
        {
            using (MailMessage message = new MailMessage
            {
                Subject = $"Вебинар по предмету {exam.Subject.SubjectName}",
                Body = $"<html><head><title>Вебинар</title></head>" +
                       $"<body><h4>Пройдено тренировочное тестирование в рамках договора №{order.OrderNumber}</h4>" +
                       $"<p>Экзамен: {exam.ExamViewSubjectDate}</p>" +
                       $"<p>Участник: {student.LastName} {student.FirstName} {student.Patronymic}</p>" +
                       $"<p>Дата начала вебинара: {link.StartDate.Value.Date}</p>" +
                       $"<p>Время начала вебинара: {link.StartTime.Value.Hours}:{link.StartTime.Value.Minutes}</p>" +
                       $"<p>Ссылка на трансляцию: <a href=\"" + link.LinkAddress + "\">перейти к трансляции</a></p>" +
                       $"</body>" +
                       $"<footer>Пожалуйста, не отвечайте на это письмо.</footer></html>",
                IsBodyHtml = true,
                //Attachments = {orderPdf, receiptPdf},
                From = new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ")
            })
            {
                message.To.Add(new MailAddress(user.Email, "Дорогой друг"));
                await SendMail(message);
            }
        }

        public async Task SendResultIsTrue(Order order, User user, Person student, Exam exam)
        {
            using (MailMessage message = new MailMessage
            {
                Subject = $"Доступен результат тренировочного тестирования по предмету {exam.Subject.SubjectName}",
                Body = $"<html><head><title>Результат тестирования</title></head>" +
                       $"<body><h4>Пройдено тренировочное тестирование в рамках договора №{order.OrderNumber}</h4>" +
                       $"<p>Экзамен: {exam.ExamViewSubjectDate}</p>" +
                       $"<p>Участник: {student.LastName} {student.FirstName} {student.Patronymic}</p>" +
                       $"<p>Результат доступен в разделе Договоры</p>" +
                       $"</body>" +
                       $"<footer>Пожалуйста, не отвечайте на это письмо.</footer></html>",
                IsBodyHtml = true,
                From = new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ")
            })
            {
                message.To.Add(new MailAddress(user.Email, "Дорогой друг"));
                await SendMail(message);
            }
        }

        public async Task SendPassOfExamToStudentExam(Order order, User user, Person student, Exam exam, StationExam stationExam)
        {
            using (MailMessage message = new MailMessage
            {
                Subject = $"Назначен пункт проведения ДТТ по предмету {exam.Subject.SubjectName}",
                Body = $"<html><head><title>Пункт проведения ДТТ</title></head>" +
                       $"<body><h3>В рамках договора №{order.OrderNumber} был присвоен пункт проведения ДТТ</h3>" +
                       $"<p><b>Участник:</b> {student.LastName} {student.FirstName} {student.Patronymic}</p>" +
                       $"<p><b>Предмет:</b> {exam.Subject.SubjectName}</p>" +
                       $"<p><b>Дата проведения:</b> {exam.TestDateTime.ToShortDateString()}</p>" +
                       $"<p><b>Время начала экзамена:</b> {stationExam.ExamStartupTime}</p>" +
                       $"<p><b>ППЭ:</b> {stationExam.Station.FullName}</p>" +
                       $"<p><b>Адрес:</b> {stationExam.Station.StationAddress}</p>" +
                       $"<p>ДТТ в очной форме проводится по форме и процедуре, максимально приближенной к процедуре и форме " +
                       $"Государственной итоговой аттестации.</p>" +
                       $"<p>Задания выполняются на бланках, идентичных бланкам ГИА, до начала работы проводится инструктаж.</p>" +
                       $"<p>При прохождении ДТТ в очной форме участник ДТТ может быть удален с тестирования при нарушении " +
                       $"им правил поведения во время написания тестирования (наличие посторонних предметов и материалов, " +
                       $"средств связи, электронных устройств и т.д.), о чем делается соответствующая отметка в регистрационном бланке.</p>" +
                       $"<p>Просим явиться в пункт проведения ДТТ за 15-20 минут до начала тестирования. При себе необходимо иметь маску!</p>" +
                       $"</body>" +
                       $"<br>" +
                       $"<footer>Пожалуйста, не отвечайте на это письмо.</footer></html>",
                IsBodyHtml = true,
                From = new MailAddress(Settings.Default.mailUserName, "СПб ЦОКОиИТ")
            })
            {
                message.To.Add(new MailAddress(user.Email, "Дорогой друг"));
                await SendMail(message);
            }
        }

        public async Task SendManagerMessage(MailMessage message)
        {
            await SendMail(message);
        }

        private async Task SendMail(MailMessage message)
        {
            // настройка логина, пароля отправителя
            

            // адрес и порт smtp-сервера, с которого мы и будем отправлять письмо
            var client = new SmtpClient("smtp.yandex.ru", 587)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(Settings.Default.mailUserName, Settings.Default.mailPasswd),
                EnableSsl = true
            };
            client.SendCompleted += Client_SendCompleted;
            await client.SendMailAsync(message);
        }

        private void Client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                throw e.Error;
            }
        }
    }
}