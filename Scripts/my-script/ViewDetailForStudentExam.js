var startTest = function (e) {
    var msDay = 60 * 60 * 24 * 1000;
    var dateTime = e.component.option("startDateTime").TestDateTime;
    var now = new Date();
    var deltaTime = ((dateTime - now) / msDay);
    var d = new Time
    if (deltaTime > 0) {
        alert("Доступ к тесту откроется в " + dateTime);
    }
    else {
        //ссылка на тест
        //window.open("/Order/ReceiptDtt/" + e.component._options._optionManager._options.id, "_blank");
    }
}

var initializedForm = function() {
    var form = $("#dxForm").dxForm("instance");
    form.option("formData", $('#customInput').data("value"));
}