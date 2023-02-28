$(function() {
    var stExamId = $("#inputId").data("value");
    var urlB = "/Api/BAnswers/";
    var urlC = "/Api/CRates/";
    var urlD = "/Api/DRates/";
    var urlCAnFiles = "/Api/CAnswerFiles/";


    $('#gridBAnswers').dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            loadUrl: urlB + "GetByStudentExamIdForUser",
            loadParams: { studentExamId: stExamId }
        }),
        height: 410,
        paging: { enabled: false },
        showBorders: true,
        showRowLines: true,
        summary: {
            totalItems: [
                {
                    column: "Rate",
                    summaryType: "sum"
                }
            ]
        },
        columns: [
            {
                dataField: "Number",
                caption: "Номер задания",
                calculateDisplayValue: function (rowData) { return rowData.Number + 1; },
                alignment: "center"
            },
            {
                dataField: "Key",
                caption: "Правильный ответ",
                calculateDisplayValue: function(rowData) {
                    var str = "";
                    for (var i = 0; i < rowData.Key.length; i++) {
                        str += rowData.Key[i].KeyValue;
                        if (i !== rowData.Key.length - 1) {
                            str += "; ";
                        }
                    }
                    return str; 
                    
                },
                alignment: "center"
            },
            {
                dataField: "Value",
                caption: "Ответ участника",
                alignment: "center"
            },
            {
                dataField: "Rate",
                caption: "Первичный балл",
                alignment: "center"
            },
            {
                dataField: "Flags",
                caption: "Статус проверки",
                alignment: "center",
                calculateCellValue: function (e) {
                    if (e.Flags === 0 || e.Flags === null) return "Не проверен";
                    if ((e.Flags & 1) === 1) return "Проверен";
                    if ((e.Flags & 4) === 4) return "На проверке";
                }
            }
        ]

    });


    $('#gridCRates').dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            loadUrl: urlC + "GetByStudentExamIdForUser",
            loadParams: { studentExamId: stExamId }
        }),
        height: 410,
        paging: { enabled: false },
        showBorders: true,
        showRowLines: true,
        summary: {
            totalItems: [
                {
                    column: "Rate",
                    summaryType: "sum"
                }
            ]
        },
        columns: [
            {
                dataField: "CQuestion.Number",
                caption: "Номер критерия",
                calculateDisplayValue: function (rowData) { return rowData.CQuestion.Number + 1; },
                alignment: "center"
            },
            {
                dataField: "Rate",
                calculateDisplayValue: function (e) {
                    if (e.Rate !== null) return e.Rate;
                    return (e.Flags && 1) > 0 ? "X" : null;
                },
                caption: "Первичный балл",
                alignment: "center"
            },
            {
                dataField: "Flags",
                caption: "Статус проверки",
                alignment: "center",
                lookup: {
                    dataSource: ({
                        store: cRateFlags
                    }),
                    valueExpr: "id",
                    displayExpr: "name"
                }

            }
        ]
    });


    $('#cAnswerFiles').dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            key: "CAnswerFileID",
            loadUrl: urlCAnFiles + "GetCAnswerFilesByStudentExamId",
            onBeforeSend: function(method, ajaxOptions) {
                if (method === 'load') ajaxOptions.url = `${ajaxOptions.url}?studentExamId=${stExamId}`;
            }
        }),
        scrolling: { mode: "virtual" },
        wordWrapEnabled: true,
        showBorders: true,
        showRowLines: true,
        columns: [
            {
                dataField: "PageNumber",
                caption: "№",
                width: 40
            },
            {
                dataField: "FileID",
                caption: "Имя файла",
                allowEditing: false,
                lookup: {
                    dataSource: DevExpress.data.AspNet.createStore({
                        key: "Value",
                        loadUrl: urlCAnFiles + "/FilesLookup"
                    }),
                    valueExpr: "Value",
                    displayExpr: "Text"
                }
            },
            {
                caption: "Скачать",
                type: "buttons",
                width: 110,
                buttons: [
                    {
                        text: "Скачать",
                        icon: "download",
                        onClick: function(e) {
                            window.open("/CAnswerFile/DownLoadFile/" + e.row.data.CAnswerFileID, "_blank");
                        }
                    }
                ]
            }
        ]
    });


})