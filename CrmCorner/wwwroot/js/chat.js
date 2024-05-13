$(function () {

    var $SendMessageTextArea = $("#SendMessageTextArea");
    var $UserListSelectBox = $("#UserList");
    var $SendMessageBtn = $("#SendMessageBtn");
    var $MessageBox = $("#MessageBox");
    var isMyMessage = false;

    var signalRConnection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

    signalRConnection.on("ChatChannel", function (message, dateTime) {

        $MessageBox.append(`<li class="clearfix">
                                <div class="message-data mb-3">
                                    <span class="small text-muted me-3">${dateTime}</span>
                                </div>
                                <div class="message my-message d-inline-block">
                                    ${message}
                                </div>
                            </li>`);
        $SendMessageTextArea.val("");
    });

    $SendMessageBtn.click(function () {

        var userId = document.getElementById("selectedUserId").value;
        var message = $SendMessageTextArea.val();
        var now = new Date();
        var day = ("0" + now.getDate()).slice(-2); 
        var month = ("0" + (now.getMonth() + 1)).slice(-2); 
        var year = now.getFullYear(); // Yılı al
        var hour = ("0" + now.getHours()).slice(-2); 
        var minute = ("0" + now.getMinutes()).slice(-2); 
        var dateTime = day + "." + month + "." + year + " " + hour + ":" + minute;

        console.log(dateTime);
        $MessageBox.append(`<li class="clearfix">
                                <div class="message-data text-end mb-3 me-3">
                                    <span class="small text-muted me-2"> ${dateTime}</span>
                                    </div>
                                 <div class="message other-message d-inline-block float-end">
                                ${message}
                                </div>
                        </li>`);
        signalRConnection.invoke("SendMessage", message, userId, dateTime);
    });

    signalRConnection.start().then(function () {

    }).catch(function (err) {
        return console.error(err.toString());
    });
    setInterval(function () {
        console.log("SignalR bağlantısı yenileniyor...");
        connection.stop()
            .then(function () {
                return connection.start();
            })
            .then(function () {
                console.log("SignalR bağlantısı başarıyla yenilendi.");
            })
            .catch(function (err) {
                console.error("SignalR bağlantısı yenilenemedi: " + err);
            });
    }, 60000); 

})