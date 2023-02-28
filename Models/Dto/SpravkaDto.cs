using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using DevExpress.Utils.Extensions;
using WebDtt.Models.Extentions;

namespace WebDtt.Models.Dto
{
    public class DonatesString
    {
        public string Name { get; set; } = "Сумма, уплаченная за обучение/Период обучения, за который внесена плата";
        public string Field { get; set; }
    }
    public class SpravkaDto
    {
        public string OrderNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatorFio { get; set; }
        public string InnNumber { get; set; }
        public string Document { get; set; }
        public string StudentFio { get; set; }
        public string EducationType { get; set; }

        //стоимость обучения
        public string TotalPrice { get; set; }
        public List<DonatesString> DonatesGroups { get; set; }
        public string Comments { get; set; }


        public SpravkaDto(Order order, Person creator, Person student, PersonGroup pg, Group group, List<DonateStatu> donates, string edType, bool vozvrat, List<Lesson> lessons)
        {
            CreatorFio =
                $"{StringMethods.RemSpCh(creator.LastName)} " +
                $"{StringMethods.RemSpCh(creator.FirstName)} " +
                $"{StringMethods.RemSpCh(creator.Patronymic)}";
            Document =
                $"Паспорт серия {creator.DocSeria} номер {creator.DocNumber}, " +
                $"выдан {creator.IssedDate.CastTo<DateTime>().ToShortDateString()} г., {creator.IssuedBy}";
            StudentFio =
                $"{StringMethods.RemSpCh(student.LastName)} " +
                $"{StringMethods.RemSpCh(student.FirstName)} " +
                $"{StringMethods.RemSpCh(student.Patronymic)}";
            EducationType = edType;
            OrderNumber = $"{order.OrderID}/{group.GroupName} от {order.CreateDate.ToShortDateString()} г.";
            var price = order.Discount > 0
                ? order.DonateType.FullTotal - (order.DonateType.FullTotal * order.Discount / 100)
                : order.DonateType.FullTotal;
            TotalPrice = $"{price} ({RusNumber.RusSpelledOut(price, true)}) руб. 00 коп.";
            var priceForLesson = price / lessons.Count;
            DonatesGroups=new List<DonatesString>();
            var firstLessonYear = lessons.First().LessonDate.Year;
            var lastLessonYear = lessons.Last().LessonDate.Year;
            if (firstLessonYear != lastLessonYear)
            {
                var firstDonate = donates.Where(d => d.DonateDate.Month >= 9).Sum(d => d.Total);
                var field1 = firstDonate > 0
                    ? $"{firstDonate} ({RusNumber.RusSpelledOut(firstDonate, true)}) руб. 00 коп. \n" +
                      $"с {lessons.First().LessonDate.ToShortDateString()} г. " +
                      $"по 31.12.{lessons.First().LessonDate.Year} г."
                    : $"- \n" +
                      $"с {lessons.First().LessonDate.ToShortDateString()} г. " +
                      $"по 31.12.{lessons.First().LessonDate.Year} г.";
                var secondDonate = donates.Where(d => d.DonateDate.Month <= 6).Sum(d => d.Total);
                var field2 = secondDonate > 0
                    ? $"{secondDonate} ({RusNumber.RusSpelledOut(secondDonate, true)}) руб. 00 коп. \n" +
                      $"с 01.01.{lessons.Last().LessonDate.Year} г. " +
                      $"по {lessons.Last().LessonDate.ToShortDateString()} г."
                    : $"- \n" +
                      $"с 01.01.{lessons.Last().LessonDate.Year} г. " +
                      $"по {lessons.Last().LessonDate.ToShortDateString()}";
                DonatesGroups.Add(new DonatesString()
                {
                    Field = field1
                });
                DonatesGroups.Add(new DonatesString()
                {
                    Field = field2
                });
            }
            else
            {
                var donatesSum = donates.Sum(d => d.Total);
                var field = donatesSum > 0
                    ? $"{donatesSum} ({RusNumber.RusSpelledOut(donatesSum, true)}) руб. 00 коп. \n" +
                      $"с {lessons.First().LessonDate.ToShortDateString()} г. " +
                      $"по {lessons.Last().LessonDate.ToShortDateString()} г."
                    : $"- \n" +
                      $"с {lessons.First().LessonDate.ToShortDateString()} г. " +
                      $"по {lessons.Last().LessonDate.ToShortDateString()} г.";
                DonatesGroups.Add(new DonatesString()
                {
                    Field = field
                });
            }
            

            Comments = "";
            if ((order.Flags & 128) != 0) Comments += "Договор удален! ";
            if (vozvrat) Comments += "Был возврат! ";
            if (!(string.IsNullOrEmpty(pg.Comment) || pg.Comment == " "))
                Comments += $"Есть комментарий: '{pg.Comment}'! ";
            if (order.CreateDate > lessons.First().LessonDate)
                Comments += $"Занятия начались раньше, чем был оформлен договор! ";


        }
        
        


    }

}