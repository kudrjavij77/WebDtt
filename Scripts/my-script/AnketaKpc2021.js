function formatLabel(value) { return value; }
function formatLabel31(value) { return value === 1 ? "практический материал" : "теоретический материал"; }
function formatLabel32(value) { return value === 1 ? "сложные" : "простые"; }
function formatLabel33(value) { return value === 1 ? "рассказывает сам" : "вовлекает в беседу"; }

function changed8(e) {
    var radioName = e.component.option('name');
    if (e.value.text === 'Неудобно') {

    }
}


function slider_valueChanged(data) {
    $("#slider-value").dxNumberBox("instance").option("value", data.value);
}

function numberBox_valueChanged(data) {
    $("#handler-slider").dxSlider("instance").option("value", data.value);
}

var radioArray1 = [
    { "text": "Отлично", "value": "5" },
    { "text": "Хорошо", "value": "4" },
    { "text": "Нормально", "value": "3" },
    { "text": "Плохо", "value": "2" },
    { "text": "Очень плохо", "value": "1" }
];

var radioArray2 = [
    { "text": "Удобно", "value": "4" },
    { "text": "Не принципиально", "value": "3" },
    { "text": "Неудобно", "value": "2" },
    { "text": "Не видел такого на сайте", "value": "1" }
];

var radioTask2 = [
    { "text": "Да", "value": "5" },
    { "text": "Да, но не подробно", "value": "4" },
    { "text": "Нет, преподаватель не отвечает на вопросы слушателей", "value": "3" },
    { "text": "Слушатели не задают вопросов", "value": "2" },
    { "text": "Преподаватель не выделяет времени на дополнительные вопросы", "value": "1" }
];

var radioTask91 = [
    { "text": "электронные курсы", "value": "1" },
    { "text": "мобильные приложения", "value": "2" },
    { "text": "обучающие платформы", "value": "4" },
    { "text": "репетитор", "value": "8" },
    { "text": "самообучение", "value": "16" },
    { "text": "дополнительные занятия в школе", "value": "32" },
    { "text": "нет", "value": "64" },
    { "text": "другое", "value": "128" }
];

var radioTask101 = [
    { "text": "структурированной информации (таблицы, схемы, конспекты)", "value": "1" },
    { "text": "игрового контента", "value": "2" },
    { "text": "возможности обсуждать сложные темы в группе", "value": "4" },
    { "text": "тренировочных тестирований в формате ЕГЭ", "value": "8" },
    { "text": "другое", "value": "16" }
];

var radioTask151 = [
    { "text": "посоветовал учитель в школе", "value": "1" },
    { "text": "узнал через других участников курсов", "value": "2" },
    { "text": "интернет", "value": "3" },
    { "text": "другое", "value": "10" }
];

var radioTask111 = [
    { "text": "да", "value": "1" },
    { "text": "нет", "value": "2" }
];




var rT91 = [
    { text: "электронные курсы", id: 1 },
    { text: "мобильные приложения", id: 2 },
    { text: "обучающие платформы", id: 4 },
    { text: "репетитор", id: 8 },
    { text: "самообучение", id: 16 },
    { text: "дополнительные занятия в школе", id: 32 },
    { text: "нет", id: 64 },
    { text: "другое", id: 128 }
];
var rT101 = [
    { text: "структурированной информации (таблицы, схемы, конспекты)", id: 1 },
    { text: "игрового контента", id: 2 },
    { text: "возможности обсуждать сложные темы в группе", id: 4 },
    { text: "тренировочных тестирований в формате ЕГЭ", id: 8 },
    { text: "другое", id: 16 }
];

var templateQ_9_1 = function (e, container) {
    $("<div>").dxList({
        dataSource: new DevExpress.data.DataSource({
            store: new DevExpress.data.ArrayStore({
                key: 'id',
                data: rT91
            })
        }),
        //height: 400,
        showSelectionControls: true,
        selectionMode: 'all',
        onSelectionChanged(e) {
            $('#form').dxForm('instance').updateData("Q_9_1", this.option('selectedItemKeys').join(', '));
        }
    }).appendTo(container);

}

var templateQ_10_1 = function (e, container) {
    $("<div>").dxList({
        dataSource: new DevExpress.data.DataSource({
            store: new DevExpress.data.ArrayStore({
                key: 'id',
                data: rT101
            })
        }),
        //height: 400,
        showSelectionControls: true,
        selectionMode: 'all',
        onSelectionChanged(e) {
            $('#form').dxForm('instance').updateData("Q_10_1", this.option('selectedItemKeys').join(', '));
        }
    }).appendTo(container);

}
