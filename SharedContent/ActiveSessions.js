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

    this.getStatusRow = function () {
        return $(self.container).find('thead > tr');
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

    this.updateMask = function () {
        var hasRows = $(container).find('tbody').children().length > 0;

        var statusRow = self.getStatusRow();
        if (!hasRows) {
            statusRow.show();

            statusRow.find('[data-status-icon]')
                .attr('class', 'fas fa-ban text-muted');

            statusRow.find('[data-status-text]')
                .text('None').attr('class', 'text-muted');
        } else {
            statusRow.hide();
        }
    };

    function onOpen() {
        self.updateMask();
    }

    function onError(e) {
        if (e.readyState === EventSource.CLOSED) {
            var statusRow = self.getStatusRow();
            statusRow.show();

            statusRow.find('[data-status-icon]')
                .prop('class', 'fas fa-exclamation-triangle text-danger');

            statusRow.find('[data-status-text]')
                .text('Error').attr('class', 'text-muted');
        }
    }

    function onMessage(e) {
        var session = JSON.parse(e.data);
        console.log('session changed', session);

        var sessionElement = null;

        var sessionElementList = $(self.container).find('[data-session-id="' + session.id + '"]');
        if (sessionElementList.length > 0) sessionElement = $(sessionElementList[0]);

        if (sessionElement === null) {
            sessionElement = appendNewRow(session);
        }

        onSessionUpdate(sessionElement, session);

        self.updateMask();
    }

    function appendNewRow(session) {
        var sessionElement = getRowTemplate(session.type)
            .clone().attr('data-session-id', session.id);

        sessionElement.prependTo($(self.container));
        return sessionElement;
    }

    function getRowTemplate(type) {
        switch (type) {
            case 'build': return buildTemplateRow;
            case 'deploy': return deployTemplateRow;
            case 'update': return updateTemplateRow;
            default: return null;
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
                e.find('[data-session-link]')
                    .attr('href', sessionDetailsUrl)
                    .text(data.projectName + ' - Deployment #' + data.number);

                e.find('[data-session-title]').text(data.name);
                e.find('[data-session-version]').text('@' + data.projectVersion);
                break;
            case 'update':
                e.find('[data-session-link]')
                    .attr('href', sessionDetailsUrl);
                break;
        }

        e.find('[data-session-status]')
            .attr('class', iconType)
            .addClass(iconColor);

        e.find('.cancel').click(function () {
            if (self.onCancel !== 'undefined' && self.onCancel !== null) self.onCancel(data.id);
        });
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
};
