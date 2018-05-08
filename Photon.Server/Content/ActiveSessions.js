var ActiveSessionMonitor = function(container) {
    var self = this;

    self.url = null;
    self.socket = null;
    self.container = container;

    var templateRow = $(container)
        .find('[data-session-template]').detach()
        .removeAttr('data-session-template');

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
        self.socket.onopen = onOpen;
        self.socket.onerror = onError;
        self.socket.onmessage = onMessage;
    };

    this.updateMask = function() {
        if (self.getListElement().children().length === 0) {
            self.getMaskElement().show();
            self.getEmptyElement().show()
                .text("None");
        } else {
            self.getMaskElement().hide();
            self.getEmptyElement().hide();
        }
    };

    function onOpen() {
        self.updateMask();
    }

    function onError(e) {
        if (e.readyState === EventSource.CLOSED) {
            self.getMaskElement().show();
            self.getEmptyElement().show()
                .text("Disconnected");
        }
    }

    function onMessage(e) {
        var session = JSON.parse(e.data);
        console.log('session changed', session);

        var sessionElement = null;

        var sessionElementList = self.getListElement().find('[data-session-id="' + session.id + '"]');
        if (sessionElementList.length > 0) sessionElement = sessionElementList[0];

        if (!session.isReleased) {
            if (sessionElement == null) {
                sessionElement = templateRow.clone()
                    .attr('data-session-id', session.id);

                sessionElement.appendTo(self.getListElement());
            }

            onSessionUpdate(sessionElement, session);
        } else {
            onSessionReleased(sessionElement);
        }

        self.updateMask();
    }

    function onSessionUpdate(e, data) {
        var icon = getStatusIcon(data.type);

        e.find('[data-session-status]').attr('class', icon);
        e.find('.label').text(data.id);
    }

    function onSessionReleased(e) {
        e.remove();
    }

    function getStatusIcon(type) {
        switch (type) {
            case "build": return "fas fa-cubes";
            case "deploy": return "fas fa-cloud-download-alt";
            case "update": return "fas fa-download";
            default: return "fas fa-ellipsis-h";
        }
    }
};
