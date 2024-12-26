let connection = new signalR.HubConnectionBuilder()
   .withUrl("/chatHub")
   .build();
let currentUser = document.getElementById("userName").value;
let userRole = document.getElementById("userRole").value;
let unreadCount = 0;
if (!currentUser) {
   document.querySelector('.chat-toggle-btn').style.display = 'none';
} else {
   connection.on("ReceiveMessage", function (user, message) {
       let messageClass = user === currentUser ? "message own" : "message";
       if (userRole === "Admin") {
           messageClass += " admin";
       }
       
       let messageDiv = document.createElement("div");
       messageDiv.className = messageClass;
       messageDiv.textContent = `${user}: ${message}`;
       document.getElementById("messagesList").appendChild(messageDiv);
       
       // Otomatik scroll
       let messagesList = document.getElementById("messagesList");
       messagesList.scrollTop = messagesList.scrollHeight;
        // Chat kapalıysa okunmamış mesaj sayısını artır
       if (document.querySelector('.chat-container').style.display === 'none') {
           unreadCount++;
           updateUnreadCount();
       }
   });
    connection.on("UserJoined", function (user) {
       let messageDiv = document.createElement("div");
       messageDiv.className = "message system";
       messageDiv.textContent = `${user} sohbete katıldı`;
       document.getElementById("messagesList").appendChild(messageDiv);
       
       // Kullanıcı listesine ekle
       let userItem = document.createElement("li");
       userItem.id = `user-${user}`;
       userItem.textContent = user;
       document.getElementById("usersList").appendChild(userItem);
   });
    connection.on("UserLeft", function (user) {
       let messageDiv = document.createElement("div");
       messageDiv.className = "message system";
       messageDiv.textContent = `${user} sohbetten ayrıldı`;
       document.getElementById("messagesList").appendChild(messageDiv);
       
       // Kullanıcı listesinden kaldır
       document.getElementById(`user-${user}`)?.remove();
   });
    connection.on("UpdateActiveUsers", function (users) {
        // Admin için aktif kullanıcı listesini güncelle
        document.getElementById("usersList").innerHTML = "";
        users.forEach(user => {
            if (user !== currentUser) { // Admin'i listede gösterme
                let userItem = document.createElement("li");
                userItem.id = `user-${user}`;
                userItem.textContent = user;
                document.getElementById("usersList").appendChild(userItem);
            }
        });
    });
    connection.on("MessageDeleted", function (messageId, message) {
        // Tüm mesajları kontrol et
        const messages = document.querySelectorAll('.message');
        messages.forEach(msg => {
            // Silinen mesajı içeren elementi bul
            if (msg.textContent.includes(message)) {
                // Mesajı güncelle
                msg.classList.add('deleted');
                msg.innerHTML += '<br><small class="text-danger">(Bu mesaj admin tarafından silindi)</small>';
            }
        });
    });
    function sendMessage() {
       let messageInput = document.getElementById("messageInput");
       if (messageInput.value.trim() === "") return;
       
       try {
           connection.invoke("SendMessage", currentUser, messageInput.value);
           messageInput.value = "";
       } catch (err) {
           console.error(err);
       }
   }
    function toggleChat() {
       let chatContainer = document.querySelector(".chat-container");
       let chatButton = document.querySelector(".chat-toggle-btn");
       
       if (chatContainer.style.display === "none") {
           chatContainer.style.display = "flex";
           chatButton.style.display = "none";
           // Chat açıldığında okunmamış mesaj sayısını sıfırla
           unreadCount = 0;
           updateUnreadCount();
       } else {
           chatContainer.style.display = "none";
           chatButton.style.display = "block";
       }
   }
    function updateUnreadCount() {
       let unreadCountElement = document.getElementById("unreadCount");
       if (unreadCount > 0) {
           unreadCountElement.style.display = "inline";
           unreadCountElement.textContent = unreadCount;
       } else {
           unreadCountElement.style.display = "none";
       }
   }
    // Enter tuşu ile mesaj gönderme
   document.getElementById("messageInput").addEventListener("keypress", function(event) {
       if (event.key === "Enter") {
           sendMessage();
       }
   });
    // Bağlantıyı başlat
   connection.start()
       .then(function () {
           if (currentUser) {
               connection.invoke("JoinChat", currentUser);
           }
       })
       .catch(function (err) {
           console.error(err);
       });
    // Sayfa kapanırken
   window.onbeforeunload = function() {
       if (currentUser) {
           connection.invoke("LeaveChat", currentUser);
       }
   }};