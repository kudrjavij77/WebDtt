function getFlagsDataSource(objectName) {
    var store = new DevExpress.data.CustomStore({
        loadMode: "raw",
        key: "value",
        cacheRawData: true,
        load: function () {
            $.getJSON("/Models/Flags/Flags.json");
        },
        loadOptions: {
            filter: ["class", "=", objectName],
            select: "flags"
        }
    });
    return store;
}