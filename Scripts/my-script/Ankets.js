var StartTest = function (e) {
    var test = e.row.data.AnketID;
    switch (test) {
    case 1:
        window.open("/AnyTest/AnketaKpc2021?objectId=" + e.row.data.ObjectAnketID, "_blank");
        break;
    }
}


var OnToolbarPreparing = function(e) {
    e.toolbarOptions.items.push(
        {
            widget: "dxButton",
            options: {
                id: "sendAlarm",
                icon: "comment",
                text: "Оповещение",
                hint: "Отправить оповещение об опросе",
                type: "normal",
                stylingMode: "outlined",
                onClick: function () {
                    var dataGrid = $("#objectAnketsGrid").dxDataGrid("instance");
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
                                url: '/AnyTest/SendMessageAboutAnket/?objectId=' + data.ObjectAnketID,
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

