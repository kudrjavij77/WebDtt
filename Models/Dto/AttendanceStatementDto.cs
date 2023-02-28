using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDtt.Models.Dto
{
    public class AttendanceStatementDto
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string LessonDate { get; set; }
        public string TeachersString { get; set; }
        public List<Person> Students { get; set; }

        public AttendanceStatementDto(Group group)
        {
            GroupID = group.GroupID;
            GroupName = group.GroupName;
            LessonDate = group.StartDateTime.ToShortDateString();
            Students = new List<Person>();
            foreach (var pg in group.PersonGroups)
            {
                if (pg.Person.PersonTypeID == 2)
                    TeachersString += pg.Person.ViewShortFio + " ";

                if (pg.OrderID != null && (pg.Flags & 4) != 4)
                    Students.Add(pg.Person);
            }


        }
    }
}