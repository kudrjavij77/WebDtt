using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDtt.Models.Dto
{
    public class PrintJournalDto
    {
        public string CurriculumName { get; set; }
        public string GroupName { get; set; }
        public int GroupId { get; set; }
        
        //count of hours
        public int Duration { get; set; }
        public List<string> Teachers { get; set; }

        //education year Example: 2020/2021
        public string EduYear { get; set; }
        public List<LessonOfPrintJournalDto> Lessons { get; set; }

    }

    
    public class LessonOfPrintJournalDto
    {
        public int LessonId { get; set; }
        public DateTime LessonDateTime { get; set; }
        public List<Topic> Topics { get; set; }
        public List<LessonJournalField> Fields { get; set; }
        public List<StudentMark> StudentMarks { get; set; }
    }


    public class StudentMark
    {
        public int StudentId { get; set; }
        public int PersonGroupId { get; set; }
        public int JournalFieldId { get; set; }
        public string StudentFio { get; set; }
        public string MarkName { get; set; }
        public int MarkValue { get; set; }
    }


    public class LessonJournalField
    {
        public int JournalFieldId { get; set; }
        public string JournalFieldName { get; set; }
    }
}