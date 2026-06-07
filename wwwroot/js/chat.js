window.chatUtils = {
    detectCountry: async function () {
        try {
            const r = await fetch('https://ipinfo.io/json', { signal: AbortSignal.timeout(4000) });
            const d = await r.json();
            return d.country || '';
        } catch {
            return '';
        }
    },
    scrollToBottom: function (elementId) {
        var el = document.getElementById(elementId);
        if (el) el.scrollTop = el.scrollHeight;
    },
    scrollToUnread: function (elementId) {
        var container = document.getElementById(elementId);
        if (!container) return;
        var divider = container.querySelector('.chat-unread-divider');
        if (divider) {
            container.scrollTo({ top: divider.offsetTop - 12, behavior: 'smooth' });
        } else {
            container.scrollTop = container.scrollHeight;
        }
    }
};
