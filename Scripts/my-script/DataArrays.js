var sexArrayLookup = [
    { id: 1, name: "Мужской" },
    { id: 2, name: "Женский" }
];

var classArrayLookup = [
    { id: 9, name: "ОГЭ (9 класс)" },
    { id: 11, name: "ЕГЭ (11 класс)" }
];

var personFlags = [
    {id: 0, name: "Активен"},
    {id: 128, name: "Удален"}
];

var orderFlags = [
    { name: "Ожидается оплата", id: 0 },
    { name: "Чек загружен", id: 1 },
    { name: "Оплата подтверждена", id: 2 },
    { name: "Возврат средств", id: 4 },
    { name: "Исполнен", id: 8 },
    { name: "Оплата мат. капитал", id: 16 },
    { name: "В ожидании", id: 32},
    { name: "Удален", id: 128 }
];

var studentExamFlags = [
    { name: "В обработке", id: 0 },
    { name: "Готов к рассадке", id: 1 },
    { name: "Доступен к прохождению согласно расписанию", id: 2 },
    { name: "Возврат средств", id: 4 },
    { name: "Материалы отправлены на почту", id: 8 },
    { name: "Тест завершен", id: 16 },
    { name: "Результат доступен", id: 32 },
    { name: "Ссылка на вебинар отправлена на почту", id: 64 },
    { name: "Удален", id: 128 },
    { name: "Оповещение о ППЭ отправлено на почту", id: 256 }
];

var studentExamCheckState = [
    { name: "Доступно к выдаче", id: 0 },
    { name: "Выдано эксперту на проверку", id: 1 },
    { name: "Работа проверена", id: 2 }
];

var bRateFlags = [
    { name: "Не проверен", id: 0},
    { name: "Проверен", id: 1 },
    { name: "Загружен", id: 4 }
];

var bAnswerFlags = [
    { name: "Создан", id: 0 },
    //Отверифицирован
    { name: "Загружен", id: 1 }
];

var cRateFlags = [
    { name: "Не проверен", id: 0 },
    { name: "Проверен", id: 1 }
];

var messageFlags = [
    { name: "Аноним", id: 0 },
    { name: "Пользователь", id: 1 },
    { name: "Оператор", id: 2 }
];

var examFlags = [
    { name: "Проводится", id: 0 },
    { name: "Регистрация закончена", id: 1},
    { name: "Удален(проведен)", id: 128 }
];

var newsFlags = [
    { name: "Создана", id: 0 },
    { name: "Удалена", id: 128}
];

var groupsFlags = [
    { name: "Создана", id: 0 },
    { name: "Набор окончен", id: 1 },
    { name: "Обучение завершено", id: 64},
    { name: "Удалена", id: 128 }
];

var examTypeDescription = [
    { name: "Проводится очно в пункте проведения экзамена согласно порядку проведения ГИА.", id: 1 },
    { name: "Проводится дистанционно (на дому). Бланки печатаются(отправляются на Email участника) в РЦОИ. " +
        "После прохождения тестирования участник возвращает заполненные бланки в РЦОИ для дальнейшей обработки.", id: 2 },
    { name: "Проводится дистанционно (на дому) полностью в электронной форме. " +
        "Часть 1 в форме теста в личном кабинете. Часть 2 - записывается на бумажный носитель. " +
        "Изображения части 2 прикрепляются к экзамену в личном кабинете.", id: 3 },
    { name: "Проводится дистанционно (на дому). Тестирование только в форме теста.", id: 4 }
];

var examAddonFlags = [
    { name: "Ссылка на консультацию", id: 1 },
    //{ name: "КИМ для участника", id: 2 },
    { name: "Удален", id: 128 }
];

var personGroupFlags = [
    { name: "Место в группе зарезервировано", id: 0 },
    { name: "Оплата мат. капитал", id: 1 },
    //{ name: "Отчислен", id: 2 },
    { name: "Отчислен", id: 4 },
    { name: "В ожидании места", id: 32 },
    { name: "Удален", id: 128 }
];

var donateStatusFlags = [
    //{ name: "необходимо прикрепить файл", id: 0 },
    { name: "отправлен на подтверждение", id: 1 },
    { name: "подтвержден", id: 2 },
    { name: "отклонен", id: 4 },
    { name: "удален", id: 128 }
];

var lessonFlags = [
    { name: "проводится по расписанию", id: 0 },
    { name: "отменилось", id: 1},
    { name: "удалено", id: 128 }
];

var topicFlags = [
    { name: "активна", id: 0 },
    { name: "удалена", id: 128 }
];

var journalFieldFlags = [
    { name: "видят все", id: 0 },
    { name: "не видит слушатель", id: 1 },
    { name: "для печатного журнала", id: 2},
    { name: "удалено", id: 128 }
];

var objectAnketFlags = [
    { name: "Оповощение отправлено на почту", id: 1 },
    { name: "Удален", id: 128 }
];

var iniButton = function(e) {

}


function flags2array(flags) {
    var arr = [];
    if (flags) {
        flags = parseInt(flags);
        for (var i = 1; i <= flags; i = i * 2) {
            if ((flags & i) > 0) {
                arr.push(i);
            }
        }
    }
    return arr;
};

function array2flags(arr) {
    if (!arr || !arr.length) {
        return 0;
    }
    var result = 0;
    for (var i = 0; i < arr.length; i++) {
        result += parseInt(arr[i]);
    }
    return result;
};

var displayFlags = function (e) {
    var arrFlags = flags2array(e.Flags);
    return arrFlags;
}