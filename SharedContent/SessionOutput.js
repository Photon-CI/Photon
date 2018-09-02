var SessionOutputReader = function() {
    var self = this;

    self.url = null;
    self.xhr = null;
    self.readPos = 0;

    self.onAppend = null;
    self.onDone = null;
    self.onError = null;

    this.begin = function(url) {
        self.url = url;

        self.xhr = new XMLHttpRequest();
        self.xhr.onprogress = onProgress;
        self.xhr.onerror = onError;
        self.xhr.onload = onLoad;
        self.xhr.open('GET', url, true);
        self.xhr.send();
    };

    function onProgress() {
        var newText = self.xhr.responseText.substring(self.readPos);

        var lineEnd = newText.lastIndexOf('\n');

        if (lineEnd >= 0)
            newText = newText.substring(0, lineEnd + 1);

        self.readPos += newText.length;

        onAppendCallback(newText);
    }

    function onLoad() {
        var newText = self.xhr.responseText.substring(self.readPos);
        self.readPos += newText.length;

        onAppendCallback(newText);
        onDoneCallback();
    }

    function onError() {
        onErrorCallback();
    }

    function onAppendCallback(text) {
        if (self.onAppend)
            self.onAppend(text);
    }

    function onDoneCallback() {
        if (self.onDone)
            self.onDone();
    }

    function onErrorCallback() {
        if (self.onError)
            self.onError();
    }
};
