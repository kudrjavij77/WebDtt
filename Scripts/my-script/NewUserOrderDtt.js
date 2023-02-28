//var form,
//    employee,
//    isFirstLoad = true;

//function onFormInitialized(e) {
//    form = e.component;
//}

//function onFormReady(e) {
//    if (isFirstLoad) {
//        employee = e.component.option("formData");
//        isFirstLoad = false;
//    }
//}

//function studentChange(e) {
//    var dxStudentsLookup = $("#studentsLookup").dxLookup('instance');
//    var dxDelegatesLookup = $("#delegatesLookup").dxLookup('instance');
//}

//function gridBox_valueChanged(e) {
//    var $dataGrid = $("#gridExams");

//    if ($dataGrid.length) {
//        var dataGrid = $dataGrid.dxDataGrid("instance");
//        dataGrid.selectRows(e.value, false);
//    }
//}

var initializedExamType = function(e) {

}

var addNewStudent = function() {
    $('#gridStudents').dxDataGrid('instance').addRow();
}

var addNewDelegate = function () {
    $('#gridDelegates').dxDataGrid('instance').addRow();
}


function onChangedStudent(e) {
    var exams = $('#dx_Exams').dxTagBox('instance');
    generatedFilters();
    exams.reset();

    $('#dx_StudentBox').dxDropDownBox('instance').close();
}

function onChangedDelegate(e) {
    $('#dx_CreatorBox').dxDropDownBox('instance').close();
}


function onChangedExamType(e) {
    var exams = $('#dx_Exams').dxTagBox('instance');
    generatedFilters();
    exams.reset();
}

function generatedFilters() {
    var examType = $('#examType').dxLookup('instance');
    var ddbStudents = $('#dx_StudentBox').dxDropDownBox('instance');
    var tg = $("#dx_Exams").dxTagBox('instance');
    var filters = [];

    
    if (ddbStudents._options._optionManager._options.value) {
        var f1 = ['Class', '=', $('#gridStudents').dxDataGrid('instance').getSelectedRowsData()[0].ParticipantClass];
        filters.push(f1);
        filters.push("and");
    }
    if (examType._options._optionManager._options.selectedItem) {
        var f2 = ['ExamTypeID', '=', examType._options._optionManager._options.selectedItem.ExamTypeID];
        filters.push(f2);
        filters.push("and");
    }
    if (ddbStudents._options._optionManager._options.value) {
        $('#gridStudents').dxDataGrid('instance').getSelectedRowsData()[0]
            .StudentExams.forEach(function(item, index, array) {
                var f3;
                if ((item.Flags < 4)) {
                    //ExamID измено с ExamId?????? влияет ли на что??????
                    f3 = ['!', ['ExamID', '=', item.ExamID]];
                    filters.push(f3);
                    filters.push("and");

                    if ((item.Exam.ExamTypeID === 1) &&
                        examType._options._optionManager._options.selectedItem.ExamTypeID === 1) {

                        //убирать из списка экзамены с выбранными предметами
                        f3 = ['!', ['SubjectID', '=', item.Exam.SubjectID]];
                        filters.push(f3);
                        filters.push("and");

                        //убирать из списка экзамены в выбранными дату
                        f3 = ['!', ['TestDateTime', '=', item.Exam.TestDateTime]];
                        filters.push(f3);
                        filters.push("and");
                    }
                }


            });
    }

    //Если надо зарегать после начала теста, следующие 3 строчки закоментить
        var f4 = ['TestDateTime', '>', new Date()];
        filters.push(f4);
        filters.push("and");
    //
    tg._dataSource.filter(filters);
    tg._dataSource.reload();
}

function onChangedExams() {
    
    var tg = $("#dx_Exams").dxTagBox('instance');
    var allItems = tg.option("items");
    var lst = tg._$list.dxList('instance');
    
    for (var k = 0; k < allItems.length; k++) {
        lst.itemElements()[k].hidden = false;
    }

    for (var i = 0; i < lst.itemElements().length; i++) {
        var b1 = lst.itemElements()[i].ariaSelected === "true";
        var b3 = lst.itemElements()[i].className.indexOf("dx-list-item-selected") > 0;
        var b2 = allItems[i].ExamTypeID === 1;
        if ((b1||b3) & b2) {
            for (var j = 0; j < allItems.length; j++) {
                var boolean3 = allItems[j].ExamID !== allItems[i].ExamID;
                var boolean4 = allItems[j].TestDateTime === allItems[i].TestDateTime;
                var boolean5 = allItems[j].ExamTypeID === 1;
                if (boolean3 & boolean4 & boolean5) lst.itemElements()[j].hidden = true;
            }
        }
    };

}
