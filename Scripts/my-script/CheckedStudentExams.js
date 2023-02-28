var openTestResult = function(rowData) {
    var stId = rowData.row.data.StudentExamID;
    window.open("/StudentExam/ViewResultOfStudentExam/?studentExamId=" + stId, "_blank");
}