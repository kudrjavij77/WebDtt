var unVisibilityZipDownloader = function (e) {
    var f = e.component._options._optionManager._options.flags;
    if (f > 0 && f!=128) {
        e.component._options._optionManager._options.visible = true;
    }
}

