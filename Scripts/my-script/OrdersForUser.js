var addOrderDtt = function () {
    window.location.href = '/Order/NewUserOrderDtt';
}
var addOrderKpc = function() {
    window.location.href = '/Order/NewUserOrderKpc';
}

var viewReportDesigner = function (e) {
    window.open("/Order/ReportViewer/" + e.component._options._optionManager._options.id, "_blank");
}

var viewOrderDtt = function (e) {
    window.open("/Order/OrderDtt/" + e.component._options._optionManager._options.id, "_blank");
}

var viewReceiptDtt = function (e) {
    window.open("/Order/ReceiptDtt/" + e.component._options._optionManager._options.id, "_blank");
}

var viewBillDtt = function(e) {
    window.open("/Order/DownLoadBill/" + e.component._options._optionManager._options.id, "_blank");
}

var toDetailStudentExam = function(e) {
    window.open("/StudentExam/ViewDetail/" + e.component._options._optionManager._options.id, "_blank");
}

var onRowClick = function (e) {
    return e.data.OrderID;
}

var unVisibility = function(e) {
    if (e.component._options._optionManager._options.flags > 0) {
        e.component._options._optionManager._options.visible = false;
    }
}

var downLoadReceipt = function(e) {
    var orderId = e.row.cells[3].column.buttons[0].orderId;
    var donateInfoId = e.row.data.DonateInfoID;
    window.open("/Order/ReceiptKpc/?orderId=" + orderId + "&donateInfoId=" + donateInfoId, "_blank");
}

var downloadBillKpc = function(e) {
    window.open("/Order/DownLoadBillKpc/?orderId=" + e.row.data.OrderID + "&fileId=" + e.row.data.FileID, "_blank");
}

var reSendReports = function(e) {
    window.open("/Order/ReCreateAndReSendOrderReports/?orderId=" + e.row.data.OrderID);
}

var downloadOrderKPC = function (e) {
    window.open("/Order/OrderDtt/?id=" + e.row.data.OrderID, "_blank");
}

var downloadMomBlank = function (e) {
    window.open("/Order/DownloadMomBlank/?id=" + e.row.data.OrderID, "_blank");
}

var downloadSpravka = function(e) {
    window.open("/Order/DownloadSpravka/?id=" + e.row.data.OrderID, "_blank");
}

var vozvratKpc = function(e) {
    var g = $('#gridOrdersKpc').dxDataGrid('instance');
    g.beginUpdate();
    g.cellValue(e.row.rowIndex, "Flags", 4);
    g.saveEditData();
    g.relaod();
}

var gridKpcToolbar = function(e) {
    e.toolbarOptions.items.push({
        widget: "dxButton",
        options: {
            icon: "export",
            text: "Выгрузить в Excel",
            type: "normal",
            stylingMode: "outlined",
            onClick: function (e) {
                var workbook = new ExcelJS.Workbook();
                var worksheet = workbook.addWorksheet("Договоры");

                DevExpress.excelExporter.exportDataGrid({
                    component: $('#gridOrdersKpc').dxDataGrid('instance'),
                    worksheet: worksheet,
                    autoFilterEnabled: true
                }).then(function () {
                    workbook.xlsx.writeBuffer().then(function (buffer) {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Выгрузка договоров от ' + new Date().toLocaleString() + '.xlsx');
                    });
                });
                e.cancel = true;
            }
        },
        location: "after"
    });
}

var contentReadyDonateStatus = function(_, e, options) {

}

var getOrderID = function(e) {
    
}



//var titleTemplate = function(data, index, element, options) {
//    return data.icon + data.title.bold();
//}

function valueChangedUploaderKpc(e) {
    var key = e.element.attr('id');
    e.component.option('uploadUrl', '/Order/Upload?key=' + key);
    var dxGrid = $('#donateStatusGrid' + key).dxDataGrid('instance');
    dxGrid.refresh();
    //var orderGrid = $('#gridOrdersKpc').dxDataGrid('instance');
    //orderGrid.reload();
}


//UPLOADER
function fileUploader_valueChanged(e){
    var key = e.element.attr('id');
    e.component.option('uploadUrl', '/Order/Upload?key=' + key);
    var dxGrid = $('#Orders').dxDataGrid('instance');
    dxGrid.refresh();
    //var files = e.value;
    //if (files.length > 0) {
    //    $("#selected-files .selected-item").remove();

    //    $.each(files, function (i, file) {
    //        var $selectedItem = $("<div />").addClass("selected-item");
    //        $selectedItem.append(
    //            $("<span />").html("Название: " + file.name + "<br/>"),
    //            $("<span />").html("Размер " + file.size + " bytes" + "<br/>")
    //        );
    //        $selectedItem.appendTo($("#selected-files"));
    //    });
    //    $("#selected-files").show();
    //}
    //else
    //    $("#selected-files").hide();
}
//


//$(function () {

//        var url = "/Api/Lookups/GetDonateBarData";
//    $("chart" + tabExtras.orderID).dxChart({
//            rotated: true,
//            dataSource: DevExpress.data.AspNet.createStore({
//                loadUrl: url,
//                onBeforeSend: function(method, ajaxOptions) {
//                    if (method === "load") ajaxOptions.url = `${ajaxOptions.url}?orderId=${tabExtras.orderID}`;
//                }
//            }),
//            commonSeriesSettings: {
//                argumentField: "Info",
//                type: "fullStackedBar"
//            },
//            series: [
//                { valueField: "Green", name: "Подтверждено", color: "green" },
//                { valueField: "Yellow", name: "В обработке", color: "yellow" },
//                { valueField: "Red", name: "Неоплачено (истекает срок)", color: "red" },
//                { valueField: "Grey", name: "Неоплачено", color: "grey" }
//            ],
//            legend: {
//                verticalAlignment: "bottom",
//                horizontalAlignment: "center",
//                itemTextPosition: "right"
//            },
//            title: {
//                text: "Прогресс оплаты"
//            },
//            tooltip: {
//                enabled: true,
//                customizeTooltip: function (arg) {
//                    return {
//                        text: arg.percentText + " - " + arg.valueText
//                    };
//                }
//            }


//        });

//    }
//);

var updateDonateBar = function(e) {
    var bar = $("#chart" + e.element[0].id.substr(16)).dxChart('instance');
    bar.refresh();
}

var initDonateBar = function(e) {

}

//function getGroupInfoForm(e) {
//    var $div = $('<div>');
//    $.get(
//        "/Api/Groups/GetGroupByOrderId",
//        { orderId: e.tabExtras.orderID },
//        onAjaxSuccess
//    );

//    function onAjaxSuccess(data) {

//    }

//    return $div;
//}

function getStudentForm (e)
{
    
    var $div = $('<div>');
     $.get(
        "/Api/Persons/GetPersonById",
        { personId: e.tabExtras.order.StudentPersonID},
        onAjaxSuccess
    );

    function onAjaxSuccess(data) {
        var d = data.data[0];   
        var form = $("<div id='dx" + e.tabExtras.order.OrderID + "StudentForm" + d.PersonID + "'>")
            .addClass("address-form form-container")
            .dxForm({
                formData: d,
                colCount: 3,
                stylingMode: "filled",
                //readOnly: true,
                onFieldDataChanged: function (e) {
                    var formData = {};
                    formData.key = e.component.option('formData').PersonID;
                    formData.values = '{"'+e.dataField+'":"'+e.value+'"}';

                    $.ajax({
                        url: "/Api/Persons/Put",
                        type: 'PUT',
                        data: formData,
                        //dataType: 'application/x-www-form-urlencoded',
                        success: function (x3d) {
                        },
                        error: function (xhr) {

                        }
                    });

                },
                showColonAfterLabel: true,
                labelLocation: "top",
                items: [{
                    itemType: "group",
                    caption: "Общее",
                    items: [
                        { dataField: "LastName", label: { text: "Фамилия" } },
                        { dataField: "FirstName", label: { text: "Имя" } },
                        { dataField: "Patronymic", label: { text: "Отчество" } },
                        { dataField: "ParticipantClass", label: { text: "Класс" } },
                        {
                            dataField: "Sex", label: { text: "Пол" }, editorType: "dxLookup",
                            editorOptions: {
                                dataSource: new DevExpress.data.DataSource({ store: sexArrayLookup }),
                                displayExpr: "name", valueExpr: "id"
                            }                            
                        },
                        {
                            dataField: "BirthDate", label: { text: "Дата рождения" }, editorType: "dxDateBox",
                            editorOptions: { dateSerializationFormat: "dd/MMMM/yyyy" }
                        }]
                },
                {
                    itemType: "group",
                    caption: "Паспортные данные",
                    items: [
                        { dataField: "DocSeria", label: { text: "Серия" } },
                        { dataField: "DocNumber", label: { text: "Номер" } },
                        {
                            dataField: "IssuedBy", label: { text: "Кем выдан" }, editorType: "dxTextArea",
                            editorOptions: { height: 80 }
                        },
                        {
                            dataField: "IssedDate", label: { text: "Дата выдачи" }, editorType: "dxDateBox",
                            editorOptions: { dateSerializationFormat: "dd/MMMM/yyyy" }
                        },
                        {
                            dataField: "RegistrationAddress", label: { text: "Адрес регистрации" }, editorType: "dxTextArea",
                            editorOptions: { height: 80 }
                        }]
                },
                {
                    itemType: "group",
                    caption: "Контактные данные",
                    items: [{ dataField: "Phones", label: { text: "Телефон" } }, "Email"]
                }]
            });
        $div.append(form);
    }

    return $div;
   
}

function getDelegateForm(e) {

    var $div = $('<div>');
    $.get(
        "/Api/Persons/GetPersonById",
        { personId: e.tabExtras.order.CreatorPersonID },
        onAjaxSuccess
    );

    function onAjaxSuccess(data) {
        var d = data.data[0];
        var form = $("<div id='dx" + e.tabExtras.order.OrderID + "DelegateForm" + d.PersonID + "'>")
            .addClass("address-form form-container")
            .dxForm({
                formData: d,
                colCount: 3,
                stylingMode: "filled",
                //readOnly: true,
                onFieldDataChanged: function (e) {
                    var formData = {};
                    formData.key = e.component.option('formData').PersonID;
                    formData.values = '{"' + e.dataField + '":"' + e.value + '"}';

                    $.ajax({
                        url: "/Api/Persons/Put",
                        type: 'PUT',
                        data: formData,
                        //dataType: 'application/x-www-form-urlencoded',
                        success: function (x3d) {
                        },
                        error: function (xhr) {

                        }
                    });

                },
                showColonAfterLabel: true,
                labelLocation: "top",
                items: [{
                    itemType: "group",
                    caption: "Общее",
                    items: [
                        { dataField: "LastName", label: { text: "Фамилия" } },
                        { dataField: "FirstName", label: { text: "Имя" } },
                        { dataField: "Patronymic", label: { text: "Отчество" } },
                        {
                            dataField: "Sex", label: { text: "Пол" }, editorType: "dxLookup",
                            editorOptions: {
                                dataSource: new DevExpress.data.DataSource({ store: sexArrayLookup }),
                                displayExpr: "name", valueExpr: "id"
                            }
                        },
                        {
                            dataField: "BirthDate", label: { text: "Дата рождения" }, editorType: "dxDateBox",
                            editorOptions: { dateSerializationFormat: "dd/MMMM/yyyy" }
                        }]
                },
                {
                    itemType: "group",
                    caption: "Паспортные данные",
                    items: [
                        { dataField: "DocSeria", label: { text: "Серия" } },
                        { dataField: "DocNumber", label: { text: "Номер" } },
                        {
                            dataField: "IssuedBy", label: { text: "Кем выдан" }, editorType: "dxTextArea",
                            editorOptions: { height: 80 }
                        },
                        {
                            dataField: "IssedDate", label: { text: "Дата выдачи" }, editorType: "dxDateBox",
                            editorOptions: { dateSerializationFormat: "dd/MMMM/yyyy" }
                        },
                        {
                            dataField: "RegistrationAddress", label: { text: "Адрес регистрации" }, editorType: "dxTextArea",
                            editorOptions: { height: 80 }
                        }]
                },
                {
                    itemType: "group",
                    caption: "Контактные данные",
                    items: [{ dataField: "Phones", label: { text: "Телефон" } }, "Email"]
                }]
            });
        $div.append(form);
    }

    return $div;

}

var contentReadyStExamForm = function (e) {
    var form = e.component;
    var items = e.component.option("items");
    var data = e.component.option("formData");

    if (data.Flags === 4 | data.Flags === 128)
        return;

    if (data.Exam.ExamTypeID === 3) {
        e.component.option("visible", true);
        if (data.Flags < 16) {
            
        }
    }
}


function getForm(_, masterDetailOptions) {
    var data = masterDetailOptions.data;
    var today = new Date();
    var tdt;
    if (data.PersonTestDateTime != null) {
        tdt = new Date(data.PersonTestDateTime);
    } else {
        tdt = new Date(data.Exam.TestDateTime);
    }
    var tdt2 = new Date(tdt.valueOf() + 172799000);
    var fd = new Date(data.FinishDateTime);
    var fd1 = new Date(fd.valueOf() + 86399000);
    var div = $("<div>");
    var todayInDelta = today < tdt2 && today >= tdt;
    var electronicKim = data.ElectronicKIMID;
    if ((data.Flags & 128) === 128) {
        return div.html("Тест удален");
    }
    if ((data.Flags & 4) === 4) {
        return div.html("Оформлен возврат средств");
    }
    if (data.Flags === 1) {
        return div.html("Ожидается прикрепление к пункту проведения ДТТ");
    }
    
    //if (data.Order.Flags === 1) {
    //    return div.html("Ожидается подтверждение оплаты");
    //}

    if (data.Flags === 0) {
        return div.html("Ожидается оплата");
    }

    switch (data.Exam.ExamTypeID) {
    case 3:
    {
        if (data.FinishDateTime == null && today > tdt2 && data.ElectronicKIMID === null) {
            return div.html("Тест не был пройден");
        }

        if (data.FinishDateTime == null) {
            div.append($("<div>").dxButton({
                stylingMode: "contained",
                text: "Начать тест",
                type: "success",
                width: 120,
                onInitialized: function(e) {
                    if (todayInDelta) {
                        e.component.option("stylingMode", "contained");
                        e.component.option("disabled", false);
                        if (electronicKim !== null) {
                            e.component.option("text", "Продолжить тест");
                            e.component.option("width", "150px");
                        }
                        div.append($("<div>").css("display", " inline-block")
                            .append($("<span>").html("Доступ к тесту будет открыт с")));
                        div.append($("<div>").css("display", " inline-block").dxDateBox({
                            readOnly: true,
                            value: tdt,
                            width: 300,
                            stylingMode: "filled",
                            focusStateEnabled: false,
                            hoverStateEnabled: false,
                            displayFormat: "LongDateLongTime"
                        }));
                        div.append($("<div>").css("display", " inline-block")
                            .append($("<span>").html(" по ")));
                        div.append($("<div>").css("display", " inline-block").dxDateBox({
                            readOnly: true,
                            value: tdt2,
                            width: 300,
                            stylingMode: "filled",
                            focusStateEnabled: false,
                            hoverStateEnabled: false,
                            displayFormat: "LongDateLongTime"
                        }));
                        div.append($("<br>"));
                        //div.append($("<hr>"));
                    } else {
                        e.component.option("stylingMode", "outlined");
                        e.component.option("disabled", true);
                        div.append($("<div>").css("display", " inline-block")
                            .append($("<span>").html("Доступ к тесту будет открыт с")));
                        div.append($("<div>").css("display", " inline-block").dxDateBox({
                            readOnly: true,
                            value: tdt,
                            width: 300,
                            stylingMode: "filled",
                            focusStateEnabled: false,
                            hoverStateEnabled: false,
                            displayFormat: "LongDateLongTime"
                        }));
                        div.append($("<div>").css("display", " inline-block")
                            .append($("<span>").html(" по ")));
                        div.append($("<div>").css("display", " inline-block").dxDateBox({
                            readOnly: true,
                            value: tdt2,
                            width: 300,
                            stylingMode: "filled",
                            focusStateEnabled: false,
                            hoverStateEnabled: false,
                            displayFormat: "LongDateLongTime"
                        }));
                        div.append($("<br>"));
                    }
                },
                onClick: function(e) {
                    window.location.href = "/StudentExam/ValidateStartTest/?studentExamId=" + data.StudentExamID;
                }
            }));
            //div.append($("<hr>"));
        } else {
            div.append($("<div>").css("display", " inline-block")
                .append($("<span>").html("<b>Тест завершен: </b>")));
            div.append($("<div>").css("display", " inline-block").dxDateBox({
                readOnly: true,
                value: fd,
                width: 300,
                stylingMode: "filled",
                focusStateEnabled: false,
                hoverStateEnabled: false,
                displayFormat: "LongDateLongTime"
            }));
            div.append($("<br>"));

        }


        div.append($("<div>").dxFileUploader({
            selectButtonText: "Нажмите, чтобы прикрепить",
            multiple: true,
            accept: "*",
            name: "myFile",
            hint: "Название файла не должно содержать специальных символов (\\ '' / : * ? < > | )",
            value: [],
            uploadMode: "instantly",
            elementAttr: { id: data.StudentExamID },
            uploadUrl: "/CAnswerFile/UploadCAnswerFile",
            onUploadError: function processResult(result) {
                switch (result.request.statusText) {
                case "5":
                {
                    alert("Время для загрузки ответов еще не наступило, так как тест еще не пройден");
                    break;
                }
                case "1":
                {
                    alert("Время на загрузку ответов части 2 истекло");
                    break;
                }
                case "2":
                {
                    alert("Время на загрузку ответов части 2 еще не наступило");
                    break;
                }
                case "3":
                {
                    alert("Не удалось загрузить файл " + result.file.name + ". Обратитесь в техническую поддержку");
                    break;
                }
                case "4":
                {
                    alert("Файл с именем '" +
                        result.file.name +
                        "' уже существует. Измените название файла и загрузите повторно");
                    break;
                }
                }
            },
            onInitialized: function(e) {
                e.component.option("disabled", false);
                if (data.FinishDateTime != null) {
                    div.append($("<hr>"));
                    div.append($("<div>").css("display", " inline-block")
                        .append($("<span>").html("Загрузить файлы ответов части 2 можно до ")));
                    div.append($("<div>").css("display", " inline-block").dxDateBox({
                        readOnly: true,
                        value: fd1,
                        width: 300,
                        stylingMode: "filled",
                        focusStateEnabled: false,
                        hoverStateEnabled: false,
                        displayFormat: "LongDateLongTime"
                    }));
                } else {
                    div.append($("<hr>"));
                    div.append($("<div>").css("display", " inline-block")
                        .append($("<span>")
                            .html("Загрузить файлы ответов части 2 можно будет после завершения теста")));
                }
                //e.component.option("visible", data.FinishDateTime != null);
                var b = today >= fd1;
                //var b = fd == null;
                e.component.option("disabled", b);
            },
            onValueChanged: function(e) {
                var key = e.element.attr('id');
                e.component.option('uploadUrl', '/CAnswerFile/UploadCAnswerFile?key=' + key);
            },
            onUploaded: function(e) {
                var key = e.element.attr('id');
                var fileList = $("#dxUploadedCAnswerFileList" + key).dxDataGrid('instance');
                fileList.refresh();
            }
        }));


        div.append($("<div>").css("display", " inline-block")
            .append($("<span>").html("<p><b>Список файлов, содержащих ответы на задания части 2 </b>" +
                "<a id = 'dxGridInfo" +
                data.StudentExamID +
                "'> (?)</a></p>")));


        div.append($("<div id='dxGridInfoPopover" + data.StudentExamID + "'>").dxPopover({
            target: "#dxGridInfo" + data.StudentExamID,
            showEvent: "mouseenter",
            hideEvent: "mouseleave",
            position: "top",
            width: 300,
            contentTemplate: "Здесь отображаются все файлы, которые прикреплены к выбранному экзамену. " +
                "У загруженных файлов можно явно указать порядковый номер страницы. " +
                "Это поможет эксперту быстрее оценить вашу работу.",
            animation: {
                show: {
                    type: "pop",
                    from: { scale: 0 },
                    to: { scale: 1 }
                },
                hide: {
                    type: "fade",
                    from: 1,
                    to: 0
                }
            }
        }));
        div.append($("<br>"));

        var url = "/Api/CAnswerFiles/";
        div.append($("<div id='dxUploadedCAnswerFileList" + data.StudentExamID + "'>").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "CAnswerFileID",
                loadUrl: url + "GetCAnswerFilesByStudentExamId",
                updateUrl: url + "Put",
                deleteUrl: url + "Delete",
                onBeforeSend: function(method, ajaxOptions) {
                    if (method === 'load') ajaxOptions.url = `${ajaxOptions.url}?studentExamId=${data.StudentExamID}`;
                }
            }),
            wordWrapEnabled: true,
            columns: [
                {
                    dataField: "PageNumber",
                    caption: "Порядковый номер страницы",
                    width: 100
                },
                {
                    dataField: "FileID",
                    caption: "Имя файла",
                    allowEditing: false,
                    lookup: {
                        dataSource: DevExpress.data.AspNet.createStore({
                            key: "Value",
                            loadUrl: url + "/FilesLookup"
                        }),
                        valueExpr: "Value",
                        displayExpr: "Text"
                    }
                },
                {
                    type: "buttons",
                    width: 110,
                    buttons: [
                        "edit", "delete",
                        {
                            text: "Скачать",
                            icon: "download",
                            onClick: function(e) {
                                window.open("/CAnswerFile/DownLoadFile/" + e.row.data.CAnswerFileID, "_blank");
                            }
                        }
                    ]
                }
            ],
            editing: {
                mode: "row",
                useIcons: true,
                selectTextOnEditStart: true
            },
            onInitialized: function(e) {
                //e.component.option("visible", data.FinishDateTime != null);
                var boo = today <= fd1;
                e.component.option("editing.allowUpdating", boo);
                e.component.option("editing.allowDeleting", boo);
            }
        }));

        div.append($("<br>"));

        div.append($("<div>").dxButton({
            stylingMode: "outlined",
            icon: "download",
            text: "Скачать вариант КИМ",
            type: "default",
            onInitialized: function(e) {
                var boo = (data.Flags & 8) === 8;
                e.component.option("visible", boo);
                if (boo) div.append($("<br>"));
            },
            onClick: function() {
                window.open("/StudentExam/DownLoadElectronicKIM/?studentExamId=" + data.StudentExamID, "_blank");
            }
        }));

        break;
    }
    case 1:
    {
        if ((data.Flags & 2) === 2) {
            div.append($("<div>").css("display", " inline-block")
                .append($("<span>").html("<b>Время начала экзамена: </b>")));
            div.append($("<div>").css("display", " inline-block").dxDateBox({
                readOnly: true,
                value: data.StationExam.ExamStartupTime,
                width: 300,
                stylingMode: "filled",
                focusStateEnabled: false,
                hoverStateEnabled: false,
                displayFormat: "LongTime"
            }));
            div.append($("<div>").append($("<span>").html("<b>Пункт проведения ДТТ: </b>")));
            div.append($("<div>").dxTextArea({
                readOnly: true,
                value: data.StationExam.Station.FullName,
                autoResizeEnabled: true,
                stylingMode: "filled",
                height: "100%"
            }));
            div.append($("<div>").append($("<span>").html("<b>Адрес: </b>")));
            div.append($("<div>").dxTextArea({
                readOnly: true,
                value: data.StationExam.Station.StationAddress,
                autoResizeEnabled: true,
                stylingMode: "filled",
                height: "100%"
            }));
            if (tdt > today) {
                div.append($("<div>").append($("<span>").html("<p>ДТТ в очной форме проводится по форме и процедуре, максимально приближенной к процедуре и форме " +
                    "Государственной итоговой аттестации.</p>" +
                    "<p>Задания выполняются на бланках, идентичных бланкам ГИА, до начала работы проводится инструктаж.</p>" +
                    "<p>При прохождении ДТТ в очной форме участник ДТТ может быть удален с тестирования при нарушении " +
                    "им правил поведения во время написания тестирования (наличие посторонних предметов и материалов, " +
                    "средств связи, электронных устройств и т.д.), о чем делается соответствующая отметка в регистрационном бланке.</p>" +
                    "<p>Просим явиться в пункт проведения ДТТ за 15-20 минут до начала тестирования. При себе необходимо иметь маску!</p>")));
            }
            
        }
        break;
    }
    }

    //div.append($("<div>").css("margin-top", "10px").dxButton({
    //    stylingMode: "outlined",
    //    text: "Посмотреть результат",
    //    type: "default",
    //    onInitialized: function (e) {
    //        var boo = (data.Flags & 32) === 32;
    //        e.component.option("visible", boo);
    //        if (boo) div.append($("<br>"));
    //    },
    //    onClick: function () {
    //        window.open("https://www.ege.spb.ru/result/index.php?mode=dttege2021&wave=2", "_blank");
    //    }
    //}));

    div.append($("<div>").css("margin-top", "10px").dxButton({
        stylingMode: "outlined",
        text: "Посмотреть первичные баллы",
        type: "default",
        onInitialized: function (e) {
            var boo = (data.Flags & 32) === 32;
            e.component.option("visible", boo);
            if (boo) div.append($("<br>"));
        },
        onClick: function () {
            window.open("/StudentExam/ViewResultOfStudentExam/?studentExamId=" + data.StudentExamID, "_blank");
        }
    }));

    for (var item in data.Exam.ExamAddons) {
        if ((data.Exam.ExamAddons[item].Flags & 1) === 1) {
            div.append($("<div>").dxButton({
                stylingMode: "outlined",
                icon: "video",
                text: data.Exam.ExamAddons[item].AddonShortName,
                type: "default",
                onInitialized: function (e) {
                    var boo = (data.Flags & 64) === 64;
                    e.component.option("visible", boo);
                    if (boo) {
                        div.append($("<hr>"));
                        div.append($("<div>").css("display", " inline-block")
                            .append($("<span>").html("<b>Дата начала вебинара: </b>")));
                        div.append($("<div>").css("display", " inline-block").dxDateBox({
                            readOnly: true,
                            value: data.Exam.ExamAddons[item].StartDate,
                            width: 300,
                            stylingMode: "filled",
                            focusStateEnabled: false,
                            hoverStateEnabled: false,
                            displayFormat: "LongDate"
                        }));
                        div.append($("<br>"));
                        div.append($("<div>").css("display", " inline-block")
                            .append($("<span>").html("<b>Время начала вебинара: </b>")));
                        div.append($("<div>").css("display", " inline-block").dxDateBox({
                            readOnly: true,
                            value: data.Exam.ExamAddons[item].StartTime,
                            width: 300,
                            stylingMode: "filled",
                            focusStateEnabled: false,
                            hoverStateEnabled: false,
                            displayFormat: "LongTime"
                        }));
                        div.append($("<br>"));
                        div.append($("<div>").css("display", " inline-block")
                            .append($("<span>").html("<b>Описание: </b>" + data.Exam.ExamAddons[item].AddonDescription)));
                        div.append($("<br>"));
                        div.append($("<br>"));
                    }
                },
                onClick: function () {
                    window.open(new URL(data.Exam.ExamAddons[item].LinkAddress), "_blank");
                }
            }));
        }
    }

    

    return div;
}




