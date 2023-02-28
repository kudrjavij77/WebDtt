var getNewStudentExamsToCheck = function() {
    var popup = $('#popupFormAddNewSeToCheck').dxPopup('instance');
    popup.option("contentTemplate", $('#popup-template'));
    popup.show();
}

var finishCheckStudentExam = function (e) {
    var cr = $("#cRates" + e).dxDataGrid('instance');
    cr.saveEditData();
    window.location.href = "/CheckStudentExam/FinishCheckStudentExam?studentExamId=" + e;
}

var rowClick = function (e) {
    
    if (!e.isExpanded) {
        this.expandRow(e.key);
    } else {
        this.collapseRow(e.key);
    }
}



function editStudentExamCRatesTemplate(_, masterDetailOptions) {
     
    var form = $('<div>');
    var data = masterDetailOptions.data;
    var seId = data.StudentExamID;

    form.append($('<div>').html('<b>Вариант КИМ: </b>' + data.VariantNumber));  //+ data.ExamKim.VariantNumber));
    form.append($('<br>'));

    var div = $('<div class="checkContainer">');

    var urlCAnFiles = "/Api/CAnswerFiles/";
    div.append($('<div class="cAnswerFiles">').dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            key: "CAnswerFileID",
            loadUrl: urlCAnFiles + "GetCAnswerFilesByStudentExamId",
            onBeforeSend: function (method, ajaxOptions) {
                if (method === 'load') ajaxOptions.url = `${ajaxOptions.url}?studentExamId=${data.StudentExamID}`;
            }
        }),
        scrolling: { mode: "virtual" },
        wordWrapEnabled: true,
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
                        onClick: function (e) {
                            window.open("/CAnswerFile/DownLoadFile/" + e.row.data.CAnswerFileID, "_blank");
                        }
                    }
                ]
            }
        ],
        onToolbarPreparing: function (e) {
            var toolbarItems = e.toolbarOptions.items;
            // Adds a new item
            toolbarItems.push(
                {
                widget: "dxButton",
                options: {
                    icon: "check", text: "Завершить проверку", type: "success", onClick: function (e) {
                        return finishCheckStudentExam(seId);
                    }
                },
                location: "before"
            },
                {
                    widget: "dxButton",
                    options: {
                        icon: "download", text: "Скачать вариант КИМ", type: "normal", stylingMode: "outlined", onClick: function (e) {
                            window.open("/StudentExam/DownLoadElectronicKIM/?studentExamId=" + data.StudentExamID, "_blank");
                        }
                    },
                    location: "before"
                });
        }
    }));

    var urlCRates = "/Api/CRates/";
    div.append($("<div id='cRates" + data.StudentExamID + "' style='margin-left:5px; width:310px;'>").dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            key: "CRateID",
            loadUrl: urlCRates + "GetByStudentExamId",
            updateUrl: urlCRates + "Put",
            onBeforeSend: function(method, ajaxOptions) {
                if (method === 'load')
                    ajaxOptions.url =
                        `${ajaxOptions.url}?studentExamId=${data.StudentExamID}&expertId=${data.ExpertID}`;
            }
        }),
        showBorders: true,
        scrolling: { mode: "virtual" },
        wordWrapEnabled: true,
        editing: {
            mode: "batch",
            allowUpdating: true,
            useIcons: true
        },
        onEditorPreparing: function(e) {
            if (e.dataField === "Rate" && e.parentType === "dataRow") {
                var items = [{ KeyRate: "X" }];
                for (var crate in e.row.data.CQuestion.CQuestionRates) {
                    if (Object.prototype.hasOwnProperty.call(e.row.data.CQuestion.CQuestionRates, crate)) {
                        items.push(e.row.data.CQuestion.CQuestionRates[crate]);
                    }
                }
                e.editorType = "dxSelectBox";
                e.editorOptions.items = items;
                e.editorOptions.displayExpr = function(e) {
                    return e === null ? "X" : e.KeyRate;
                };
                e.editorOptions.valueExpr = "KeyRate";
                e.editorOptions.onValueChanged = function(e1) {
                    var value = e1.value === "X" ? null : e1.value;
                    e.setValue(value);
                }
            }
        },
        columns: [
            {
                dataField: "CRateID",
                caption: "RowID",
                allowEditing: false,
                visible: false
            },
            {
                dataField: "CQuestion.Number",
                calculateDisplayValue: function (rowData) { return rowData.CQuestion.Number + 1; },
                width: 150,
                alignment: "center",
                caption: "Критерий",
                allowEditing: false
            },
            {
                dataField: "Rate",
                calculateDisplayValue: function (e) {
                    if (e.Rate !== null) return e.Rate;
                    return (e.Flags && 1) > 0 ? "X" : null;
                },
                caption: "Балл",
                alignment: "center",
                width: 150,
                allowEditing: true
            }
        ],
        onToolbarPreparing: function (e) {
            var toolbarItems = e.toolbarOptions.items;
            // Adds a new item
            toolbarItems.push({
                widget: "dxButton",
                options: {
                    icon: "", text: "", visible: false, onClick: function () {
                    }
                },
                location: "after"
            });
        }
        
    }));

    form.append(div);

    return form;
}

var aisGetReadonlyLookup = function (path, options) {
    var params = {
        key: "ObjectID",
        loadUrl: path
    };
    if (options) {
        params = Object.assign({}, params, options);
    }
    return DevExpress.data.AspNet.createStore(params);
};

