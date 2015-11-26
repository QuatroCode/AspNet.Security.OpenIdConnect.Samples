$(document).ready(function () {
    var connectionBase = "http://localhost:5000";
    var connectionPath = connectionBase + "/signalr";
    var connectToSignalR = function (token) {
        var connection = $.connection(connectionPath);
        var log = function (data) { console.log(data); };
        connection.qs = "access_token=" + token;
        connection.received(function (data) {
            log("From server: " + data);
        });
        connection.stateChanged(function (change) {
            switch (change.newState) {
                case $.signalR.connectionState.reconnecting:
                    log('Re-connecting');
                    break;
                case $.signalR.connectionState.connected:
                    log("Connected to " + connection.url);
                    break;
                case $.signalR.connectionState.disconnected:
                    log("Disconnected");
                    break;
            }
        });
        connection.start().done(function () {
            connection.send("Test message");
        });
    };
    $.ajax({
        type: "POST",
        url: connectionBase + "/connect/token",
        data: "client_id=AspNetContribSample"
            + "&grant_type=password"
            + "&username=AspNet"
            + "&password=contrib"
            + "&resource=" + encodeURIComponent("http://localhost:5000/"),
        contentType: "application/x-www-form-urlencoded"
    }).then(function (data) {
        connectToSignalR(data.access_token);
    });
});
