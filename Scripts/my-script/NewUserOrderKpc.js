var addNewStudent = function () {
    $('#gridStudents').dxDataGrid('instance').addRow();
}
var addNewDelegate = function () {
    $('#gridDelegates').dxDataGrid('instance').addRow();
}
function onChangedStudent(e) {
    generatedFilters();
    $('#dx_StudentBox').dxDropDownBox('instance').close();
}
function onChangedDelegate(e) {
    $('#dx_CreatorBox').dxDropDownBox('instance').close();
}

function onChangedEducationForm(e) {
    generatedFilters();
}

function onChangedSubjects(e) {
    generatedFilters();
}

function onChangedGroup() {
    var d = $('#dx_DonateTypesBox').dxLookup('instance');
    var groupBox = $('#dx_GroupBox').dxDropDownBox('instance');
    if (groupBox._options._optionManager._options.value) {
        //var groupData = $('#groupsGrid').dxDataGrid('instance').getSelectedRowsData()[0];
        //var today = new Date();
        //var donTypes;
        //groupData.DonateTypes.forEach(item => {
        //    if (groupData.StartDateTime < today && today )

        //});


        d._options.option("dataSource", $('#groupsGrid').dxDataGrid('instance').getSelectedRowsData()[0].DonateTypes);
        
    } else {
        d._options.option("dataSource", null);
    }
    d.reset();
    $('#dx_GroupBox').dxDropDownBox('instance').close();
}


function switchMomValueChanged(e) {
    var d = $('#dx_DonateTypesBox').dxLookup('instance');
    d.option("disabled", e.value);
    d.option("isRequired", !e.value);
}


function generatedFilters() {
    var studentBox = $('#dx_StudentBox').dxDropDownBox('instance');
    var eduFormBox = $('#dx_EducationFormBox').dxLookup('instance');
    var subjectBox = $('#dx_SubjectsBox').dxLookup('instance');
    var groupBox = $('#dx_GroupBox').dxDropDownBox('instance');
    var donateBox = $('#dx_DonateTypesBox').dxLookup('instance');
    var filters = [];
    filters.push(['FreePlaces', '>', 0]);
    filters.push('and');

    if (studentBox._options._optionManager._options.value) {
        var f1 = ['Class', '=', $('#gridStudents').dxDataGrid('instance').getSelectedRowsData()[0].ParticipantClass];
        filters.push(f1);
        filters.push("and");
        $('#gridStudents').dxDataGrid('instance').getSelectedRowsData()[0]
            .PersonGroups.forEach(function (item, index, array) {
                var f11;
                if (item.Flags < 4) {
                    f11 = ['!', ['GroupID', '=', item.GroupID]];
                    filters.push(f11);
                    filters.push('and');

                    ////TODO:фильтр, который не показывает группы в день, в который уже есть действующие
                    //var date = new Date(item.Group.StartDateTime);
                    //f11 = ['!', ['DayOfWeek', '=', date.getDay()]];
                    //filters.push(f11);
                    //filters.push('and');
                }
            });

    }

    if (eduFormBox._options._optionManager._options.value) {
        var f2 = ['EducationFormID', '=', eduFormBox._options._optionManager._options.selectedItem.EducationFormID];
        filters.push(f2);
        filters.push("and");
    }

    if (subjectBox._options._optionManager._options.value) {
        var f3 = ['Subject', '=', subjectBox._options._optionManager._options.selectedItem.SubjectID];
        filters.push(f3);
        filters.push("and");
    }

    groupBox._dataSource.filter(filters);
    groupBox._dataSource.reload();
    //groupGrid._options.option('dataSource', groupBox.getDataSource().items());
    onChangedGroup();
    groupBox.reset();
    donateBox.reset();

}

function groupsGridDisplayValues(e, op, data) {
    var a;
    a = 1;
    var studentBox = $('#dx_StudentBox').dxDropDownBox('instance');
    var groups = $('#groupsGrid').dxDataGrid('instance');

    if (studentBox._options._optionManager._options.value) {
        $('#gridStudents').dxDataGrid('instance').getSelectedRowsData()[0]
            .PersonGroups.forEach(function(item, index, array) {
                if (item.Flags < 4) {
                    groups.getDataSource().items().forEach(function(it, index, array) {
                        var date1 = new Date(item.Group.StartDateTime);
                        var date2 = new Date(it.StartDateTime);
                        if (date1.getDay() === date2.getDay()) {
                            it.visible = false;
                        }
                    });
                }
            });
    }
}

