var ActiveSessionMonitor = function(container) {
    var self = this;

    self.url = null;
    self.socket = null;
    //self.listElement = sessionListElement;
    self.container = container;


    this.getListElement = function() {
        return $(self.container).find('[data-session-list]');
    };

    this.getMaskElement = function () {
        return $(self.container).find('[data-session-mask]');
    };

    this.getEmptyElement = function () {
        return $(self.container).find('[data-session-empty]');
    };

    this.connect = function(url) {
        self.url = url;

        if (!window.WebSocket) {
            // not supported
            console.log('WebSocket is unsupported!');
            return;
        }

        self.socket = new EventSource(url);
        self.socket.addEventListener('open', onOpen, false);
        self.socket.addEventListener('close', onClose, false);
        self.socket.addEventListener('message', onMessage, false);
    };

    this.updateMask = function() {
        if (self.getListElement().is(':empty')) {
            self.getMaskElement().show();
            self.getEmptyElement().show();
        } else {
            self.getMaskElement().hide();
            self.getEmptyElement().hide();
        }
    };

    function onOpen(e) {
        // TODO: hide overlay
    }

    function onClose(e) {
        // TODO: show overlay
    }

    function onMessage(e) {
        var session = JSON.parse(e.data);
        console.log('session changed', session);

        var sessionElement = null;

        var sessionElementList = self.getListElement().find('[data-session-id="' + session.id + '"]');
        if (sessionElementList.length > 0) sessionElement = sessionElementList[0];

        if (!session.isReleased) {
            if (sessionElement == null) {
                sessionElement = $('<div/>')
                    .addClass('session-row')
                    .attr('data-session-id', session.id);

                //sessionElement.prepend($('<i class="">'));

                sessionElement.appendTo(self.getListElement());
                //$(self.listElement).append(sessionElement);
            }

            onSessionUpdate(sessionElement, session);
        } else {
            onSessionReleased(sessionElement, session);
        }

        self.updateMask();
    }

    function onSessionUpdate(e, data) {
        e.append($('<span>')
            .text(data.id));
    }

    function onSessionReleased(e, data) {
        e.remove();
    }
};
