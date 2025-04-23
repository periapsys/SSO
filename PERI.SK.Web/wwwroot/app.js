function scrollToBottom() {
    var chatBox = document.getElementById('chatBox');
    if (chatBox) {
        setTimeout(function () {
            chatBox.scrollTop = chatBox.scrollHeight;
        }, 50); // Delay just enough to allow rendering
    }
}
