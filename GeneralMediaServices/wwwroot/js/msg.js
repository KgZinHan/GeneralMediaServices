// Create a new SignalR connection object
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/messageHub")
  .build();

// Start the connection to the server
connection.start()
  .then(() => {
    console.log("SignalR connection established.");
  })
  .catch((err) => {
    console.error("SignalR connection error: ", err);
  });



// Add an event listener for the "submit" event on the form
console.log("i am listener");
const btn = document.getElementById("submitbtn");
btn.addEventListener("click", function (event) {
  var msg = document.getElementById("id01").value;
  var sys = document.getElementById("id02").value;
  var sms = document.getElementById("id03").value;
  var api = document.getElementById("id04").value;
  connection.invoke("UpdateMediaService", msg, sys, sms, api).catch(function (err) {
    return console.error(err.toString());
  });
  document.getElementById("createForm").onreset();

  event.preventDefault();

});
