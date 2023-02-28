$(function() {
    $('#buttonDTT').dxButton({
        type: "default",
        stylingMode: "outlined",
        text: "Запишись на ДТТ",
        height: 150,
        elementAttr: {
            class: "col-5",
            style: "/*border-radius:20px*/; font-size: 35px; font-weight: 500;"
        },
        onClick: function () { window.location.href = '/Order/NewUserOrderDtt'; }
    });
});


$(function() {
    $('#buttonCourse').dxButton({
        type: "default",
        stylingMode: "outlined",
        text: "Запишись на курсы",
        height: 150,
        elementAttr: {
            class: "col-5",
            style: "/*border-radius:20px;*/ font-size: 35px; font-weight: 500;"
        },
        onClick: function () { window.location.href = '/Order/NewUserOrderKpc'; }
    });
});

$(function() {
    $('#headingOne').dxButton({
        type: "default",
        stylingMode: "outlined",
        text: "Как будет проводиться ДТТ?",
        elementAttr: {
            class: "card-header btn btn-outline-primary",
            style: "color:#2e5188; font-size: 22px; text-align:left"
        }
    });
});

$(function() {
    $('#headingTwo').dxButton({
        type: "default",
        stylingMode: "outlined",
        text: "Сколько времени дается на решение ДТТ в онлайн формате?",
        elementAttr: {
            class: "card-header btn btn-outline-primary",
            style: "color:#2e5188; font-size: 22px; text-align:left"
        }
    });
});

$(function() {
    $('#headingThree').dxButton({
        type: "default",
        stylingMode: "outlined",
        text: "Как долго ждать результатов ДТТ?",
        elementAttr: {
            class: "card-header btn btn-outline-primary",
            style: "color:#2e5188; font-size: 22px; text-align:left"
        }
    });
});

$(function() {
    $('#headingFour').dxButton({
        type: "default",
        stylingMode: "outlined",
        text: "Чем можно пользоваться при решении?",
        elementAttr: {
            class: "card-header btn btn-outline-primary",
            style: "color:#2e5188; font-size: 22px; text-align:left"
        }
    });
});

$(function() {
    $('#headingFive').dxButton({
        type: "default",
        stylingMode: "outlined",
        text: "Сколько ждать проверку?",
        elementAttr: {
            class: "card-header btn btn-outline-primary",
            style: "color:#2e5188; font-size: 22px; text-align:left"
        }
    });
});

//$(function() {
//    $('#headingSix').dxButton({
//        type: "default",
//        stylingMode: "outlined",
//        text: "Будет ли разбор ошибок?",
//        elementAttr: {
//            class: "card-header btn btn-outline-primary",
//            style: "color:#2e5188; font-size: 22px; text-align:left"
//        }
//    });
//});

$(function() {
    $('#headingSeven').dxButton({
        type: "default",
        stylingMode: "outlined",
        text: "План действий",
        elementAttr: {
            class: "card-header btn btn-outline-primary",
            style: "color:#2e5188; font-size: 22px; text-align:left"
        }
    });
});

$(function() {
    $('#headingEight').dxButton({
        type: "default",
        stylingMode: "outlined",
        text: "Очный формат",
        elementAttr: {
            class: "card-header btn btn-outline-primary",
            style: "color:#2e5188; font-size: 18px; text-align:left"
        }
    });
});

$(function() {
    $('#headingNine').dxButton({
        type: "default",
        stylingMode: "outlined",
        text: "Онлайн формат",
        elementAttr: {
            class: "card-header btn btn-outline-primary",
            style: "color:#2e5188; font-size: 18px; text-align:left"
        }
    });
});



//var markerUrl = "https://js.devexpress.com/Demos/RealtorApp/images/map-marker.png";
var markerUrl = "https://checkin.bohemia.bg/images/icons/pin00@2x.png";

function checkBox_onValueChanged(data) {
    var mapWidget = $("#map").dxMap("instance");
    var markersData = mapWidget.option("markers");
    var newMarkers = $.map(markersData, function (item) {
        return $.extend(true, {}, item, { tooltip: { isShown: false } });
    });
    mapWidget.option("markers", newMarkers);
    mapWidget.option("markerIconSrc", data.value ? markerUrl : null);
}

function button_onClick() {
    var mapWidget = $("#map").dxMap("instance");
    var markersData = mapWidget.option("markers");
    var newMarkers = $.map(markersData, function (item) {
        return $.extend(true, {}, item, { tooltip: { isShown: true } });
    });
    mapWidget.option("markers", newMarkers);
}

$(function() {
    var url = "/api/Groups/";
    $("#courseScheduler").dxScheduler({
        timeZone: "Europe/Moscow",
        dataSource: DevExpress.data.AspNet.createStore({
            key: "GroupID",
            loudUrl: url + "GetGroupsForScheduler"
        }),
        views: ["week", "month"],
        currentView: "month",
        textExpr: "GroupName",
        startDateExpr: "StartDateTime",
        endDateExpr: "StartDateTime"

    }).dxScheduler("instance");
});
$(function () {
    var url = "/api/Groups/";
    $("#courseScheduler2").dxScheduler({
        timeZone: "Europe/Moscow",
        dataSource: DevExpress.data.AspNet.createStore({
            key: "GroupID",
            loudUrl: url + "GetGroupsForScheduler"
        }),
        views: ["week", "month"],
        currentView: "month",
        textExpr: "GroupName",
        startDateExpr: "StartDateTime",
        endDateExpr: "StartDateTime"

    }).dxScheduler("instance");
});


//$(function() {
//    var url = "/Api/Groups/";
//    $("#courseScheduler").dxPivotGrid({
//        allowSorting: true,
//        allowSortingBySummary: true,
//        allowFiltering: true,
//        height: 620,
//        showBorders: true,
//        rowHeaderLayout: "tree",
//        scrolling: {
//            mode: "virtual"
//        },
//        dataSource: {
//            remoteOperations: true,
//            store: DevExpress.data.AspNet.createStore({
//                key: "GroupID",
//                loudUrl: url + "GetGroupsForScheduler"
//            }),
//            fields: [
//                {
//                    caption: "День недели",
//                    dataField: "Day",
//                    area: "column"
//                },
//                {
//                    caption: "Предмет",
//                    dataField: "Subject",
//                    area: "row"
//                },
//                {
//                    dataField: "GroupName",
//                    area: "data"
//                }
//                ]
//        }
//    });
//});

var expandFirstRow = function(e) {
    e.component.expandRow(e.component.getKeyByRowIndex(0));
}