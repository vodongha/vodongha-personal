window.chatUtils = {
    scrollToBottom: function (elementId) {
        var el = document.getElementById(elementId);
        if (el) {
            el.scrollTop = el.scrollHeight;
        }
    },
    scrollToUnread: function (elementId) {
        var container = document.getElementById(elementId);
        if (!container) return;
        var divider = container.querySelector('.chat-unread-divider');
        if (divider) {
            // Scroll so the divider is near the top with a little breathing room
            var offsetTop = divider.offsetTop - 12;
            container.scrollTo({ top: offsetTop, behavior: 'smooth' });
        } else {
            container.scrollTop = container.scrollHeight;
        }
    }
};
