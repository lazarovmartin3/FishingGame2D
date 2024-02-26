// WebSocketInterop.jslib
var webSocket;

mergeInto(LibraryManager.library, {
    ConnectToLocalServer: function() {
        webSocket = new WebSocket("ws://localhost:8080");

        webSocket.onopen = function(event) {
            window.unityInstance.SendMessage("WebSocketManager", "OnConnected");
            //window.unityInstance.SendMessage("WebSocketManager", "GetAllGames");
        };

        webSocket.onmessage = function(event) {
            window.unityInstance.SendMessage("WebSocketManager", "OnMessage", event.data);
        };

        webSocket.onerror = function(event) {
            window.unityInstance.SendMessage("WebSocketManager", "OnError", event.message);
        };

        webSocket.onclose = function(event) {
            // Optionally handle WebSocket close event
        };
    },

    SendToServer: function(message) {
        if (webSocket.readyState === WebSocket.OPEN) {
            var parsedMessage = Pointer_stringify(message);
            console.log("State is open, sending " + parsedMessage);
            webSocket.send(parsedMessage);
        } else {
            console.error("WebSocket is not open");
        }
    }
});
