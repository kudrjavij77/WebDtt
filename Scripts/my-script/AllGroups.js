function teacherDisplayExpr(container, options) {
    var count = options.value.length;
    var i = 0;
    var str = "";
    while (i < count) {
        if (options.value[i].PersonTypeID === 2) str = str + options.value[i].ViewShortFio + "; ";
        i++;
    }
    container.text(str);
    //str.appendTo(container);
}

function donateTypesDisplayExpr(container, options) {
    var count = options.value.length;
    var i = 0;
    var str = "";
    while (i < count) {
        var s = "";
        var donateCount = options.value[i].DonateInfos.length;
        var j = 0;
        while (j < donateCount) {
            if (donateCount === 1) s = options.value[i].DonateInfos[j].Total;
            else s = s + "/" + options.value[i].DonateInfos[j].Total;
            j++;
        }
        str = str + options.value[i].FullTotal + " (" + s +")" + "; ";
        i++;
    }
    container.text(str);
}

var onBeforeSend = function (e, option) {
    if (e === "insert") {
        var a = 1;
    }
    if (e === "update") {
        var b = 1;
    }

    
}

var onChangedTeachers = function(e) {
    var $dataGrid = $("#gridTeachers");

    if ($dataGrid.length) {
        var dataGrid = $dataGrid.dxDataGrid("instance");
        dataGrid.selectRows(e.value, false);
    }
}

var addNewTeacher = function (e, option) {
    if (e === "insert") {
        //option.url += "?type=2";
        var newValues = option.data.values.slice(0, 1) + '\"PersonTypeID\":\"2\",' + option.data.values.slice(1);
        option.data.values = newValues;
    }
    var a = 1;
}

var toolbarGroup = function(e) {
    e.toolbarOptions.items.push({
        widget: "dxButton",
        options: {
            icon: "export",
            text: "Ведомость учета посещаемости",
            type: "normal",
            stylingMode: "outlined",
            onClick: function () {
                var dataGrid = $("#grid").dxDataGrid("instance");
                var rows = dataGrid.getSelectedRowsData();

                for (var i = 0; i < rows.length; i++) {
                    window.open('/Report/AttendanceStatement/?id=' + rows[i].GroupID, '_blank');
                }

                //var progressBar = $('#alerts').dxProgressBar({
                //    max: rows.length,
                //    onComplete: function (e) {
                //        dataGrid.deselectAll();
                //        e.component.dispose();
                //        dataGrid.refresh();
                //    }
                //}).dxProgressBar('instance');
                //$.when.apply($,
                //    rows.map(function (data) {
                //        $.ajax({
                //            url: '/Report/AttendanceStatement/' + data.GroupID,
                //            type: 'POST',
                //            success: function (s) {
                //                progressBar.option('value', progressBar.option('value') + 1);
                //            }
                //        });
                //    })).done(function () {
                //});
            }
        },
        location: "after"
    });
}