$(function () {
    //setup ajax error handling
    $.ajaxSetup({
        error: function (x) {
            var exception;
            if (x.responseJSON) {
                exception = x.responseJSON;
            } else {
                exception = x;
            }
                
			var message, stackTrace, type;
            if (exception.ExceptionMessage) {
                message = exception.ExceptionMessage;
                stackTrace = exception.StackTrace;
                type = exception.ExceptionType;
            } else if (x.status) {
                if (x.status === 200) {
                    return;
                }
                message = exception.statusText;
                type = exception.status;
                stackTrace = exception.responseText;
            }
            if(!message)
			{
				if(exception.MessageDetail) {
					message = exception.MessageDetail;
					stackTrace = exception.Message;
				} else {
					message = exception.Message;
					stackTrace = "";
				}				
                type = x.status + ': ' + x.statusText;
            }
            message = message.replace(/(?:\r\n|\r|\n)/g, '<br>');
            var popup = null;
            var popupOptions = {
                elementAttr: {
                    class: "dx-ais-exception"
                },
                minWidth: 600,
                width: 800,
                height: 400,
                position: {
                    my: "center",
                    at: "center"
                },
                contentTemplate: function() {
                    return $("<div />").append(
                        $("<h5>" + message + "</h5>"),
                        $(),
                        $("<div id='stackTrace'></div>")
                    );
                },
                toolbarItems: [
                    {
                        disabled: false,
                        location: "center",
                        text:"Ошибка!"
                    }
                ]
            };
            popup = $("#exception").dxPopup(popupOptions).dxPopup("instance");
            popup.show();

            $("#stackTrace").html("<p>" + type + "</p>" + stackTrace);
            $("#stackTrace").dxScrollView({
                scrollByContent: true,
                scrollByThumb: true,
                showScrollbar: "always",
                height: 200
            }).dxScrollView("instance");
        }
    });
});