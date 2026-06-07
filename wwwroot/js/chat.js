window.chatUtils = {
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
    },
    getCountryCode: async function () {
        try {
            var res = await fetch('https://ipapi.co/country/', { signal: AbortSignal.timeout(3000) });
            if (!res.ok) return null;
            var code = (await res.text()).trim();
            return code.length === 2 ? code : null;
        } catch {
            return null;
        }
    }
};
