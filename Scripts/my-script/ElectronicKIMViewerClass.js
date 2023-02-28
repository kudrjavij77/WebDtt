var iconVol = 'data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiA/PjwhRE9DVFlQRSBzdmcgIFBVQkxJQyAnLS8vVzNDLy9EVEQgU1ZHIDEuMS8vRU4nICAnaHR0cDovL3d3dy53My5vcmcvR3JhcGhpY3MvU1ZHLzEuMS9EVEQvc3ZnMTEuZHRkJz48c3ZnIGVuYWJsZS1iYWNrZ3JvdW5kPSJuZXcgMCAwIDI0IDI0IiBoZWlnaHQ9IjI0cHgiIGlkPSJMYXllcl8xIiB2ZXJzaW9uPSIxLjEiIHZpZXdCb3g9IjAgMCAyNCAyNCIgd2lkdGg9IjI0cHgiIHhtbDpzcGFjZT0icHJlc2VydmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiPjxnPjxnPjxwYXRoIGNsaXAtcnVsZT0iZXZlbm9kZCIgZD0iTTE5Ljc3OSwzLjM0OWwtMS4xMTEsMS42NjRDMjAuNjk5LDYuNjYzLDIyLDkuMTc5LDIyLDEyICAgIGMwLDIuODIyLTEuMzAxLDUuMzM4LTMuMzMyLDYuOTg4bDEuMTExLDEuNjYzQzIyLjM0NSwxOC42MzksMjQsMTUuNTE2LDI0LDEyQzI0LDguNDg1LDIyLjM0Niw1LjM2MiwxOS43NzksMy4zNDl6IE0xNy41NSw2LjY4NyAgICBsLTEuMTIyLDEuNjhjMC45NjgsMC45MTMsMS41OCwyLjE5OCwxLjU4LDMuNjM0cy0wLjYxMiwyLjcyMi0xLjU4LDMuNjM1bDEuMTIyLDEuNjhDMTkuMDQ3LDE2LjAzLDIwLDE0LjEyOCwyMCwxMiAgICBDMjAsOS44NzMsMTkuMDQ4LDcuOTcxLDE3LjU1LDYuNjg3eiBNMTIsMWMtMS4xNzcsMC0xLjUzMywwLjY4NC0xLjUzMywwLjY4NFM3LjQwNiw1LjA0Nyw1LjI5OCw2LjUzMUM0LjkxLDYuNzc4LDQuNDg0LDcsMy43Myw3ICAgIEgyQzAuODk2LDcsMCw3Ljg5NiwwLDl2NmMwLDEuMTA0LDAuODk2LDIsMiwyaDEuNzNjMC43NTQsMCwxLjE4LDAuMjIyLDEuNTY3LDAuNDY5YzIuMTA4LDEuNDg0LDUuMTY5LDQuODQ4LDUuMTY5LDQuODQ4ICAgIFMxMC44MjMsMjMsMTIsMjNjMS4xMDQsMCwyLTAuODk1LDItMlYzQzE0LDEuODk1LDEzLjEwNCwxLDEyLDF6IiBmaWxsLXJ1bGU9ImV2ZW5vZGQiLz48L2c+PC9nPjwvc3ZnPg==';
var kimFormatTime = function(date) {
    return DevExpress.localization.formatDate(date, "dd MMMM yyyy, HH:mm");
}
var strip = function(html){
    var doc = new DOMParser().parseFromString(html, 'text/html');
    return doc.body.textContent || "";
}
function b64EncodeUnicode(str) {
    return btoa(encodeURIComponent(str).replace(/%([0-9A-F]{2})/g,
        function toSolidBytes(match, p1) {
            return String.fromCharCode('0x' + p1);
        }));
};
function b64DecodeUnicode(str) { return decodeURIComponent(atob(str).split('').map(function (c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
};
/**
 * 
 * @param {any} parentElement jquery element to cast viewer at
 * @param {any} controllerPath path to IElectronicKIMViewer controller
 * @param {any} options options of class { id, }
 */
function ElectronicKIMViewerClass(parentElement, controllerPath, options) {
    if (typeof DevExpress === "undefined") throw ("DevExpress class not defined");
    //DevExpress.localization.locale (navigator.language || navigator.browserLanguage);
    if (typeof b64DecodeUnicode === 'undefined') throw ('base64 decoder class not defined');
    var _this = this;
    this.parent = parentElement ? parentElement : window;
    // this.container = 
    this.thresoldWidth = 500;
    this.thresoldPassed;
    this.options = options;
    this.controllerPath = controllerPath;
    this._sceduleStopService = false;
    this.$ev = $('<div id="kim-responsive-box" class="electronic-kim-view">').css({
        'font-size': '1.2em',
        'font-family':'sans-serif'
    });
    if(this.parent instanceof jQuery){
        this.$ev.appendTo(this.parent);
    } else {
        $('body').append('<div class="container">').append(this.$ev);
    }
    this.$err = $('<div id="kim-err">').css({
        'display': 'none',
        'color': 'rgb(235, 64, 52)',
        'background': 'rgb(255, 198, 194)',
        'position': 'absolute',
        'top': '40%',
        'text-align': 'center',
        'padding': '5px',
        'font-size': '1.2em',
        'z-index': '99999',
        'border': '1px rgb(235, 64, 52) dotted',
        'width':'95%',
        'border-radius':'5px'
}).on('click', function(e) { 
        $(e.target).css('display', 'none'); 
    }).appendTo(this.$ev);
    this.$toolBar = $('<div id="kim-view-toolbar">').appendTo(this.$ev);
    this.$playerContainer = $('<div id="kim-audio-container" style="display: none">').appendTo(this.$ev);
    this.$tasksBar = $('<div id="kim-view-tasksBar">').css({
        "padding":"5px 0",
        "height": _this.contentHeight
    }).appendTo(this.$ev);
    this.$wnd = $('<div id="kim-view-wnd">').appendTo(this.$tasksBar);
    this.$wnd.css('padding', '5px');

    this.$footer = $('<div id="kim-view-footer">')
        .css({
            'font-weight': 'bold',
            'position': 'absolute',
            'bottom': '7px',
            'left':'0px',
            'padding':'0 5px'
        })
        .appendTo(this.$ev);
    this.contentHeight = 0;
    this._totalWidth = 0; 
    this._totalHeight = 0;
    this.prepareLayout = function() {
        var totalHeight = _this._totalHeight = $(_this.parent).height();
        var totalWidth = _this._totalWidth = $(_this.parent).width();
        _this.thresoldPassed = totalWidth < _this.thresoldWidth;
        var theme = _this.thresoldPassed ? 'generic.light.compact' : 'generic.light';
        //DevExpress.ui.themes.current(theme);
        var toolbarHeight = _this.$toolBar.height();
        if (!toolbarHeight) toolbarHeight = _this.thresoldPassed ? 36 : 48;
        var footerHeight = _this.$footer.height();
        if (!footerHeight) footerHeight = _this.thresoldPassed ? 36 : 48;
        _this.contentHeight = totalHeight - (toolbarHeight + footerHeight) - 30;
        var $tasksList = $('#kim-view-tasks-list');
        if ($tasksList.length) {
            $tasksList.dxList('instance').option('height', Math.round(_this.contentHeight));
        }
        if (_this.kimContent) {
            _this.kimContent.option('height', Math.round(_this.contentHeight));
        }
        if (_this.timer) {
            _this.timer.option('width', _this.thresoldPassed ? 80 : 95);
        }
        if (_this._kimAnswerValueBox) {
            var w=totalWidth-700;w=w<250?250:w;
            _this._kimAnswerValueBox.option('width', w);
        }
        $('#kim-instruct').height(totalHeight);
        $('#kim-instr-body').height(totalHeight-160);
    };
    this.prepareLayout();
    window.onresize = function() {
        _this.prepareLayout();
    }
    this._getExamTimeLeftPtr = 0;
    this._getExamTimeLeft = function() {
        if (!_this.session) return;
        $.get(`${_this.controllerPath}/GetTimeLeft`, { sessionId: _this.session.SessionID }, function(data) {
            var time;
            if (data === null) time = '--:--';
            else time = _this.getTime(data);
            $('.exam-end-timer').html(time);
            $('#kim-timer-box').dxTextBox('instance').option('value',time);
            _this._examTimeLeft = data;
            if (data <= 0 && data != null) {
                _this._finishExam(true);
            }
            if (_this._sceduleStopService) clearInterval(_this._getExamTimeLeftPtr);
        });
    };
    this._examTimeLeft = null;
    this.currentKimTask = null;
    if (typeof options === "undefined" | !options["id"]) throw ("Не задан идентификатор задания");

    this.kim = null;
    this._getScrollView = function($div){        
       _this.kimContent = $div.css('font-size', '1.5em').dxScrollView({
            useNative: false,
            direction:'vertical',
            height: Math.round(_this.contentHeight),
            showScrollbar: 'always'
        }).dxScrollView('instance');
    };
    this.kimContent = null;
    this.drawer = null;
    this.toolbar = _this.$toolBar.dxToolbar({
        items:[
           {
            widget:"dxTextBox",
            location:"after",
            options:{
                elementAttr:{ id: 'kim-timer-box' },
                inputAttr: { style: 'font-weight: bold' },
                stylingMode:"outlined",
                width: _this.thresoldPassed ? 80: 95,
                readOnly:"true",
                buttons:[{
                    name:'clock',
                    location:'before',
                    options: {
                        icon: 'clock',
                        stylingMode: 'text',
                        type:'success'
                    }                    
                }]
            }
        }
    ]}).dxToolbar('instance');
    this.loadKimNavigation = function() {
        var drawer = _this.$tasksBar.dxDrawer({
            opened: true,
            closeOnOutsideClick: false,
            animationEnabled: true,
            animationDuration: 400,
            template: function() {
                var $list = $('<div id="kim-view-tasks-list">').width(64).css('text-align', 'center');
                return $list.dxList({
                    dataSource: _this.kim,
                    onItemClick: function(e){
                        _this.currentListIndex = e.itemIndex;
                        e.component.option('selectedItemKeys', [e.itemIndex]);
                    },
                    onSelectionChanged: _this.handleTaskChange,
                    height: _this.contentHeight,
                    showScrollbar: 'always',
                    selectionMode: 'single',
                    pageLoadMode: "scrollBottom",
                    itemTemplate: function(t) {
                        return $('<b>').html(t.text);
                    }
                });
            }
        });
        _this.drawer = drawer.dxDrawer('instance');
        var items = _this.toolbar.option('items');
        items.push({
            widget: "dxButton",
            location: "before",
            options: {
                icon: "menu",
                onClick: function() { _this.drawer.toggle(); }
            }
        });
        items.push({
            widget:"dxButton",
            location:"after",
            options: {
                elementAttr: { id:"kim-finish-exam" },
                text:"Завершить",
                icon:"check",
                type:"success",
                onClick: function() {
                    _this._finishExam();
                }
            }
        });
        _this.toolbar.option('items', items);
    }
    this._startExam = function(onStart) {
        $.post(`${controllerPath}/StartExam`, { id: _this.options.id }, function(data) {
            _this.session.ExamStartTime = data.ExamStartTime;
            _this.session.ExamEndTime = data.ExamEndTime;
            if (onStart) onStart();
        });
    }
    this._finishExam = function(force) {
        if (_this._isFinishingState) return;
        _this._isFinishingState = true;
        var func = function() {
            $.get(`${_this.controllerPath}/FinishExam`,
                function(data) {
                    $('#kim-view-tasks-list').dxList('instance').option('disabled', true);
                    _this.footer.option('disabled', true);
                    $('#kim-finish-exam').dxButton('instance').option('disabled', true);
                    if (_this._player) {
                        _this._player.pause();
                        $('#kim-audioTask').empty();
                    }
                    _this._sceduleStopService = true;
                    //if (data) return;
                    var dateEndLoad = new Date(_this.session.ExamEndTime);
                    dateEndLoad.setDate(dateEndLoad.getDate() + 1);					
                    var res = DevExpress.ui.dialog.alert(`<h4>Сдача экзамена завершена.</h4><p>Все Ваши ответы на вопросы части 1 сохранены автоматически. Фотографии или сканы листов с Вашими ответами на задания части 2 Вы можете прикрепить к экзамену <br>в Личном кабинете участника в течении 24 часов с момента завершения экзамена, до ${kimFormatTime(dateEndLoad)}</p><p>Спасибо за участие!</p>`, "Экзамен завершён");
                    res.done(function(){
						_this._isFinishingState = false;
                       if(data === 3){
                           document.location.href = "/Order/OrdersForUser";
                       } 
                    });
                });
        }
        if (force) {
            func();
        } else {
            var result = DevExpress.ui.dialog.confirm("<i>Вы уверены, что хотите завершить сдачу экзамена?</i><br />Дальнейшее изменение Ваших ответов части 1 будет невозможно.", "Завершить экзамен?");
            result.done(function(dialogResult) {
                if (dialogResult) func();
				_this._isFinishingState = false;
            });
        }
    }
    this._setFinishButtonState = function() {
        var btn = $('#kim-finish-exam').dxButton('instance');
        var stylingMode = 'outlined';
        if (_this.session.Answers) {
            var cnt = _this.session.Answers.filter(function(i) { return i.length > 0 }).length;
            if (cnt === _this.kim.length) stylingMode = 'contained';
        }
        btn.option('stylingMode', stylingMode);
    }
    this._isAudioComplete = function() {
        return _this.session && _this.session.Player && _this.session.Player.audioComplete;
    };
    this.loadKimContent = function(d, c) {
        if(_this.kimContent) _this.kimContent.dispose();
        _this.$wnd.empty();
        for (var i = 0; i < d.data.length; i++) {
            var item = d.data[i];
            var html = "";
            if (item.HtmlData) html = b64DecodeUnicode(item.HtmlData);
            if (html.includes('<audio')) {
                // if we found an audio-tag in current recieved html data then 
                // initializing player(), if session has incomplete audio
                if (!_this._isAudioComplete()) {
                    if(_this._player == null) _this._initPlayer(html);
                }
            } else {
                _this.$wnd.append(html);
            }
            
        }
        _this._getScrollView(_this.$wnd);

    }
    this.reInitControlsState = function(loadedData, paramsData) {
        _this.disableValueChangeHandlers = true;
        var tb = $('#kim-answer-value').dxTextBox('instance');
        tb.option('value',null);
        tb.option('visible', paramsData.TaskType === 1);
        $('#kim-part2-onlist-hint').css('display', paramsData.TaskType === 2 ? 'block':'none');
        tb.element().css('border-color', '');
        $('#kim-regexp-message').empty();        
        _this.currentKimTask = paramsData;
        _this.currentKimTask.data = loadedData;
        var rules = _this.answerValidator.option('validationRules');
        rules[0].pattern = new RegExp(paramsData.RegExp);
        rules[0].message = paramsData.RegExpTip;
        _this.answerValidator.option('validationRules', rules);        
        tb.option('disabled', false);
        if (_this.session) {
            if(Object.keys(_this.session.Answers).length !== 0){
                var answer = _this.session.Answers[`${paramsData.TaskNumber}`];
                if(answer){                   
                    tb.option('value',answer);
                }
            }
        }
        $('#kim-next-button').dxButton('instance').option('disabled', paramsData.TaskNumber === _this.kim.length );
        $('#kim-prev-button').dxButton('instance').option('disabled', paramsData.TaskNumber === 1 );
        _this.disableValueChangeHandlers = false;
    }
    this.currentListIndex;

    this._selectTask = function(number) {
        if (_this.session.SessionEnded) return;
        _this.currentListIndex = number;
        var list = $('#kim-view-tasks-list').dxList('instance');
        if(!list && !_this._selectTask.interval ){
            _this._selectTask.interval = setInterval( function(){ _this._selectTask(number); }, 200);
        } else {
            if(_this._selectTask.interval) clearInterval(_this._selectTask.interval);
            list.selectItem(number);
        }        
    };
    this.handleTaskChange = function(e1) {
        if(!e1.addedItems.length) return;
        var item = e1.addedItems[0];
        //_this.currentListIndex = item.itemIndex;
        $.get(`${controllerPath}/GetTask`,
            { id: _this.options.id, taskNumber: item.TaskNumber },
            function(data) {
                if (!data) return;
                _this.reInitControlsState(data, item);
                _this.loadKimContent(data, _this.kimContent);
            });
    }
    $.get(`${controllerPath}/GetKIMStructure`,
        { id: _this.options.id },
        function(data) {
            if (!data) throw("Невозможно получить струкруру КИМ");
            _this.kim = data;
            _this.loadKimNavigation();
        });
    this.changeTask = function(direction){
        var newInx = (_this.currentListIndex ? _this.currentListIndex: 0) + direction;        
        _this._selectTask(newInx);
    }
    this.footer = this.$footer.dxToolbar({
        items: [
            {
                widget: "dxButton",
                location: "after",
                options: {
                    icon: "chevronprev",
                    elementAttr: {id:"kim-prev-button"},
                    hint:"Предыдущее задание",
                    onClick: function() {
                        _this.changeTask(-1);
                    }
                }
            },
            {
                widget: "dxButton",
                location: "after",
                options: {
                    icon: "chevronright",
                    elementAttr: {id:"kim-next-button"},
                    hint:"Следующее задание",
                    onClick: function() {
                        _this.changeTask(1);
                    }
                }
            },
            {
                location:"before",
                template: function(e){
                    var $div = $('<div id="kim-part2-onlist-hint">').css({'padding':'5px', 'display':'none'})
                    .html('Для записи ответов части 2 используйте чистые листы');
                    return $div;
                }
            },
            {
                location:'before',
                widget:'dxTextArea',
                options: {
                    elementAttr: { id: "kim-answer-multiline" },
                    visible: false,

                }
            },
            {
                widget: "dxTextBox",
                location: "before",
                options: {
                    elementAttr: { id: "kim-answer-value" },
                    inputAttr: { 'style':  "text-transform: uppercase; font-weight: bold;" },
                    placeholder:"ответ...",
                    width: (_this._totalWidth-700)<250?250:(_this._totalWidth-700),
                    disabled: true, //_this.currentKimTask ? true : false
                    onValueChanged: function(e1) {
                        _this.answerValidator.validate();
                        if (_this.disableValueChangeHandlers) return;                        
                        _this.updateSessionTask(e1.value);
                    }
}
            },
            {
                widget:"",
                location:"before",
                template: function(){
                    return $('<div id="kim-regexp-message">').css('color', 'rgb(252, 186, 3)');
                } 
            },
            {
                widget:"dxValidator",
                location:"before",
                options: {
                    elementAttr: { id:"kim-input-validator" },
                    adapter: {
                        getValue: function() { 
                            var val =  _this._kimAnswerValueBox.option('value');
                            if(val && typeof val === 'string') val = val.toUpperCase();
                            return val; 
                        },
                        applyValidationResults: function(e){
                            if(!e.value) return;
                            var $elt = _this._kimAnswerValueBox.element();
                            if(!e.isValid) {
                                $('#kim-regexp-message').html(_this.currentKimTask.RegExpTip);
                                $elt.css('border', '1px solid rgb(252, 152, 3)');
                            } else {
                                $('#kim-regexp-message').empty();
                                $elt.css('border', '1px solid rgb(3, 252, 152)');
                            }
                        },
                        validationRequestsCallbacks: _this._callbacks
                    },
                    validationRules: [
                        {
                            type: "pattern",
                            pattern: _this.currentKimTask ? new RegExp(_this.currentKimTask.RegExp) : undefined,
                            message: _this.currentKimTask ? _this.currentKimTask.RegExpTip : undefined               
                        }],
                        onValidated: function(e){
                            if(!e.value) return;
                            var list = $('#kim-view-tasks-list').dxList('instance');
                            _this._repaintTasksList(list, _this.currentListIndex, e.isValid);
                        }
                }
            }
        ]
    }).dxToolbar('instance');
    this._repaintTasksList = function(list, index, valid) {
        var item = list.getItemElementByFlatIndex(index);
        if(!item) return;        
        var color = valid ? "rgb(174 252 284)" : "rgb(252 242 166)";
        var hoverColor = valid ? 'rgb(152, 242, 245)' : 'rgb(255, 237, 99)';
        //var activeColor = valid ? 'rgb(48, 167, 171)' : 'rgb(222, 198, 64)'
        item.css('background-color', color);
        item.hover(function(e){ $(this).css('background-color', e.type === "mouseenter"?hoverColor:color ); });
        //item.active(function(e){ $(this).css({'color':'black', 'background-color': activeColor}); });
    };
    this._kimAnswerValueBox = $('#kim-answer-value').dxTextBox('instance');
    /* VALIDATOR */
    this._callbacks = [];
    this._revalidate = function() {
        _this._callbacks.forEach(func => {
            func();
        });
    }
    this.answerValidator = $('#kim-input-validator').dxValidator('instance');

    /* SESSION */
    this.updateSessionTask = function(val) {
        val = strip(val);
        if(!_this.currentKimTask) return;
        _this.session.Answers[`${_this.currentKimTask.TaskNumber}`] = val;
        if(_this.disableValueChangeHandlers) return; var data = {
                taskNumber: _this.currentKimTask.TaskNumber,
                taskType: _this.currentKimTask.TaskType,
                sessionId: _this.session.SessionID,
                kimId: _this.options.id,
                value: val
            };
            if(_this.options.Graduate) data.graduate = _this.options.Graduate;
        $.post(`${_this.controllerPath}/UpdateTask`, data);
    }
    this.session = null;
    this.timer = null;
    this.enableSessionWatcher = function() {
        _this._interval = setInterval(_this._syncSession, 5000);
        _this.timer = $('#kim-timer-box').dxTextBox('instance');
    };
    this._interval = 0;
    this._syncSession = function() {
        if(_this._syncSession.syncState) return;
        $.get(`${controllerPath}/GetSession`, { id: _this.options.id }, function(data) {
            _this._syncSession.syncState = true;
            _this.session.ExamStartTime = data.ExamStartTime;
            _this.session.ExamEndTime = data.ExamEndTime;
            _this.session.SessionStartTime = data.SessionStartTime;
            _this.session.SessionEnded = data.SessionEnded;
            if (!data.Player && _this.session.Player) data.Player = {};
            data.Player.audioComplete = _this.session.Player.audioComplete;
            data.Player.currentTime = _this.session.Player.currentTime;
            data.Player.index = _this.session.Player.index;
            data.Player.volume = _this.session.Player.volume;
            $.post(`${controllerPath}/SetSession`, data, function(e){
                _this._syncSession.syncState = false;
                //clearing interval on exam finish
                if (_this._sceduleStopService) clearInterval(_this._interval);
            });
        });
    };
    this._repaintAllTasks = function(answers) {
        if (!answers) answers = _this.session.Answers;
        if (!answers) return;
        var list = $('#kim-view-tasks-list').dxList('instance');
        if(!list){
            _this._repaintAllTasks.interval = setInterval(function(){ _this._repaintAllTasks(answers); }, 200)
        } else {
            if(_this._repaintAllTasks.interval) clearInterval(_this._repaintAllTasks.interval);
            for (var a in answers) {
            if (Object.prototype.hasOwnProperty.call(answers, a)) {
                var task = _this.kim.find(function(x) { return x.TaskNumber ===  parseInt(a); });
                if(!task) continue;
                var re = new RegExp(task.RegExp);
                _this._repaintTasksList(list, a-1,re.test(answers[a]));
                }
            }
        }
        
    }
    /* СЕССИОН */
    $.get(`${controllerPath}/GetSession`, { id: _this.options.id }, function(data) {
        if (!data) return;
        // session init
        _this.session = data;
        //prepare initfunc
        _this._getExamTimeLeftPtr = setInterval(_this._getExamTimeLeft, 1000);
        var func = function() {
            // reassemble answered tasks state
            if (!jQuery.isEmptyObject(data.Answers)) {
                _this._repaintAllTasks(data.Answers);
            }
            // player init (if already started)
            if(data.Player && typeof data.Player.index !== 'undefined') _this._selectTask(data.Player.index);
            _this.enableSessionWatcher();
        }
        //checking examStartTime
        if (data.ExamStartTime == null) _this._loadInstuct(func);
        else func();

    });
    /* ИНСТРАКШОН */
    _this._loadInstuct = function(func) {
        $.get(`${controllerPath}/GetTask`,
            { id: _this.options.id, taskNumber: 0 },
            function(d) {
                var $instr = $('<div id="kim-instruct" align="center">').css({
                     'position':'absolute', 'top':'0', 'background-color':'white'
                }).appendTo(_this.$ev);
                var $ins2 = $('<div>').css('padding', '20px').appendTo($instr);
                var $head = $('<div><h2>Инструкция по выполнению заданий</h2></div>')
                    .appendTo($ins2);
                var $body = $('<div id="kim-instr-body">').appendTo($ins2);
                for (var i = 0; i < d.data.length; i++) {
                    var item = d.data[i];
                    var html = "";
                    if (item.HtmlData) html = b64DecodeUnicode(item.HtmlData);
                    $body.append(html);
                }
                $body.dxScrollView({
                    height: _this.contentHeight-40
                });
                var $instrTool = $('<div id="kim-instr-tool">').css('margin-top', '8px').dxToolbar({
                    items: [
                        {
                            widget: "dxCheckBox",
                            location: "after",
                            options: {
                                text:"Я прочитал инструкцию и готов начать",
                                onValueChanged: function(e) {
                                    $('#kim-begin-exam-btn').dxButton('instance').option("disabled", !e.value);
                                }
                            }
                        }, {
                            widget: "dxButton",
                            location: "after",
                            options: {
                                elementAttr: {id: "kim-begin-exam-btn" },
                                type:"success",
                                text:"Начать экзамен",
                                disabled: true,
                                onClick: function() {
                                    _this._startExam(function() {
                                        $instr.remove();
                                        func();
                                    });
                                }
                            }
                        }
                    ]
                });
                $instrTool.appendTo($ins2);
            });
        
    };
    /* ПЛАУЕР */
    this._player = null;
    this.getTime = function(t) {
        var m = ~~(t / 60), s = ~~(t % 60);
        return (m < 10 ? "0" + m : m) + ':' + (s < 10 ? "0" + s : s);
    };
    this._initPlayer = function(audio) {
        var duration = 0;
        var volume = _this.session.Player.volume ? _this.session.Player.volume:0.7;
        if (_this._isAudioComplete()) {
            $(audio).empty();
            return;
        }
        _this.$playerContainer.append($(audio));
        _this._player = document.getElementById('kim-audioTask');
        if(!_this._player) return;
        var durationText = '00:00';
        var currentTime = 0;
        var box = null;
        /* ONCANPLAY */
        var q=0;
        var $player = $(_this._player);
         $player.one('canplay', function(e){
            e.target.currentTime = currentTime; 
        });
        $player.on('durationchange',function(e){
            var a = 1;
        });       
        /* PLAYER LOAD META */
        _this._player.onloadedmetadata = function() {
            duration = _this._player.duration;
            _this._getPlayerControl();
            box = $('#kim-toolbar-player-control').dxTextBox('instance');
            durationText = _this.getTime(duration);
            if (_this.session.Player.currentTime > 0) {
                currentTime = _this.session.Player.currentTime;
                // if already initialized and played a bit, then let it rock!
                var kop = $('<div id="kinda-popup">').appendTo($('#kim-responsive-box')).dxPopup({
                    fullScreen: true,
                    showTitle: false,
                    contentTemplate: function() {
                        var $c = $("<div align='center'><h2>Ваш сеанс не был завершен корректно</h2><div id='continue-my-exam'></div><div><h3>До конца экзамена <strong><div class='exam-end-timer'></strong></h3></div></div>");
                        $c.find('#continue-my-exam').dxButton({
                            text: "Продолжить экзамен",
                            icon: "chevronright",
                            onClick: function() {
                                $('#kim-toolbar-player-play').dxButton('instance').option('disabled', true);
                                kop.hide();
                                _this._player.play();
                                setTimeout(function(){
                                    _this._player.pause();
                                    _this._player.currentTime = currentTime;
                                    _this._player.play();
                                }, 200);
                                
                               // _this._player.pause();
                                
                                kop.dispose();
                            }
                        });
                        return $c;
                    }
                }).dxPopup('instance');
                kop.show();
            }
            box.option('value', `${_this.getTime(currentTime)} / ${durationText}`);
            /* AFTER META IS LOADED HOOKING EVENTLISTENER ON TIME UPD */
            _this._player.addEventListener("timeupdate",
            function() {
                var complete = duration > 0 ? (_this._player.currentTime >= duration) : false;
                _this.session.Player.audioComplete = complete;
                _this.session.Player.currentTime = _this._player.currentTime;
                if(box){
                    box.option('value', `${_this.getTime(_this._player.currentTime)} / ${durationText}`);
                    // disposing player box after 10 secs of 'complete'
                    if (complete) setTimeout(function() { box.dispose(); }, 10000);
                }                    
            },
            false);

        };

        _this._player.load();
        _this._player.volume = volume;
        if (typeof _this.session.Player === "undefined") _this.session.Player = {};
        _this.session.Player.index = _this.currentListIndex;
        
        
    };
    this._getPlayerControl = function() {
        var items = _this.toolbar.option('items');
        var player = $('#kim-toolbar-player-control');
        if (player.length) {
            player = player.dxTextBox('instance');
            return player;
        } else {
            items.push({
                widget: "dxTextBox",
                location: "before",
                options: {
                    elementAttr: { id: "kim-toolbar-player-control", style:"border:none" },
                    width: _this.thresoldPassed ? 100 : 130,
                    stylingMode: "underlined",
                    buttons:[
                        {
                            name: "play",
                            location: "before",
                            options: {
                                stylingMode: "text",
                                icon:"chevronright",
                                type:"success",
                                elementAttr: {id: 'kim-toolbar-player-play' },
                                onClick: function() {
                                    var dlg = DevExpress.ui.dialog.custom({
                                        buttons: [
                                            {
                                                text:"Да",
                                                onClick: function() {
                                                    if (!_this._player) return;
                                                    _this._player['play']();
                                                    $("#kim-toolbar-player-play").dxButton('instance')
                                                        .option('disabled', true);
                                                }
                                            }, { text:"Нет" }],
                                        messageHtml: "Вы запросили воспроизведение звуковой дорожки для заданий<br>Приостановить воспроизведение нельзя<br>Воспроизвести сейчас?"
                                });
                                dlg.show();
                                }
                            }
                        }
                    ]
                }
            });
            items.push({
                name: "vol",
                location: "before",
                widget: "dxDropDownButton",
                options: {
                    elementAttr: { id: "kim-volume-control" },
                    icon: iconVol,
                    activeStateEnabled: false,
                    stylingMode: "text",
                    showArrowIcon: false,
                    dropDownContentTemplate: function(e) {
                        var $slide = $('<input type="range" orient="vertical" id="kim-volume" />').css({
                            'writing-mode': 'bt-lr',
                            '-webkit-appearance': 'slider-vertical',
                            'width': '8px',
                            'height': '175px',
                            'padding': '11px'
                        });
                        $slide.on('input', function(e) {
                            if (_this._player) _this.session.Player.volume = _this._player.volume = e.target.value / 100;
                        });
                        return $slide;
                    }
                }
            });
            _this.toolbar.option('items', items);
        }
        return $('#kim-toolbar-player-control').dxTextBox('instance');
    }

    // err handle
    $(function() {
        $.ajaxSetup({
            error: function(x) {
                var exception = x.responseJSON ? x.responseJSON : x;
                $('#kim-err').css('display', 'block').html(exception.Message);
            }
        });
        }
    );
}

/*
 * GetKIMStructure: [{
 *      @TaskNumber - 1-based pass-through number,
 *      @PartTaskNumber - 1-based TaskType-scope number, 
 *      @TaskType - (0-A, 1-B, 2-C, 3-D)
 *      @text - (B1, B2, B3 etc)
 *      @RegExp - expression to check answer,
 *      @RegExpTip - regular expression completion tip
 * }]
 */