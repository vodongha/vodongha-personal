window.chatUtils = {
    detectCountry: async function () {
        // Cache per session — ipinfo.io free tier has a low rate limit and
        // the country code never changes within a single browser session.
        const CACHE_KEY = '__ipinfo_country';
        try {
            const cached = sessionStorage.getItem(CACHE_KEY);
            if (cached !== null) { return cached; }  // '' is a valid cached "miss"
        } catch { /* sessionStorage blocked by tracking prevention — fall through */ }

        let country = '';
        try {
            const r = await fetch('https://ipinfo.io/json', { signal: AbortSignal.timeout(4000) });
            if (r.ok) {
                const d = await r.json();
                country = d.country || '';
            }
            // 429 Too Many Requests / other non-ok: stay with '' fallback silently
        } catch { /* network error, CORS block, or timeout — silent fallback */ }

        try { sessionStorage.setItem(CACHE_KEY, country); } catch { }
        return country;
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
