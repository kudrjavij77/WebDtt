var OnToolbarPreparing = function (e) {
    e.toolbarOptions.items.push(
        {
            widget: "dxButton",
            options: {
                icon: "columnchooser",
                text: "Выбор столбцов",
                type: "normal",
                stylingMode: "outlined",
                onClick: function() {
                    $('#dxStudentExamsGrid').dxDataGrid('instance').showColumnChooser();
                }
            },
            location: "after"
        },
        {
            widget: "dxButton",
            options: {
                icon: "clear",
                text: "",
                hint: "Сбросить фильтры",
                type: "danger",
                stylingMode: "outlined",
                onClick: function() {
                    $('#dxStudentExamsGrid').dxDataGrid('instance').clearFilter();
                }
            },
            location: "after"
        },
        {
            widget: "dxButton",
            options: {
                icon: "refresh",
                text: "",
                hint: "Relload data",
                type: "normal",
                stylingMode: "outlined",
                onClick: function() {
                    $('#dxStudentExamsGrid').dxDataGrid('instance').refresh();
                }
            },
            location: "after"
        },
        {
            widget: "dxButton",
            options: {
                id: "attachmentStudentExams",
                icon: "home",
                text: "Прикрепить к ППЭ",
                hint: "Прикрепить участника экзамена к ППЭ назначенному на экзамен по приоритету рассадки",
                type: "default",
                stylingMode: "outlined",
                onClick: function() {
                    var dataGrid = $("#dxStudentExamsGrid").dxDataGrid("instance");
                    var rows = dataGrid.getSelectedRowsData();
                    var progressBar = $('#alerts').dxProgressBar({
                        max: rows.length,
                        onComplete: function (e) {
                            dataGrid.deselectAll();
                            e.component.dispose();
                            dataGrid.refresh();
                        }
                    }).dxProgressBar('instance');
                    $.when.apply($,
                        rows.map(function (data) {
                            $.ajax({
                                url: '/StudentExam/AttachStudentExamToStationExam/?studentExamId=' + data.StudentExamID,
                                type: 'POST',
                                success: function (s) {
                                    progressBar.option('value', progressBar.option('value') + 1);
                                }
                            });
                        })).done(function () {
                    });
                }
            },
            location: "after"
        },
        {
            widget: "dxButton",
            options: {
                id: "detachmentStudentExams",
                icon: "home",
                text: "Открепить от ППЭ",
                hint: "Открепить участника экзамена от ППЭ",
                type: "danger",
                stylingMode: "outlined",
                onClick: function () {
                    var dataGrid = $("#dxStudentExamsGrid").dxDataGrid("instance");
                    var rows = dataGrid.getSelectedRowsData();
                    var progressBar = $('#alerts').dxProgressBar({
                        max: rows.length,
                        onComplete: function (e) {
                            dataGrid.deselectAll();
                            e.component.dispose();
                            dataGrid.refresh();
                        }
                    }).dxProgressBar('instance');
                    $.when.apply($,
                        rows.map(function (data) {
                            $.ajax({
                                url: '/StudentExam/DetachStudentExamToStationExam/?studentExamId=' + data.StudentExamID,
                                type: 'POST',
                                success: function (s) {
                                    progressBar.option('value', progressBar.option('value') + 1);
                                }
                            });
                        })).done(function () {
                    });
                }
            },
            location: "after"
        },
        {
            widget: "dxButton",
            options: {
                id: "sendPassOfExamToStudentExam",
                icon: "comment",
                text: "Информация о ППЭ",
                hint: "Отправить информацию о ППЭ на почту участнику",
                type: "normal",
                stylingMode: "outlined",
                onClick: function () {
                    var dataGrid = $("#dxStudentExamsGrid").dxDataGrid("instance");
                    var rows = dataGrid.getSelectedRowsData();
                    var progressBar = $('#alerts').dxProgressBar({
                        max: rows.length,
                        onComplete: function (e) {
                            dataGrid.deselectAll();
                            e.component.dispose();
                            dataGrid.refresh();
                        }
                    }).dxProgressBar('instance');
                    $.when.apply($,
                        rows.map(function (data) {
                            $.ajax({
                                url: '/StudentExam/SendPassOfExamToStudentExam/?studentExamId=' + data.StudentExamID,
                                type: 'POST',
                                success: function (s) {
                                    progressBar.option('value', progressBar.option('value') + 1);
                                }
                            });
                        })).done(function () {
                    });
                }
            },
            location: "after"
        },
        {
            widget: "dxButton",
            options: {
                id: "sendLinkToBroadcastButton",
                icon: "video",
                text: "Вебинар",
                hint: "Отправить ссылку на консультацию",
                type: "normal",
                stylingMode: "outlined",
                onClick: function() {
                    var dataGrid = $("#dxStudentExamsGrid").dxDataGrid("instance");
                    var rows = dataGrid.getSelectedRowsData();
                    var progressBar = $('#alerts').dxProgressBar({
                        max: rows.length,
                        onComplete: function(e) {
                            dataGrid.deselectAll();
                            e.component.dispose();
                            dataGrid.refresh();
                        }
                    }).dxProgressBar('instance');
                    $.when.apply($,
                        rows.map(function(data) {
                            $.ajax({
                                url: '/StudentExam/SendLinkToBroadcast/?studentExamId=' + data.StudentExamID,
                                type: 'POST',
                                success: function(s) {
                                    progressBar.option('value', progressBar.option('value') + 1);
                                }
                            });
                        })).done(function() {
                    });
                }
            },
            location: "after"
        },
        {
            widget: "dxButton",
            options: {
                id: "sendElectronicKimButton",
                icon: "airplane",
                text: "КИМ",
                hint: "Отправить КИМ на почту",
                type: "normal",
                stylingMode: "outlined",
                onClick: function() {
                    var dataGrid = $("#dxStudentExamsGrid").dxDataGrid("instance");
                    var rows = dataGrid.getSelectedRowsData();
                    var progressBar = $('#alerts').dxProgressBar({
                        max: rows.length,
                        onComplete: function(e) {
                            dataGrid.deselectAll();
                            e.component.dispose();
                            dataGrid.refresh();
                        }
                    }).dxProgressBar('instance');
                    $.when.apply($,
                        rows.map(function(data) {
                            //return @*dataGrid.getDataSource().store().remove(data.ID);*@
                            $.ajax({
                                url: '/StudentExam/SendElectronicKim/?studentExamId=' + data.StudentExamID,
                                type: 'POST',
                                success: function(s) {
                                    progressBar.option('value', progressBar.option('value') + 1);
                                }
                            });
                        })).done(function() {
                    });
                }
            },
            location: "after"
        },
        {
            widget: "dxButton",
            options: {
                id: "sendEmailButton",
                icon: "message",
                text: "",
                hint: "Написать",
                type: "normal",
                disabled:true,
                stylingMode: "outlined",
                onClick: function() {
                    var dataGrid = $("#dxStudentExamsGrid").dxDataGrid("instance");
                    var rows = dataGrid.getSelectedRowsData();
                    var progressBar = $('#alerts').dxProgressBar({
                        max: rows.length,
                        onComplete: function(e) {
                            dataGrid.deselectAll();
                            e.component.dispose();
                            dataGrid.refresh();
                        }
                    }).dxProgressBar('instance');
                    $.when.apply($,
                        rows.map(function(data) {
                            //return @*dataGrid.getDataSource().store().remove(data.ID);*@
                            $.ajax({
                                url: '/StudentExam/SendEmail/?studentExamId=' + data.StudentExamID,
                                type: 'POST',
                                success: function(s) {
                                    progressBar.option('value', progressBar.option('value') + 1);
                                }
                            });
                        })).done(function() {});
                }
            },
            location: "after"
        },
        {
            widget: "dxButton",
            options: {
                id: "sendResultTrueButton",
                icon: "check",
                text: "",
                hint: "Проставить флаг РЕЗУЛЬТАТ ДОСТУПЕН",
                type: "success",
                stylingMode: "outlined",
                onClick: function () {
                    var dataGrid = $("#dxStudentExamsGrid").dxDataGrid("instance");
                    var rows = dataGrid.getSelectedRowsData();
                    var progressBar = $('#alerts').dxProgressBar({
                        max: rows.length,
                        onComplete: function (e) {
                            dataGrid.deselectAll();
                            e.component.dispose();
                            dataGrid.refresh();
                        }
                    }).dxProgressBar('instance');
                    $.when.apply($,
                        rows.map(function (data) {
                            $.ajax({
                                url: '/StudentExam/ResultIsTrue/?studentExamId=' + data.StudentExamID,
                                type: 'POST',
                                success: function (s) {
                                    progressBar.option('value', progressBar.option('value') + 1);
                                }
                            });
                        })).done(function () {
                    });
                }
            },
            location: "after"
        }
    );
}

var displayFlags = function(e) {
    var arrFlags = flags2array(e.Flags);
    return arrFlags;
}