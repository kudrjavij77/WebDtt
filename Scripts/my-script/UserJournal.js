$(function() {
    $('#userLessonsGrid').dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            loadUrl: '/Api/Journals/GetLessonsForUser',
            key: "KeyID"
        }),
        showBorders: true,
        showRowLines: true,
        columnAutoWidth: true,
        wordWrapEnabled: true,
        headerFilter: {
            visible: true
        },
        grouping: {
            allowCollapsing: true,
            autoExpandAll: false,
            expandMode: "rowClick"
        },
        columns: [
            {
                dataField: "LessonDate",
                dataType: "datetime",
                format: "longDateLongTime",
                sortOrder: "desc",
                caption: "Дата занятия",
                groupIndex: 0
            },
            { dataField: "FIO", caption: "Слушатель" },
            { dataField: "SubjectName", caption: "Предмет" },
            { dataField: "GroupName", caption: "Группа" },
            { dataField: "StationAddress", caption: "Место проведения"}
        ],
        masterDetail: {
            enabled: true,
            template: function(container, options) {
                var lessonId = options.data.LessonID;
                var personGroupId = options.data.PersonGroupID;
                var groupId = options.data.GroupID;

                $('<h5>').text("Оценки").appendTo(container);
                $('<div>').dxDataGrid({
                    dataSource: DevExpress.data.AspNet.createStore({
                        loadUrl: '/api/Journals/GetJournalFieldValuesForPersonGroup?lessonId=',
                        key: "JournalFieldID",
                        onBeforeSend: function(method, ajaxOptions) {
                            if (method !== "load") return;
                            ajaxOptions.url += lessonId + '&personGroupId=' + personGroupId;
                        }
                    }),
                    columnAutoWidth: true,
                    columns: [
                        { dataField: "FieldName", caption: "Параметр" },
                        { dataField: "FieldDescription", caption: "Описание" },
                        {
                            dataField: "Value",
                            caption: "Значение",
                            alignment: "center",
                            cellTemplate: function(container, options) {
                                if (options.data.Value === null) return;
                                if (options.data.MaxValue === 1) {
                                    $('<div>').dxCheckBox({
                                        value: options.data.Value.Value ? true : false,
                                        readOnly: true
                                    }).appendTo(container);
                                } else {
                                    $('<div>').text(options.data.Value.Value).css("text-align", "center")
                                        .appendTo(container);
                                }
                            }
                        }
                    ]
                }).appendTo(container);
                $('<hr>').appendTo(container);

                var accordion = $(`<div id="accordionl${lessonId}pg${personGroupId}">`).css("display", "block");

                //add CARD LessonMaterials of Student
                
                var topicCard = $('<div class="card">');
                        $(`<div id="headingFourl${lessonId}pg${personGroupId}" data-toggle="collapse" data-target="#collapseFourl${lessonId}pg${personGroupId}" aria-expanded="true" aria-controls="collapseFourl${lessonId}pg${personGroupId}">`)
                            .dxButton({
                                type: "default",
                                stylingMode: "outlined",
                                text: "Материалы слушателя (файлы)",
                                elementAttr: {
                                    class: "card-header btn btn-outline-primary",
                                    style: "color:#2e5188; font-size: 16px; text-align:left"
                                }
                            }).appendTo(topicCard);

                        $(`<div id="collapseFourl${lessonId}pg${personGroupId}" class="collapse" style="color: #2e5188" aria-labelledby="headingFourl${lessonId}pg${personGroupId}" data-parent="#accordionl${lessonId}pg${personGroupId}">`)
                            .append($('<div class="card-body">')
                                .append($(`<div id="uploaderl${lessonId}pg${personGroupId}">`).dxFileUploader({
                                    selectButtonText: "Нажмите, чтобы прикрепить",
                                    multiple: true,
                                    accept: "*",
                                    name: "myFile",
                                    hint: "Название файла не должно содержать специальных символов (\\ '' / : * ? < > | )",
                                    value: [],
                                    uploadMode: "instantly",
                                    uploadUrl: "/Journal/UploadLessonMaterial",
                                    onValueChanged: function (e) {
                                        e.component.option('uploadUrl', '/Journal/UploadLessonMaterial?lessonId=' + lessonId + '&personGroupId=' + personGroupId);
                                    },
                                    onUploadError: function processResult(result) {
                                        alert(result.request.statusText);
                                    },
                                    onUploaded: function () {
                                        $(`#studentMaterialsl${lessonId}pg${personGroupId}`).dxDataGrid('instance').refresh();
                                    }
                                }))
                                .append($(`<div id="studentMaterialsl${lessonId}pg${personGroupId}">`).dxDataGrid({
                                    dataSource: DevExpress.data.AspNet.createStore({
                                        loadUrl: '/api/Journals/LessonMaterials?groupId=',
                                        deleteUrl: '/api/Journals/DeleteLessonMaterial',
                                        key: "LessonMaterialID",
                                        onBeforeSend: function (method, ajaxOptions) {
                                            if (method !== 'load') return;
                                            ajaxOptions.url += groupId + '&lessonId=' + lessonId + '&personGroupId=' + personGroupId;
                                        }
                                    }),
                                    showRowLines: true,
                                    editing: { allowDeleting: true, useIcons: true },
                                columns: [
                                    { dataField: "LessonMaterialID", visible:false },
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
                            }))).appendTo(topicCard);

                        topicCard.appendTo(accordion);

                        //add CARD Topics of Lesson
                $.get(`/api/Journals/Topics?groupId=${groupId}&lessonId=${lessonId}`,
                    function(data) {
                        if (data.length === 0) return;
                        var topicCard = $('<div class="card">');
                        $(`<div id="headingOnel${lessonId}pg${personGroupId}" data-toggle="collapse" data-target="#collapseOnel${lessonId}pg${personGroupId}" aria-expanded="true" aria-controls="collapseOnel${lessonId}pg${personGroupId}">`)
                            .dxButton({
                                type: "default",
                                stylingMode: "outlined",
                                text: "Темы занятия",
                                elementAttr: {
                                    class: "card-header btn btn-outline-primary",
                                    style: "color:#2e5188; font-size: 16px; text-align:left"
                                }
                            }).appendTo(topicCard);

                        $(`<div id="collapseOnel${lessonId}pg${personGroupId}" class="collapse" style="color: #2e5188" aria-labelledby="headingOnel${lessonId}pg${personGroupId}" data-parent="#accordionl${lessonId}pg${personGroupId}">`)
                            .append($('<div class="card-body">').append($('<div>').dxDataGrid({
                                dataSource: data,
                                showRowLines: true,
                                columns: [
                                    { dataField: "TopicName", caption: "Название темы" },
                                    { dataField: "Description", caption: "Описание" }
                                ]
                            }))).appendTo(topicCard);

                        topicCard.appendTo(accordion);
                    });

                //add CARD LessonMaterials of Teachers
                $.get(`/api/Journals/LessonMaterials?groupId=${groupId}&lessonId=${lessonId}&personGroupId=${null}`,
                    function (data) {
                        if (data.length === 0) return;
                        var topicCard = $('<div class="card">');
                        $(`<div id="headingTwol${lessonId}pg${personGroupId}" data-toggle="collapse" data-target="#collapseTwol${lessonId}pg${personGroupId}" aria-expanded="true" aria-controls="collapseTwol${lessonId}pg${personGroupId}">`)
                            .dxButton({
                                type: "default",
                                stylingMode: "outlined",
                                text: "Материалы преподавателя (файлы)",
                                elementAttr: {
                                    class: "card-header btn btn-outline-primary",
                                    style: "color:#2e5188; font-size: 16px; text-align:left"
                                }
                            }).appendTo(topicCard);

                        $(`<div id="collapseTwol${lessonId}pg${personGroupId}" class="collapse" style="color: #2e5188" aria-labelledby="headingTwol${lessonId}pg${personGroupId}" data-parent="#accordionl${lessonId}pg${personGroupId}">`)
                            .append($('<div class="card-body">').append($('<div>').dxDataGrid({
                                dataSource: data,
                                showRowLines: true,
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
                                            }
                                        ]
                                    }
                                ]
                            }))).appendTo(topicCard);

                        topicCard.appendTo(accordion);
                    });

                //add CARD LessonAddons of Teachers
                $.get(`/api/Journals/LessonAddons?lessonId=${lessonId}`,
                    function (data) {
                        if (data.length === 0) return;
                        var topicCard = $('<div class="card">');
                        $(`<div id="headingThreel${lessonId}pg${personGroupId}" data-toggle="collapse" data-target="#collapseThreel${lessonId}pg${personGroupId}" aria-expanded="true" aria-controls="collapseThreel${lessonId}pg${personGroupId}">`)
                            .dxButton({
                                type: "default",
                                stylingMode: "outlined",
                                text: "Материалы преподавателя (ссылки)",
                                elementAttr: {
                                    class: "card-header btn btn-outline-primary",
                                    style: "color:#2e5188; font-size: 16px; text-align:left"
                                }
                            }).appendTo(topicCard);

                        $(`<div id="collapseThreel${lessonId}pg${personGroupId}" class="collapse" style="color: #2e5188" aria-labelledby="headingThreel${lessonId}pg${personGroupId}" data-parent="#accordionl${lessonId}pg${personGroupId}">`)
                            .append($('<div class="card-body">').append($('<div>').dxDataGrid({
                                dataSource: data,
                                showRowLines: true,
                                columns: [
                                    { dataField: "AddonShortName", caption: "Краткое название" },
                                    { dataField: "AddonDescription", caption: "Описание" },
                                    {
                                        dataField: "LinkAddress", caption: "Ссылка",
                                        cellTemplate: function (container, options) {
                                            $("<div />").dxButton({
                                                icon: "link",
                                                stylingMode: "text",
                                                type: "default",
                                                text: "Перейти по ссылке",
                                                onClick: function () {
                                                    window.open(options.value, "_blank");
                                                }
                                            }).appendTo(container);
                                        }
                                    }
                                ]
                            }))).appendTo(topicCard);

                        topicCard.appendTo(accordion);
                    });



                accordion.appendTo(container);
            }
        }
    }).dxDataGrid('instance');
});