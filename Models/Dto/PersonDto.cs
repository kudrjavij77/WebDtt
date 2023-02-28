using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;
using DevExtreme.AspNet.Mvc;

namespace WebDtt.Models.Dto
{
    public class StudentViewModel
    {
        public int PersonID { get; set; }
        public Guid? PersonGUID { get; set; }
        public int PersonTypeID { get; set; }
        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }
        [Display(Name = "Отчество")]
        public string Patronymic { get; set; }
        [Required]
        [Display(Name = "Серия документа")]
        public string DocSeria { get; set; }
        [Required]
        [Display(Name = "Номер документа")]
        public string DocNumber { get; set; }
        [Required]
        [Display(Name = "Кем выдан")]
        public string IssuedBy { get; set; }
        [Required]
        [Display(Name = "Дата выдачи")]
        public DateTime IssedDate { get; set; }
        [Required]
        [Display(Name = "Дата рождения")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Пол")]
        public byte? Sex { get; set; }
        [Required(ErrorMessage = "Отсутствует поле Номер телефона")]
        [Display(Name = "Номер телефона")]
        [RegularExpression(@"^\+\s*7\s*\(\s*[02-9]\d{2}\)\s*\d{3}\s*-\s*\d{4}$", 
            ErrorMessage = "Телефонный номер нeкорректен")]
        public string Phones { get; set; }
        [Required]
        [EmailAddress]
        [RegularExpression(@"^[\d\w\._\-]+@([\d\w\._\-]+\.)+[\w]+$", ErrorMessage = "Некорректный Email")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Адрес регистрации")]
        public string RegistrationAddress { get; set; }
        [Required]
        [Display(Name = "Класс")]
        public byte? ParticipantClass { get; set; }
        public Guid? UserID { get; set; }
        public int? ParentPersonID { get; set; }
        public int Flags { get; set; }
        //public DateTime CreateDate { get; set; }
        //public DateTime? UpdateDate { get; set; }
        [Display(Name = "Псевдоним")]
        public string NickName { get; set; }
        public string ViewFio => $"{LastName} {FirstName} {Patronymic}";
        [DevExtremeRequired]
        [Display(Name = "Подтвердить")]
        public bool ConfirmedData { get; set; }
        [DevExtremeRequired]
        [Display(Name = "Подтвердить")]
        public bool ConfirmedPolicy { get; set; }
        [Display(Name = "Удален")]
        public bool IsDeleted { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Order> Orders1 { get; set; }
        public virtual ICollection<Person> Persons1 { get; set; }
        public Person Person1 { get; set; }
        public PersonType PersonType { get; set; }
        public User User { get; set; }
        public ICollection<StudentExam> StudentExams { get; set; }

        public StudentViewModel()
        {
            PersonTypeID = 1;
        }
        public StudentViewModel(Person person)
        {
            UserID = person.UserID;
            PersonID = person.PersonID;
            PersonTypeID = person.PersonTypeID;
            LastName = person.LastName;
            FirstName = person.FirstName;
            Patronymic = person.Patronymic;
            if (person.BirthDate != null) BirthDate = (DateTime) person.BirthDate;
            Sex = person.Sex;
            ParticipantClass = person.ParticipantClass;
            DocSeria = person.DocSeria;
            DocNumber = person.DocNumber;
            if (person.IssedDate != null) IssedDate = (DateTime) person.IssedDate;
            IssuedBy = person.IssuedBy;
            RegistrationAddress = person.RegistrationAddress;
            Phones = person.Phones;
            Email = person.Email;
            PersonType = person.PersonType;
            Person1 = person.Person1;
            User = person.User;
            Flags = person.Flags;
            ConfirmedData = ((Flags & 1) == 1);
            ConfirmedPolicy = ((Flags & 2) == 2);
            IsDeleted = (Flags & 128) == 128;
        }

        private void ValidateStudent(StudentViewModel st)
        {

        }

    }

    public class DelegateViewModel
    {
        public int PersonID { get; set; }
        public Guid? PersonGUID { get; set; }
        public int PersonTypeID { get; set; }
        [Required] [Display(Name = "Фамилия")] public string LastName { get; set; }
        [Required] [Display(Name = "Имя")] public string FirstName { get; set; }
        [Display(Name = "Отчество")] public string Patronymic { get; set; }

        [Required]
        [Display(Name = "Серия документа")]
        public string DocSeria { get; set; }

        [Required]
        [Display(Name = "Номер документа")]
        public string DocNumber { get; set; }

        [Required]
        [Display(Name = "Кем выдан")]
        public string IssuedBy { get; set; }

        [Required]
        [Display(Name = "Дата выдачи")]
        public DateTime IssedDate { get; set; }

        [Required]
        [Display(Name = "Дата рождения")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Пол")] public byte? Sex { get; set; }

        [Required(ErrorMessage = "Отсутствует поле Номер телефона")]
        [Display(Name = "Номер телефона")]
        [RegularExpression(@"^\+\s*7\s*\(\s*[02-9]\d{2}\)\s*\d{3}\s*-\s*\d{4}$",
            ErrorMessage = "Телефонный номер нeкорректен")]
        public string Phones { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[\d\w\._\-]+@([\d\w\._\-]+\.)+[\w]+$", ErrorMessage = "Некорректный Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Адрес регистрации")]
        public string RegistrationAddress { get; set; }

        //[Required] [Display(Name = "Класс")] public byte? ParticipantClass { get; set; }
        public Guid? UserID { get; set; }
        public int? ParentPersonID { get; set; }

        public int Flags { get; set; }

        //public DateTime CreateDate { get; set; }
        //public DateTime? UpdateDate { get; set; }
        [Display(Name = "Псевдоним")] public string NickName { get; set; }
        public string ViewFio => $"{LastName} {FirstName} {Patronymic}";

        [DevExtremeRequired]
        [Display(Name = "Подтвердить")]
        public bool ConfirmedData { get; set; }

        [DevExtremeRequired]
        [Display(Name = "Подтвердить")]
        public bool ConfirmedPolicy { get; set; }

        [Display(Name = "Удален")] public bool IsDeleted { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Order> Orders1 { get; set; }
        public virtual ICollection<Person> Persons1 { get; set; }
        public Person Person1 { get; set; }
        public PersonType PersonType { get; set; }
        public User User { get; set; }
        public ICollection<StudentExam> StudentExams { get; set; }

        public DelegateViewModel()
        {
            PersonTypeID = 3;
        }

        public DelegateViewModel(Person person)
        {
            UserID = person.UserID;
            PersonID = person.PersonID;
            PersonTypeID = person.PersonTypeID;
            LastName = person.LastName;
            FirstName = person.FirstName;
            Patronymic = person.Patronymic;
            if (person.BirthDate != null) BirthDate = (DateTime) person.BirthDate;
            Sex = person.Sex;
            //ParticipantClass = person.ParticipantClass;
            DocSeria = person.DocSeria;
            DocNumber = person.DocNumber;
            if (person.IssedDate != null) IssedDate = (DateTime) person.IssedDate;
            IssuedBy = person.IssuedBy;
            RegistrationAddress = person.RegistrationAddress;
            Phones = person.Phones;
            Email = person.Email;
            PersonType = person.PersonType;
            Person1 = person.Person1;
            User = person.User;
            Flags = person.Flags;
            ConfirmedData = ((Flags & 1) == 1);
            ConfirmedPolicy = ((Flags & 2) == 2);
            IsDeleted = (Flags & 128) == 128;
        }
    }


    public class ExpertViewModel
    {
        public int PersonID { get; set; }
        public Guid? PersonGUID { get; set; }
        public int PersonTypeID { get; set; }
        [Required] [Display(Name = "Фамилия")] public string LastName { get; set; }
        [Required] [Display(Name = "Имя")] public string FirstName { get; set; }
        [Display(Name = "Отчество")] public string Patronymic { get; set; }

        [Display(Name = "Серия документа")]
        public string DocSeria { get; set; }

        [Display(Name = "Номер документа")]
        public string DocNumber { get; set; }

        [Display(Name = "Кем выдан")]
        public string IssuedBy { get; set; }

        [Display(Name = "Дата выдачи")]
        public DateTime? IssedDate { get; set; }

        [Display(Name = "Дата рождения")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Пол")] public byte? Sex { get; set; }

        [Display(Name = "Номер телефона")]
        [RegularExpression(@"^\+\s*7\s*\(\s*[02-9]\d{2}\)\s*\d{3}\s*-\s*\d{4}$",
            ErrorMessage = "Телефонный номер нeкорректен")]
        public string Phones { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[\d\w\._\-]+@([\d\w\._\-]+\.)+[\w]+$", ErrorMessage = "Некорректный Email")]
        public string Email { get; set; }

        [Display(Name = "Адрес регистрации")]
        public string RegistrationAddress { get; set; }

        [Display(Name = "Класс")] 
        public byte? ParticipantClass { get; set; }
        public Guid? UserID { get; set; }
        
        public int Flags { get; set; }

        [Display(Name = "Псевдоним")] public string NickName { get; set; }
        public string ViewFio => $"{LastName} {FirstName} {Patronymic}";
            
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Order> Orders1 { get; set; }
        public virtual ICollection<Person> Persons1 { get; set; }

        [Required]
        [Display(Name = "Предметы, проверяемые экспертом")]
        public List<Subject> Subjects { get; set; }
        public Person Person1 { get; set; }
        public PersonType PersonType { get; set; }
        public User User { get; set; }
        public ICollection<StudentExam> StudentExams { get; set; }

        public ExpertViewModel()
        {
            PersonTypeID = 4;
        }

        public ExpertViewModel(Person person)
        {
            UserID = person.UserID;
            PersonID = person.PersonID;
            PersonTypeID = person.PersonTypeID;
            LastName = person.LastName;
            FirstName = person.FirstName;
            Patronymic = person.Patronymic;
            if (person.BirthDate != null) BirthDate = (DateTime)person.BirthDate;
            Sex = person.Sex;
            ParticipantClass = person.ParticipantClass;
            DocSeria = person.DocSeria;
            DocNumber = person.DocNumber;
            if (person.IssedDate != null) IssedDate = (DateTime)person.IssedDate;
            IssuedBy = person.IssuedBy;
            RegistrationAddress = person.RegistrationAddress;
            Phones = person.Phones;
            Email = person.Email;
            PersonType = person.PersonType;
            Person1 = person.Person1;
            User = person.User;
            Flags = person.Flags;
            Subjects = person.Subjects.ToList();
        }
    }



    public class TeacherViewModel
    {
        public int PersonID { get; set; }
        public Guid? PersonGUID { get; set; }
        public int PersonTypeID { get; set; }
        [Required] [Display(Name = "Фамилия")] public string LastName { get; set; }
        [Required] [Display(Name = "Имя")] public string FirstName { get; set; }
        [Display(Name = "Отчество")] public string Patronymic { get; set; }

        [Display(Name = "Серия документа")]
        public string DocSeria { get; set; }

        [Display(Name = "Номер документа")]
        public string DocNumber { get; set; }

        [Display(Name = "Кем выдан")]
        public string IssuedBy { get; set; }

        [Display(Name = "Дата выдачи")]
        public DateTime? IssedDate { get; set; }

        [Display(Name = "Дата рождения")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Пол")] public byte? Sex { get; set; }

        [Display(Name = "Номер телефона")]
        [RegularExpression(@"^\+\s*7\s*\(\s*[02-9]\d{2}\)\s*\d{3}\s*-\s*\d{4}$",
            ErrorMessage = "Телефонный номер нeкорректен")]
        public string Phones { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[\d\w\._\-]+@([\d\w\._\-]+\.)+[\w]+$", ErrorMessage = "Некорректный Email")]
        public string Email { get; set; }

        [Display(Name = "Адрес регистрации")]
        public string RegistrationAddress { get; set; }

        [Display(Name = "Класс")]
        public byte? ParticipantClass { get; set; }
        public Guid? UserID { get; set; }

        public int Flags { get; set; }

        [Display(Name = "Псевдоним")] public string NickName { get; set; }
        public string ViewFio => $"{LastName} {FirstName} {Patronymic}";

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Order> Orders1 { get; set; }
        public virtual ICollection<Person> Persons1 { get; set; }

        [Required]
        [Display(Name = "Предметы, проверяемые экспертом")]
        public List<Subject> Subjects { get; set; }
        public Person Person1 { get; set; }
        public PersonType PersonType { get; set; }
        public User User { get; set; }
        public ICollection<StudentExam> StudentExams { get; set; }

        public TeacherViewModel()
        {
            PersonTypeID = 2;
        }

        public TeacherViewModel(Person person)
        {
            UserID = person.UserID;
            PersonID = person.PersonID;
            PersonTypeID = person.PersonTypeID;
            LastName = person.LastName;
            FirstName = person.FirstName;
            Patronymic = person.Patronymic;
            if (person.BirthDate != null) BirthDate = (DateTime)person.BirthDate;
            Sex = person.Sex;
            ParticipantClass = person.ParticipantClass;
            DocSeria = person.DocSeria;
            DocNumber = person.DocNumber;
            if (person.IssedDate != null) IssedDate = (DateTime)person.IssedDate;
            IssuedBy = person.IssuedBy;
            RegistrationAddress = person.RegistrationAddress;
            Phones = person.Phones;
            Email = person.Email;
            PersonType = person.PersonType;
            Person1 = person.Person1;
            User = person.User;
            Flags = person.Flags;
            Subjects = person.Subjects.ToList();
        }
    }


}