var rowClick = function (e) {
    if (!e.isExpanded) {
        this.expandRow(e.key);
    } else {
        this.collapseRow(e.key);
    }
}


function viewStatByQuestionTemplate(_, masterDetailOptions) {
    var data = masterDetailOptions.data;
    var examId = data.ExamID;
    var div = $('<div >');

    div.append($("<div>").append($("<h5>").html("Задания с кратким ответом")));
    var urlBQuestions = "/Api/CheckStudentExams/";
    div.append($("<div id='dxGridStatByBQuestion" + examId + "'>").css("max-height", "460px").dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            key: "Number",
            loadUrl: urlBQuestions + "GetStatByBQuestion",
            onBeforeSend: function (method, ajaxOptions) {
                if (method === 'load') ajaxOptions.url = `${ajaxOptions.url}?examId=${examId}`;
            }
        }),
        scrolling: { mode: "virtual" },
        paging: { enabled: false },
        wordWrapEnabled: true,
        showBorders: true,
        showRowLines: true,
        columnAutoWidth: true,
        onToolbarPreparing: function(e) {
            var toolbarItems = e.toolbarOptions.items;
            toolbarItems.push({
                widget: "dxButton",
                options: {
                    icon: "exportxlsx", text: "Экспорт", hint: "Экспортировать в Excel", onClick: function() {
                        var b = $("#dxGridStatByBQuestion" + examId).dxDataGrid('instance');
                        b.exportToExcel();
                    }
                },
                location: "after"
            });
        },
        onExporting: function (e) {
            var workbook = new ExcelJS.Workbook();
            var worksheet = workbook.addWorksheet('Часть С');

            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet: worksheet,
                autoFilterEnabled: true
            }).then(function () {
                workbook.xlsx.writeBuffer().then(function (buffer) {
                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Статистика по заданиям часть B.xlsx');
                });
            });
            e.cancel = true;
        },
        columns: [
            {
                dataField: "Number",
                caption: "Номер задания",
                alignment: "center"
            },
            {
                dataField: "MaxRate",
                caption: "Максимальный балл за задание",
                alignment: "center"
            },
            {
                caption: "Получили баллов",
                alignment: "center",
                columns: [
                    {
                        caption: "Ответ не был получен",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "RateNull",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.RateNull / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "0",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate0",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate0 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "1",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate1",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate1 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "2",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate2",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate2 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "3",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate3",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate3 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "4",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate4",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate4 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "5",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate5",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate5 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "6",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate6",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate6 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "7",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate7",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate7 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "8",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate8",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate8 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "9",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate9",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate9 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "10",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate10",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate10 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    }
                ]
            }
        ]
    }));
    div.append($("<br>"));

    div.append($("<div>").append($("<h5>").html("Задания с развернутым ответом")));
    var urlCQuestions = "/Api/CheckStudentExams/";
    div.append($("<div id='dxGridStatByCQuestion" + examId + "'>").css("max-height", "460px").dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            key: "Number",
            loadUrl: urlCQuestions + "GetStatByCQuestion",
            onBeforeSend: function (method, ajaxOptions) {
                if (method === 'load') ajaxOptions.url = `${ajaxOptions.url}?examId=${examId}`;
            }
        }),
        scrolling: { mode: "virtual" },
        paging: { enabled: false },
        wordWrapEnabled: true,
        showBorders: true,
        showRowLines: true,
        columnAutoWidth: true,
        onToolbarPreparing: function (e) {
            var toolbarItems = e.toolbarOptions.items;
            toolbarItems.push({
                widget: "dxButton",
                options: {
                    icon: "exportxlsx", text:"Экспорт", hint: "Экспортировать в Excel", onClick: function () {
                        var b = $("#dxGridStatByCQuestion" + examId).dxDataGrid('instance');
                        b.exportToExcel();
                    }
                },
                location: "after"
            });
        },
        onExporting: function (e) {
            var workbook = new ExcelJS.Workbook();
            var worksheet = workbook.addWorksheet('Часть С');

            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet: worksheet,
                autoFilterEnabled: true
            }).then(function () {
                workbook.xlsx.writeBuffer().then(function (buffer) {
                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Статистика по заданиям часть C.xlsx');
                });
            });
            e.cancel = true;
        },
        columns: [
            {
                dataField: "Number",
                caption: "Номер критерия",
                alignment: "center"
            },
            {
                dataField: "MaxRate",
                caption: "Максимальный балл за задание",
                alignment: "center"
            },
            {
                caption: "Получили баллов",
                alignment: "center",
                columns: [
                    {
                        caption: "Ответ не был получен",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "RateNull",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.RateNull / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "0",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate0",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate0 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "1",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate1",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate1 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "2",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate2",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate2 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "3",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate3",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate3 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "4",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate4",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate4 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "5",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate5",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate5 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "6",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate6",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate6 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "7",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate7",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate7 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "8",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate8",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate8 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "9",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate9",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate9 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    },
                    {
                        caption: "10",
                        alignment: "center",
                        columns: [
                            {
                                dataField: "Rate10",
                                caption: "Чел.",
                                alignment: "center"
                            },
                            {
                                caption: "%",
                                alignment: "center",
                                calculateDisplayValue: function (e) {
                                    var allCount = e.RateNull + e.Rate0 + e.Rate1 + e.Rate2 + e.Rate3 +
                                        e.Rate4 + e.Rate5 + e.Rate6 + e.Rate7 + e.Rate8 + e.Rate9 + e.Rate10;
                                    return allCount === 0 ? 0 + "%" : +(e.Rate10 / allCount * 100).toFixed(2) + "%";
                                }
                            }
                        ]
                    }
                ]
            }
        ]
    }));
    div.append($("<br>"));

    //div for part D

    return div;
}

