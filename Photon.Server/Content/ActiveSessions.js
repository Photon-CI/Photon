var ActiveSessionMonitor = function(container) {
    var self = this;
    self.container = container;

    self.dataUrl = null;
    self.detailsUrl = null;
    self.socket = null;
    self.onCancel = null;

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

    this.setDetailsUrl = function(url) {
        self.detailsUrl = url;
    };

    this.setCancelAction = function(cancelAction) {
        self.onCancel = cancelAction;
    };

    this.connect = function(url) {
        self.dataUrl = url;

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

        var sessionDetailsUrl = self.detailsUrl
            + '?id=' + encodeURIComponent(data.id);

        var labelText = data.projectName;

        if (!!data.projectVersion)
            labelText += ' - ' + data.projectVersion;

        e.find('[data-session-status]').attr('class', icon);
        e.find('.label').text(labelText).attr('href', sessionDetailsUrl);

        e.find('.cancel').click(function () {
            if (self.onCancel !== 'undefined' && self.onCancel != null) self.onCancel(data.id);
        });
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
