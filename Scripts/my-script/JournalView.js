function JournalView() {
    var _this = this;
    //this.userId = userId;
    this.lessonId = 0;
    this.groupId = 0;
    this.groupSource = null;
    this.groupSelectBox = null;
    this.personGroupId = 0;

    this.lessonSource = DevExpress.data.AspNet.createStore({
        loadUrl: '/api/Journals/Lessons?groupId=',
        insertUrl: '/api/Journals/InsertLesson?groupId=',
        updateUrl: '/api/Journals/UpdateLesson',
        key: "LessonID",
        onBeforeSend: function(method, ajaxOptions) {
            if (method === "update") return;
            ajaxOptions.url += _this.groupId;
        }
    });

    this.grid = null;
    this.getStore = function () {
        var store = new DevExpress.data.CustomStore({
            key: "PersonID",
            load: function (loadOptions) {
                var func = function(resolve, reject) {
                    $.get(`/api/Journals/GetJournalFieldValues?lessonId=${_this.lessonId}&groupId=${_this.groupId}`,
                        function (data) {
                            var personGroup = 0;
                            var pivotItem = {};
                            var itemArray = [];
                            for (var i = 0; i < data.length; i++) {
                                var item = data[i];
                                if (personGroup !== item.PersonGroupID) {
                                    pivotItem = {
                                        PersonGroupID: item.PersonGroupID,
                                        PersonID: item.PersonID,
                                        FIO: item.FIO,
                                        IsLessonMaterials: item.IsLessonMaterials
                                    }
                                    itemArray.push(pivotItem);
                                }
                                var fieldId = `${item.FieldName}ID`;
                                pivotItem[fieldId] = item["JournalFieldID"];
                                var fieldName = `${item.FieldName}`;
                                if (item["JFV"]) {
                                    pivotItem[fieldName] = item['JFV']["Value"];
                                    
                                }
                                personGroup = item.PersonGroupID;
                            }
                            resolve(itemArray);
                        }).fail(function(e) { reject(e) });
                }
                var promise = new Promise(func);
                return promise;
            },
            update: function(key, values) {
                var func = function (resolve, reject) {
                    _this.grid.byKey(key).done(function (row) {
                        var keys = Object.keys(values);
                        var success = 0;
                        for (var k in keys) {
                            if (keys.hasOwnProperty(k)) {
                                var jvKey = `${keys[k]}ID`;
                                var journalFieldId = row[jvKey];
                                var value = values[keys[k]];
                                var kInner = k;
                                if (typeof value === "undefined") {
                                    value = "";
                                }
                                $.ajax({
                                    url: `/api/Journals/UpdateJournalValue?personGroupId=${row.PersonGroupID}&journalFieldId=${journalFieldId}&value=${value}`,
                                    method: "PUT",
                                    //error: reject(),
                                    success: function () {
                                        success++;
                                        if (success == keys.length) {
                                            resolve();
                                        }
                                    }
                                });
                            }
                        }
                    });
                }
                var promise = new Promise(func);
                return promise;
            }
        });
        return store;
    }

    this.formatGrid = function (value) {
        if (value.value == null) {
            _this.lessonId = 0;
            $('#dxDataGridJournalFields').css('display', 'none');
            $('#accordion').css('display', 'none');
            $('#exportToExcel').css('display', 'none');
            return;
        } else {
            $('#dxDataGridJournalFields').css('display', 'block');
            $('#accordion').css('display', 'block');
            $('#exportToExcel').css('display', 'inline-block');
        }

        $(function () {
            $('#exportToExcel').dxButton({
                type: "default",
                stylingMode: "outlined",
                text: "Экспорт в Excel список группы",
                icon: "exportxlsx",
                visible: true,
                onClick: function (e) {
                    _this.grid.exportToExcel();
                }
            });
        });


        //card: Topics
        $(function() {
            $('#headingOne').dxButton({
                type: "default",
                stylingMode: "outlined",
                text: "Темы занятия",
                elementAttr: {
                    class: "card-header btn btn-outline-primary",
                    style: "color:#2e5188; font-size: 16px; text-align:left"
                }
            });

            $('#addTopic').dxButton({
                type: "default",
                stylingMode: "outlined",
                text: "Добавить тему",
                icon: "add",
                onClick: function(e) {
                    $('#dxTopicsGrid').dxDataGrid('instance').addRow();
                }
            });

            $('#dxTopicsGrid').dxDataGrid({
                dataSource: DevExpress.data.AspNet.createStore({
                    loadUrl: '/api/Journals/Topics?groupId=',
                    insertUrl: '/api/Journals/InsertTopic?lessonId=',
                    updateUrl: '/api/Journals/UpdateTopic?lessonId=',
                    key: "TopicID",
                    onBeforeSend: function(method, ajaxOptions) {
                        if (method === "update") ajaxOptions.url += _this.lessonId;
                        if (method === "insert") ajaxOptions.url += _this.lessonId;
                        if (method === "load") ajaxOptions.url += _this.groupId + '&lessonId=' + _this.lessonId;
                    }
                }),
                wordWrapEnabled: true,
                showRowLines: true,
                editing: {
                    allowUpdating: true,
                    useIcons: true
                },
                columns: [
                    {
                        dataField: "TopicName", caption: "Название темы",
                        validationRules: [
                            {
                                type: "stringLength",
                                max: 250,
                                message: "Максимальная длина названия - 250 символов"
                            }
                        ]
                    },
                    { dataField: "Description", caption: "Описание" },
                    {
                        dataField: "InLesson",
                        dataType: "boolean",
                        caption: "Проходят на выбранном занятии",
                        sortOrder: "desc",
                        cellTemplate: function(container, options) {
                            $('<div>').dxCheckBox({
                                value: options.value ? true : false,
                                disabled: true
                            }).appendTo(container);
                        },
                        editCellTemplate: function(e, i) {
                            var t = $('<div>').dxCheckBox({
                                value: i.value ? true : false,
                                onValueChanged: function(v) {
                                    i.setValue(v.value ? true : false);
                                }
                            });
                            return t;
                        }
                    },
                    { dataField: "TopicID", visible: false, sortOrder: "desc" },
                    {
                        dataField: "Flags",
                        caption: "Состояние",
                        lookup: {
                            dataSource: function() { return { store: topicFlags }; },
                            valueExpr: "id",
                            displayExpr: "name"
                        }
                    }
                ]
            }).dxDataGrid('instance');
        });

        //card: LessonMaterials
        $(function () {
            $('#headingTwo').dxButton({
                type: "default",
                stylingMode: "outlined",
                text: "Материалы занятия",
                elementAttr: {
                    class: "card-header btn btn-outline-primary",
                    style: "color:#2e5188; font-size: 16px; text-align:left"
                }
            });

            $('#addFile').dxFileUploader({
                selectButtonText: "Нажмите, чтобы прикрепить",
                multiple: true,
                accept: "*",
                name: "myFile",
                hint: "Название файла не должно содержать специальных символов (\\ '' / : * ? < > | )",
                value: [],
                uploadMode: "instantly",
                uploadUrl: "/Journal/UploadLessonMaterial",
                onValueChanged: function (e) {
                    e.component.option('uploadUrl', '/Journal/UploadLessonMaterial?lessonId=' + _this.lessonId);
                },
                onUploadError: function processResult(result) {
                    alert(result.request.statusText);
                },
                onUploaded: function() {
                    $('#dxLessonMaterialsGrid').dxDataGrid('instance').refresh();
                }
            });

            $('#dxLessonMaterialsGrid').dxDataGrid({
                dataSource: DevExpress.data.AspNet.createStore({
                    loadUrl: '/api/Journals/LessonMaterials?groupId=',
                    deleteUrl: '/api/Journals/DeleteLessonMaterial',
                    key: "LessonMaterialID",
                    onBeforeSend: function(method, ajaxOptions) {
                        if (method !== 'load') return;
                        ajaxOptions.url += _this.groupId + '&lessonId=' + _this.lessonId + '&personGroupId=' + null;
                    }
                }),
                wordWrapEnabled: true,
                showRowLines: true,
                editing: { allowDeleting: true, useIcons: true },
                columns: [
                    { dataField: "LessonMaterialID", visible: false },
                    { dataField: "FileName", caption: "Имя файла" },
                    { dataField: "CreateDate", dataType: "date", format: "shortdate", caption: "Дата загрузки" },
                    {
                        name: "dx_Buttons",
                        type: "buttons",
                        visibleIndex: 100,
                        buttons: [
                            {
                                hint: "Скачать файл",
                                icon: "download",
                                visible: true,
                                onClick: function (e) {
                                    window.open("/Journal/DownLoadLessonMaterial/?lessonMaterialId=" + e.row.data.LessonMaterialID, "_blank");
                                }
                            },
                            "delete"
                        ]
                    }
                ]
            }).dxDataGrid('instance');

            $('#addLink').dxButton({
                type: "default",
                stylingMode: "outlined",
                text: "Добавить ссылку",
                icon: "plus",
                onClick: function () { $('#dxLessonAddonsGrid').dxDataGrid('instance').addRow(); }
            });

            $('#dxLessonAddonsGrid').dxDataGrid({
                dataSource: DevExpress.data.AspNet.createStore({
                    loadUrl: '/api/Journals/LessonAddons?lessonId=',
                    insertUrl: '/api/Journals/InsertLessonAddon?lessonId=',
                    updateUrl: '/api/Journals/UpdateLessonAddon',
                    deleteUrl: '/api/Journals/DeleteLessonAddon',
                    key: "LessonAddonID",
                    onBeforeSend: function (method, ajaxOptions) {
                        if (method === 'load') ajaxOptions.url += _this.lessonId;
                        if (method === 'insert') ajaxOptions.url += _this.lessonId;
                    }
                }),
                wordWrapEnabled: true,
                showRowLines: true,
                editing: {
                    allowUpdating: true,
                    allowDeleting: true,
                    useIcons: true
                },
                columns: [
                    {
                        dataField: "AddonShortName", caption: "Краткое название",
                        validationRules: [
                            {
                                type: "stringLength",
                                max: 150,
                                message: "Максимальная длина названия - 150 символов"
                            }
                        ]
                    },
                    { dataField: "AddonDescription", caption: "Описание" },
                    {
                        dataField: "LinkAddress", caption: "Ссылка",
                        cellTemplate: function (container, options) {
                            $("<div />").dxButton({
                                icon: "link",
                                stylingMode: "text",
                                type: "default",
                                text: "Перейти по ссылке",
                                onClick: function() {
                                    window.open(options.value, "_blank");
                                }
                            }).appendTo(container);
                        }
                    }
                ]
            }).dxDataGrid('instance');
        });

        //card: JournalFields
        $(function () {
            $('#headingThree').dxButton({
                type: "default",
                stylingMode: "outlined",
                text: "Редактор журнала",
                elementAttr: {
                    class: "card-header btn btn-outline-primary",
                    style: "color:#2e5188; font-size: 16px; text-align:left"
                }
            });

            $('#addJornalField').dxButton({
                type: "default",
                stylingMode: "outlined",
                text: "Добавить колонку в журнал",
                icon: "plus",
                onClick: function() {
                    $('#dxFieldsGrid').dxDataGrid('instance').addRow();
                }
            });

            $('#dxFieldsGrid').dxDataGrid({
                dataSource: DevExpress.data.AspNet.createStore({
                    loadUrl: '/api/Journals/GetFields?lessonId=',
                    insertUrl: '/api/Journals/InsertField?lessonId=',
                    updateUrl: '/api/Journals/UpdateField',
                    key: "JournalFieldID",
                    onBeforeSend: function (method, ajaxOptions) {
                        if (method === "insert") ajaxOptions.url += _this.lessonId;
                        if (method === "load") ajaxOptions.url += _this.lessonId;
                    }
                }),
                wordWrapEnabled: true,
                showRowLines: true,
                editing: {
                    allowUpdating: true,
                    useIcons: true
                },
                columns: [
                    {
                        dataField: "FieldName",
                        caption: "Название колонки",
                        validationRules: [
                            {
                                type: "pattern",
                                pattern: /^[a-zA-Z0-9\sа-яА-Я()_]+$/,
                                message: "В названии колонки можно использовать только буквы, цифры, круглые скобки."
                            },
                            {
                                type: "stringLength",
                                max: 100,
                                message: "Максимальная длина названия - 100 символов"
                            }
                        ]
                    },
                    { dataField: "FieldDescription", caption: "Описание" },
                    { dataField: "MaxValue", dataType: "number", caption: "Максимальное значение" },
                    {
                        dataField: "Flags",
                        caption: "Состояние",
                        visible: false,
                        lookup: {
                            dataSource: function () { return { store: journalFieldFlags }; },
                            valueExpr: "id",
                            displayExpr: "name"
                        }
                    },
                    {
                        dataField: "UserVisability",
                        dataType: "boolean",
                        caption: "Видит слушатель",
                        cellTemplate: function (container, options) {
                            $('<div>').dxCheckBox({
                                value: options.value ? true : false,
                                disabled: true
                            }).appendTo(container);
                        },
                        editCellTemplate: function (e, i) {
                            var t = $('<div>').dxCheckBox({
                                value: i.value ? true : false,
                                onValueChanged: function (v) {
                                    i.setValue(v.value ? true : false);
                                }
                            });
                            return t;
                        }
                    },
                    {
                        dataField: "ForPrint",
                        dataType: "boolean",
                        caption: "Для печатного журнала",
                        cellTemplate: function (container, options) {
                            $('<div>').dxCheckBox({
                                value: options.value ? true : false,
                                disabled: true
                            }).appendTo(container);
                        },
                        editCellTemplate: function (e, i) {
                            var t = $('<div>').dxCheckBox({
                                value: i.value ? true : false,
                                onValueChanged: function (v) {
                                    i.setValue(v.value ? true : false);
                                }
                            });
                            return t;
                        }
                    }
                ],
                onRowInserted: function() {
                    $.get(`/api/Journals/JournalFields?lessonId=${_this.lessonId}`, rebuildJournalGrid);
                    _this.grid.refresh();
                },
                onRowUpdated: function () {
                    $.get(`/api/Journals/JournalFields?lessonId=${_this.lessonId}`, rebuildJournalGrid);
                    _this.grid.refresh(); }
            }).dxDataGrid('instance');

        });

        _this.lessonId = value.value;
        $.get(`/api/Journals/JournalFields?lessonId=${value.value}`, rebuildJournalGrid);
    }

    this.lessonsGridInstance = null;
    this.lessonSelectBox = $('#dxSelectBoxLessons').dxDropDownBox({
        dataSource: _this.lessonSource,
        displayExpr: function(e) {
            return new Date(e.LessonDate).toLocaleString();
        },
        valueExpr: "LessonID",
        disabled: true,
        contentTemplate: function(e) {
            var value = e.component.option('value');

            var lessonsGrid = $('<div>').dxDataGrid({
                dataSource: e.component.getDataSource(),
                editing: {
                    allowAdding: true,
                    allowUpdating: true,
                    //allowDeleting: true,
                    useIcons: true
                },
                columns: [
                    {
                        dataField: "LessonDate",
                        dataType: "datetime",
                        format: "longDateLongTime",
                        sortOrder: "desc",
                        caption: "Дата занятия",
                    },
                    {
                        dataField: "Duration",
                        caption: "Продолжительность",
                    },
                    {
                        dataField: "Flags",
                        caption: "Статус",
                        lookup: {
                            dataSource: function(options) {
                                return {
                                    store: lessonFlags
                                };
                            },
                            valueExpr: "id",
                            displayExpr: "name"
                        }
                    }
                    ,
                    {
                        type: "buttons",
                        buttons: [
                            "edit",
                            {
                                hint: "Удалить",
                                icon: "trash",
                                visible: false,
                                onClick: function(e) {

                                }
                            },
                            {
                                hint: "Клонировать",
                                visible: false,
                                icon: "repeat",
                                onClick: function(e) {

                                }
                            }
                        ]
                    }
                ],
                filterRow: { visible: true },
                selection: { mode: "single" },
                wordWrapEnabled: true,
                selectedRowKeys: [value],
                height: "100%",
                width: "100%",
                onSelectionChanged: function (selectedItems) {
                    var keys = selectedItems.selectedRowKeys,
                        hasSelection = keys.length;

                    e.component.option("value", hasSelection ? keys[0] : null);
                }
            });
            _this.lessonsGridInstance = lessonsGrid.dxDataGrid("instance");

            e.component.on("valueChanged", function (args) {
                _this.lessonsGridInstance.selectRows(args.value, false);
                e.component.close();
            });

            return lessonsGrid;
        },
        onValueChanged: _this.formatGrid
    }).dxDropDownBox('instance');

    $.get('/api/Journals/Groups', function(data) {
        _this.groupSource = data;
        _this.groupSelectBox = $('#dxSelectBoxGroups').dxSelectBox({
            dataSource: _this.groupSource,
            displayExpr: "GroupName",
            valueExpr: "GroupID",
            placeholder: "Выберите группу...",
            onValueChanged: function(v) {
                _this.groupId = v.value;
                _this.lessonSource.load().done(function () {
                    _this.lessonSelectBox.reset();
                    _this.lessonSelectBox.option('dataSource', _this.lessonSource);
                    if (_this.lessonsGridInstance) {
                        _this.lessonsGridInstance.option('dataSource', _this.lessonSource);
                    }
                });
                var d = _this.lessonSelectBox.option('disabled');
                if (d) {
                    _this.lessonSelectBox.option('disabled', false);
                    _this.lessonSelectBox.option('placeholder', 'Выберите занятие...');
                }
            }
        }).dxSelectBox('instance');
    });

    function rebuildJournalGrid(data) {
        var gridColumns = [
            {
                name: "dx_PersonID",
                dataField: "PersonID",
                dataType: "number",
                visible: false,
                allowEditing: false,
                allowExporting: false
            },
            {
                name: "dx_PersonGroupID",
                dataField: "PersonGroupID",
                dataType: "number",
                visible: false,
                allowEditing: false,
                allowExporting: false
            },
            {
                name: "dx_IsLessonMaterials",
                dataField: "IsLessonMaterials",
                dataType: "boolean",
                visible: false,
                allowEditing: false,
                allowExporting: false
            },
            {
                name: "dx_FIO",
                dataField: "FIO",
                dataType: "string",
                caption: "ФИО",
                visible: true,
                allowEditing: false,
                allowExporting: true,
                sortOrder: "asc"
            },
            {
                name: "dx_Buttons",
                type: "buttons",
                visibleIndex: 100,
                allowExporting: false,
                buttons: [
                    {
                        hint: "Посмотреть прикрепленные файлы",
                        icon: "product",
                        visible: function (e) { return e.row.data.IsLessonMaterials; },
                        onClick: function (e) {
                            _this.personGroupId = e.row.data.PersonGroupID;
                            $("#popup").dxPopup({
                                contentTemplate: function () {
                                    return $('<div>').dxDataGrid({
                                        dataSource: DevExpress.data.AspNet.createStore({
                                            loadUrl: '/api/Journals/LessonMaterials?groupId=',
                                            key: "LessonMaterialID",
                                            onBeforeSend: function (method, ajaxOptions) {
                                                if (method !== 'load') return;
                                                ajaxOptions.url += _this.groupId + '&lessonId=' + _this.lessonId + '&personGroupId=' + _this.personGroupId;
                                            }
                                        }),
                                        columnAutoWidth: true,
                                        showRowLines: true,
                                        wordWrapEnabled: true,
                                        columns: [
                                            { dataField: "LessonMaterialID", visible: false },
                                            { dataField: "FileName", caption: "Файл" },
                                            { dataField: "CreateDate", dataType: "datetime", format: "longDateLongTime", caption: "Дата загрузки" },
                                            {
                                                name: "dx_Buttons",
                                                type: "buttons",
                                                visibleIndex: 100,
                                                buttons: [
                                                    {
                                                        hint: "Скачать",
                                                        icon: "download",
                                                        onClick: function (e) { window.open("/Journal/DownLoadLessonMaterial/?lessonMaterialId=" + e.row.data.LessonMaterialID, "_blank");}
                                                    }
                                                ]
                                            }
                                        ]

                                    });
                                },
                                width: 500,
                                height: 400,
                                container: ".dx-viewport",
                                showTitle: true,
                                title: e.row.data.FIO + " (материалы занятия)",
                                visible: false,
                                dragEnabled: false,
                                closeOnOutsideClick: true,
                                showCloseButton: false,
                                position: {
                                    at: "bottom",
                                    my: "center",
                                    of: e.element
                                },
                                toolbarItems: [{
                                    widget: "dxButton",
                                    toolbar: "bottom",
                                    location: "after",
                                    options: {
                                        text: "Close",
                                        onClick: function () {
                                            $("#popup").dxPopup('instance').hide();
                                        }
                                    }
                                }]
                            }).dxPopup("instance").show();
                        }
                    },
                    {
                        hint: "Отправить сообщение слушателю",
                        icon: "message",
                        visible: false,
                        onClick: function (e) {

                        }
                    }
                ]
            }
        ];

        for (var i = 0; i < data.length; i++) {
            var col = {
                name: `dx_${data[i]["FieldName"]}`,
                dataField: data[i]["FieldName"],
                dataType: "number",
                caption: data[i]["FieldName"],
                visible: true,
                allowEditing: true,
                allowExporting: false
            };
            if (data[i]["MaxValue"] === 1) {
                col.cellTemplate = function (container, options) {
                    $('<div>').dxCheckBox({
                        value: options.value ? true : false,
                        disabled: true
                    }).appendTo(container);
                };
                col.alignment = 'center';
                col.editCellTemplate = function (e, i) {
                    var t = $('<div>').dxCheckBox({
                        value: i.value ? true : false,
                        onValueChanged: function (v) {
                            i.setValue(v.value ? 1 : 0);
                        }
                    });
                    return t;
                };

            } else {
                col.validationRules = [
                    {
                        type: "range",
                        min: 0,
                        max: data[i]["MaxValue"],
                        message: `Значение должно быть от 0 до ${data[i]["MaxValue"]}`,
                        ignoreEmptyValue: true
                    }
                    //, {
                    //    type: "pattern",
                    //    pattern: "^ [^ 0 - 9]",
                    //    message: "Do not use digits in the Name."
                    //}
                ];
            }
            gridColumns.push(col);

            var col2 = {
                name: `dx_${data[i]["FieldName"]}ID`,
                dataField: `${data[i]["FieldName"]}ID`,
                dataType: "number",
                visible: false,
                allowEditing: false,
                allowExporting: false
            };
            gridColumns.push(col2);
        }

        var gridOptions = {
            columns: gridColumns,
            showRowLines: true,
            editing: {
                allowAdding: false,
                allowDeleting: false,
                allowUpdating: true,
                mode: "cell"
            },
            dataSource: _this.getStore(),
            //export: {
            //    enabled: true
            //},
            onExporting: function (e) {
                var workbook = new ExcelJS.Workbook();
                var worksheet = workbook.addWorksheet(new Date(_this.lessonsGridInstance.getSelectedRowsData()[0].LessonDate).toLocaleDateString());

                DevExpress.excelExporter.exportDataGrid({
                    component: e.component,
                    worksheet: worksheet,
                    autoFilterEnabled: true
                }).then(function () {
                    workbook.xlsx.writeBuffer().then(function (buffer) {
                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }),
                            'Группа ' + _this.groupSelectBox.option('displayValue') + ' Дата занятия ' + new Date(_this.lessonsGridInstance.getSelectedRowsData()[0].LessonDate).toLocaleDateString() + '.xlsx');
                    });
                });
                e.cancel = true;
            }
        }

        _this.grid = $('#dxDataGridJournalFields').dxDataGrid(gridOptions).dxDataGrid('instance');
    }

    
}



