
$(function (e) {
    $('#dxMailForm').dxForm({
        showValidationSummary: true,
        formData: formData,
        labelLocation: "top",
        colCount: 2,
        items: [
            {
                dataField: "GroupIds",
                editorType: "dxDropDownBox",
                label: { text: "Группы" },
                colSpan: 2,
                isRequired: true,
                editorOptions: {
                    dataSource: DevExpress.data.AspNet.createStore({
                        loadUrl: '/Api/Lookups/SchedularGroups',
                        key: "GroupID"
                    }),
                    showClearButton: true,
                    displayExpr: "GroupName",
                    //onValueChanged: function(args) {
                    //    var value = args.value;
                    //    $('#groupsGrid').dxDataGrid('instance').selectRows(value, false);
                    //},
                    contentTemplate: function(e) {
                        var value = e.component.option("value"),
                            $dataGrid = $("<div>").dxDataGrid({
                                dataSource: e.component.getDataSource(),
                                columns: [
                                    { dataField: "GroupID", visible: false },
                                    //{
                                    //    dataField: "DayOfWeek",
                                    //    dataType: "date",
                                    //    format: "dayOfWeek",
                                    //    caption: "День недели проведения занятий",
                                    //    groupIndex: 0
                                    //},
                                    { dataField: "GroupName", caption: "Группа" },
                                    { dataField: "SubjectName", caption: "Предмет" },
                                    { dataField: "Class", caption: "Класс" },
                                    { dataField: "Duration", caption: "Кол-во часов" },
                                    { dataField: "Adress", caption: "Место проведения занятий" }
                                ],
                                columnAutoWidth: true,
                                showRowLines: true,
                                hoverStateEnabled: true,
                                paging: { enabled: true, pageSize: 10 },
                                filterRow: { visible: true },
                                scrolling: { mode: "virtual" },
                                height: 345,
                                headerFilter: { visible: true },
                                selection: { mode: "multiple" },
                                selectedRowKeys: value,
                                onSelectionChanged: function(selectedItems) {
                                    var keys = selectedItems.selectedRowKeys;
                                    e.component.option("value", keys);
                                    e.value = keys;
                                },
                                onContentReady: function (e) {
                                    e.component.option("loadPanel.enabled", false);
                                }
                            });

                        var dataGrid = $dataGrid.dxDataGrid("instance");

                        e.component.on("valueChanged",
                            function(args) {
                                var value = args.value;
                                dataGrid.selectRows(value, false);
                            });

                        return $dataGrid;
                    }
                }
            },
            {
                dataField: "ForStudents",
                label: { text: "Отправить слушателям" },
                editorType: "dxSwitch",
                editorOptions: { switchedOnText: "да", switchedOffText: "нет" }
            },
            {
                dataField: "ForTeachers",
                label: { text: "Отправить преподавателям" },
                editorType: "dxSwitch",
                editorOptions: { switchedOnText: "да", switchedOffText: "нет" }
            },
            { dataField: "Title", editorType: "dxTextBox", label: { text: "Тема" }, isRequired: true, colSpan: 2 },
            {
                dataField: "Body",
                label: { text: "Текст" },
                colSpan: 2,
                isRequired: true,
                editorType: "dxHtmlEditor",
                editorOptions: {
                    height: 300,
                    valueType: "html",
                    toolbar: {
                        items: [
                            "undo", "redo", "separator", 
                            {
                                formatName: "size",
                                formatValues: ["8pt", "10pt", "12pt", "14pt", "18pt", "24pt", "36pt"]
                            },
                            {
                                formatName: "font",
                                formatValues: [
                                    "Arial", "Courier New", "Georgia", "Impact", "Lucida Console", "Tahoma",
                                    "Times New Roman", "Verdana"
                                ]
                            },
                            "separator", "bold", "italic", "strike", "underline", "separator",
                            "alignLeft", "alignCenter", "alignRight", "alignJustify", "separator",
                            "orderedList", "bulletList", "separator",
                            {
                                formatName: "header",
                                formatValues: [false, 1, 2, 3, 4, 5]
                            }, "separator",
                            "color", "background", "separator",
                            "link", "separator",
                            "clear", "separator"
                        ]
                    },
                    onValueChanged: function(e) {
                        var form = $('#dxMailForm').dxForm('instance');
                        form.updateData("Body", e.component.option("value"));
                    }
                }
            },
            {
                itemType: "button",
                horizontalAlignment: "left",
                buttonOptions: {
                    text: "Отправить",
                    type: "success",
                    useSubmitBehavior: true
                }
            }
        ]
    });
});




var htmlEditorTemplate = function() {
    var div = $("<div>").dxHtmlEditor({
        height: 300,
        valueType: "html",
        toolbar: {
            items: [
                "undo", "redo", "separator",
                {
                    name: "size",
                    acceptedValues: ["8pt", "10pt", "12pt", "14pt", "18pt", "24pt", "36pt"]
                },
                {
                    name: "font",
                    acceptedValues: ["Arial", "Courier New", "Georgia", "Impact", "Lucida Console", "Tahoma", "Times New Roman", "Verdana"]
                },
                "separator", "bold", "italic", "strike", "underline", "separator",
                "alignLeft", "alignCenter", "alignRight", "alignJustify", "separator",
                "orderedList", "bulletList", "separator",
                {
                    name: "header",
                    acceptedValues: [false, 1, 2, 3, 4, 5]
                }, "separator",
                "color", "background", "separator",
                "link", "image", "separator",
                "clear", "codeBlock", "blockquote", "separator",
                "insertTable", "deleteTable",
                "insertRowAbove", "insertRowBelow", "deleteRow",
                "insertColumnLeft", "insertColumnRight", "deleteColumn"
            ]
        },
        onValueChanged: function (e) {
            var form = $('#dxMailForm').dxForm('instance');
            form.updateData("Body", e.component.option("value"));
        }
    });

    return div;
}



