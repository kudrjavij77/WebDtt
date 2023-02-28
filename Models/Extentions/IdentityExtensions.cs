using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using DevExpress.CodeParser;
using DevExpress.Utils.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using WebDtt.Models.CustomTypes;

namespace WebDtt.Models
{
    //public partial class User : IUser
    //{
    //    public string Id { get; }
    //}

    //public class ApplicationManager : UserManager<User>
    //{
    //    public ApplicationManager(IUserStore<User> store) : base(store)
    //    {
    //    }
    //    public static ApplicationManager Create(IdentityFactoryOptions<ApplicationManager> options,
    //        IOwinContext context)
    //    {
    //        AISExam_testingEntities db = context.Get<AISExam_testingEntities>();
    //        ApplicationManager manager = new ApplicationManager(new UserStore<>(db));
    //        return manager;
    //    }
    //}

    public partial class Order
    {
        public bool MomBank { get; set; }
        public string OrderNumber
        {
            get
            {
                switch (OrderTypeID)
                {
                    case 1:
                        return (OrderType != null)
                            ? $"{OrderID}/{OrderType.OrderTypeName}{CreateDate.Year}"
                            : OrderID.ToString();
                    case 2:
                        return (Group != null)
                            ? $"{OrderID}/{Group.GroupName}"
                            : OrderID.ToString();
                    default: 
                        return null;
                }
            }
        }

        public int LevelExam { get; set; }
        public int ExamType { get; set; }
        public int[] Exams { get; set; }
        public int? GroupID { get; set; }
        public virtual Group Group => PersonGroup != null ? PersonGroup.Group : null;
        public int? EducationFormID { get; set; }
        public int? SubjectID { get; set; }

        public virtual PersonGroup PersonGroup => PersonGroups?.FirstOrDefault();
    }

    public partial class Subject
    {
        public string SubjectViewName => $"{SubjectCode} - {SubjectName}";
    }

    public partial class Exam
    {
        public string ExamViewNameWithoutClass => Subject != null ? $"{Subject.SubjectViewName} - {TestDateTime.ToShortDateString()}" : null;

        public string ExamViewSubjectTypeClassDate
        {
            get
            {
                if (Subject != null)
                    return ExamType != null
                        ? $"{Subject.SubjectCode} - {Subject.ShortSubjectName} - {ExamType.ExamTypeName} - {Class} класс - {TestDateTime.ToShortDateString()}"
                        : null;
                return null;
            }
        }

        public string ExamViewSubjectDate
        {
            get
            {
                if (Subject != null) return $"{Subject.SubjectName} - {TestDateTime.ToShortDateString()}";
                return null;
            }
        }
    }

    public partial class ExamType
    {
        public string ViewName
        {
            get { return $"{ExamTypeName} ({ExamTypeDescription})"; }
        }
    }

    public partial class Station
    {
        public string StationViewName => $"{StationCode} - {StationName}";
        
    }

    public partial class Group
    {
        public List<Person> Teachers { get; set; }
        public List<Person> Students { get; set; }
        public byte Class { get; set; }
        public int Subject { get; set; }
    }

    public partial class Person
    {
        //
        //public int[] ExpertSubjects { get; set; }
        public string ViewClass
        {
            get
            {
                string _class = null;
                if (ParticipantClass != null)
                {
                    switch (ParticipantClass)
                    {
                        case 9:
                        {
                            _class = $"ОГЭ({ParticipantClass} класс)";
                            break;
                        }
                        case 11:
                        {
                            _class = $"ЕГЭ({ParticipantClass} класс)";
                            break;
                        }
                        default:
                        {
                            _class = $"{ParticipantClass} класс";
                            break;
                        }
                    }
                }
                else _class = "";

                return _class;
            }
            set => throw new NotImplementedException();
        }

        public string ViewFio => (Patronymic != null)
            ? $"{LastName} {FirstName} {Patronymic}"
            : $"{LastName} {FirstName}";

        public string ViewShortFio => (Patronymic != null)
            ? $"{LastName} {FirstName.Substring(0, 1).ToUpper()}.{Patronymic.Substring(0, 1).ToUpper()}."
            : $"{LastName} {FirstName.Substring(0, 1).ToUpper()}.";

        public string ViewFioTypeClass
        {
            get
            {
                if(PersonType!=null)
                return (ParticipantClass != null)
                    ? $"{LastName} {FirstName} {Patronymic} - {PersonType.PersonTypeDescription} - {ViewClass}"
                    : $"{LastName} {FirstName} {Patronymic} - {PersonType.PersonTypeDescription}";
                return (ParticipantClass != null)
                    ? $"{LastName} {FirstName} {Patronymic} - {ViewClass}"
                    : $"{LastName} {FirstName} {Patronymic}";
            }
        }
        //public string ViewFioType => $"{LastName} {FirstName} {Patronymic} - {PersonType.PersonTypeDescription}";

    }


    public partial class DonateStatu
    {
        public string FileName { get; set; }
    }

    
    
    public static class IdentityExtensions
    {
        public static T GetUserId<T>(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var ci = identity as ClaimsIdentity;
            if (ci != null)
            {
                var id = ci.FindFirst(ClaimTypes.NameIdentifier);
                if (id != null)
                {
                    return (T)Convert.ChangeType(id.Value, typeof(T), CultureInfo.InvariantCulture);
                }
            }
            return default(T);
        }
        public static string GetUserRole(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var ci = identity as ClaimsIdentity;
            string role = "";
            if (ci != null)
            {
                var id = ci.FindFirst(ClaimsIdentity.DefaultRoleClaimType);
                if (id != null)
                    role = id.Value;
            }
            return role;
        }

        
    }
}