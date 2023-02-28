using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace WebDtt.Models.CustomTypes
{
    [Flags]
    public enum OrderFlags
    {
        [Description("Ожидается оплата")]
        PaymentIsExpected = 0,
        [Description("Чек загружен")]
        ReceiptIsLoaded = 1,
        [Description("Оплата подтверждена")]
        PaymentIsConfirmed = 2,
        [Description("Возврат средств")]
        Refund = 4,
        [Description("Исполнен")]
        Completed = 8,
        [Description("Удален")]
        IsDeleted = 128,
    }

    [Flags]
    public enum StudentExamFlags
    {
        [Description("Готов к рассадке")]
        ReadyForAttach = 1,
        [Description("Прикреплен к ППЭ")]
        Attached = 2,
        [Description("Возврат средств")]
        Refund = 4,
        [Description("Материалы отправлены на почту")]
        MaterialsIsSendToEmail = 8,
        [Description("Тест завершен")]
        TestIsCompleted = 16,
        [Description("Результат доступен")]
        FilesOfAnswersIsAttached = 32,
        [Description("Ссылка на вебинар отправлена на почту")]
        LinkToWebIsSendToEmail = 64,
        [Description("Удален")]
        IsDeleted = 128,
    }

    [Flags]
    public enum PersonFlags
    {
        [Description("Данные подтверждены")]
        ConfirmedData = 1,
        [Description("Согласен на обработку персональных данных")]
        ConfirmedPolicy = 2,
        [Description("Удален")]
        IsDeleted = 128,
    }

}