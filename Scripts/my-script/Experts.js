function subjectDisplayExpr(container, options) {
    var count = options.value.length;
    var i = 0;
    var str = "";
    while (i < count) {
        str = str + options.value[i].SubjectViewName + "; ";
        
        i++;
    }
    container.text(str);
    //str.appendTo(container);
}

var addNewTeacher = function(e, option) {
    if (e === "insert") {
        //option.url += "?type=2";
        var newValues = option.data.values.slice(0, 1) + '\"PersonTypeID\":\"2\",' + option.data.values.slice(1);
        option.data.values = newValues;
    }
    var a = 1;
}

var addNewExpert = function (e, option) {
    if (e === "insert") {
        var newValues = option.data.values.slice(0, 1) + '\"PersonTypeID\":\"4\",' + option.data.values.slice(1);
        option.data.values = newValues;
    }
}