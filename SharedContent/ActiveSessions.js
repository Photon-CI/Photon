var ActiveSessionMonitor = function(container) {
    var self = this;
    self.container = container;

    self.dataUrl = null;
    self.detailsUrl = null;
    self.socket = null;
    self.onCancel = null;

    var buildTemplateRow = $(container)
        .find('[data-session-template="build"]').detach()
        .removeAttr('data-session-template');

    var deployTemplateRow = $(container)
        .find('[data-session-template="deploy"]').detach()
        .removeAttr('data-session-template');

    var updateTemplateRow = $(container)
        .find('[data-session-template="update"]').detach()
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
            if (sessionElement === null) {
                sessionElement = appendNewRow(session);
            }

            onSessionUpdate(sessionElement, session);
        } else {
            onSessionReleased(sessionElement);
        }

        self.updateMask();
    }

    function appendNewRow(session) {
        var sessionElement = getRowTemplate(session.type)
            .clone().attr('data-session-id', session.id);

        sessionElement.prependTo(self.getListElement());
        return sessionElement;
    }

    function getRowTemplate(type) {
        switch (type) {
            case 'build': return buildTemplateRow;
            case 'deploy': return deployTemplateRow;
            case 'update': return updateTemplateRow;
        }
    }

    function onSessionUpdate(e, data) {
        var iconType = getTypeIcon(data.type);
        var iconColor = getStatusColor(data.status);

        var sessionDetailsUrl = self.detailsUrl
            + '?id=' + encodeURIComponent(data.id);

        switch (data.type) {
            case 'build':
                e.find('[data-session-link]')
                    .attr('href', sessionDetailsUrl)
                    .text(data.projectName + ' - Build #' + data.number);

                e.find('[data-session-title]').text(data.name);
                e.find('[data-session-refspec]').text('@'+data.gitRefspec);
                break;
            case 'deploy':
                //
                break;
            case 'update':
                //
                break;
        }

        e.find('[data-session-status]')
            .attr('class', iconType)
            .addClass(iconColor);

        e.find('.cancel').click(function () {
            if (self.onCancel !== 'undefined' && self.onCancel !== null) self.onCancel(data.id);
        });
    }

    function onSessionReleased(e) {
        e.remove();
    }

    function getTypeIcon(type) {
        switch (type) {
            case "build": return "fas fa-cubes";
            case "deploy": return "fas fa-cloud-download-alt";
            case "update": return "fas fa-download";
            default: return "fas fa-ellipsis-h";
        }
    }

    function getStatusColor(status) {
        switch (status) {
            case "running": return "text-info";
            case "success": return "text-success";
            case "failed": return "text-danger";
            case "cancelled": return "text-warning";
            case "pending":
            default: return "text-muted";
        }
    }

    //function getStatusIcon(status) {
    //    switch (status) {
    //        case "running": return "fas fa-spinner text-info";
    //        case "success": return "fas fa-check text-success";
    //        case "failed": return "fas fa-exclamation-circle text-danger";
    //        case "cancelled": return "far fa-ban text-warning";
    //        case "pending":
    //        default: return "fas fa-ellipsis-h text-muted";
    //    }
    //}
};
