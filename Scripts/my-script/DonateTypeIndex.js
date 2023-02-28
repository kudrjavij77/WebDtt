function donateInfoDisplayExpr(container, options) {
    var count = options.value.length;
    var i = 0;
    var str = "";
    var d = $('#gridDonateTypes').dxDataGrid('instance');
    while (i < count) {
        str = str + options.value[i].Total + "(этап " + options.value[i].DonateNumber + ")" + "/ ";
        i++;
    }
    container.text(str);
};
