"use strict";

//To establish the connection 
var connection = new signalR.HubConnectionBuilder().withUrl("/messageHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

//Create the message space
connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    li.textContent = `${user} says ${message}`;
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

//Send the message when user enter in message input box
document.getElementById("messageInput").addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
        
        var user = document.getElementById("userInput").value;
        var message = document.getElementById("messageInput").value;
        document.getElementById("messageInput").value = '';
        connection.invoke("SendMessage", user, message).catch(function (err) {
            return console.error(err.toString());
        });

        // Clear the input field
        message = "";
    }
});

//Send the message when user click send button
document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    document.getElementById("messageInput").value = '';
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    
    event.preventDefault();
});