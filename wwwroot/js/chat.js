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

// ── Dial-code picker (pure JS — no Blazor re-render) ──────────────
window.chatDial = {
    _dotNetRef: null,

    init: function (dotNetRef) {
        this._dotNetRef = dotNetRef;
        // Close dropdown when clicking outside
        document.addEventListener('click', function (e) {
            const picker = document.getElementById('chatDialPicker');
            if (picker && !picker.contains(e.target)) {
                chatDial.close();
            }
        });
    },

    toggle: function () {
        const dd = document.getElementById('chatDialDropdown');
        if (!dd) return;
        const isOpen = dd.style.display !== 'none';
        if (isOpen) {
            this.close();
        } else {
            dd.style.display = 'block';
            document.getElementById('chatDialPicker')?.classList.add('chat-dial--open');
            const search = document.getElementById('chatDialSearch');
            if (search) { search.value = ''; this.filter(''); search.focus(); }
        }
    },

    close: function () {
        const dd = document.getElementById('chatDialDropdown');
        if (dd) dd.style.display = 'none';
        document.getElementById('chatDialPicker')?.classList.remove('chat-dial--open');
    },

    filter: function (q) {
        const list = document.getElementById('chatDialList');
        if (!list) return;
        const lower = q.toLowerCase();
        let shown = 0;
        for (const btn of list.querySelectorAll('.chat-dial__item')) {
            const match = !lower
                || btn.dataset.name.toLowerCase().includes(lower)
                || btn.dataset.dial.includes(lower)
                || btn.dataset.region.toLowerCase().includes(lower);
            btn.style.display = (match && shown < 50) ? '' : 'none';
            if (match) shown++;
        }
        // empty state
        let empty = list.querySelector('.chat-dial__empty-js');
        if (shown === 0) {
            if (!empty) {
                empty = document.createElement('div');
                empty.className = 'chat-dial__empty chat-dial__empty-js';
                empty.textContent = 'Không tìm thấy';
                list.appendChild(empty);
            }
            empty.style.display = '';
        } else if (empty) {
            empty.style.display = 'none';
        }
    },

    select: function (btn) {
        const region = btn.dataset.region;
        const dial   = btn.dataset.dial;
        const flag   = btn.querySelector('.chat-dial__flag')?.textContent ?? '';

        // Update trigger label
        const label = document.getElementById('chatDialLabel');
        if (label) label.textContent = flag + ' ' + dial;

        // Update hidden input and fire change event so Blazor picks it up
        const hidden = document.getElementById('chatDialRegion');
        if (hidden) {
            hidden.value = region;
            hidden.dispatchEvent(new Event('change', { bubbles: true }));
        }

        // Highlight active item
        document.querySelectorAll('#chatDialList .chat-dial__item').forEach(b => {
            b.classList.toggle('chat-dial__item--active', b === btn);
        });

        this.close();
    },

    // Clean phone input in JS — no Blazor round-trip while typing
    cleanPhone: function (input) {
        const pos = input.selectionStart;
        let val = input.value;

        // Strip leading zero
        if (val.startsWith('0')) val = val.slice(1);

        // Keep only digits, spaces, hyphens
        const cleaned = val.replace(/[^\d\s\-]/g, '');

        if (cleaned !== input.value) {
            input.value = cleaned;
            // Restore cursor position adjusted for removed chars
            const diff = val.length - cleaned.length;
            input.setSelectionRange(Math.max(0, pos - diff), Math.max(0, pos - diff));
        }
    }
};
