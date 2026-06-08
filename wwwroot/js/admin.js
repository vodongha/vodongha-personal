// ── Sortable cards ────────────────────────────────────────────────────────────
window.initSortableCards = function (gridId, dotnetRef, prefKey) {
    var el = document.getElementById(gridId);
    if (!el || !window.Sortable) return;

    // Apply saved order first
    var saved = el.dataset.savedOrder;
    if (saved) {
        try {
            var ids = JSON.parse(saved);
            ids.forEach(function (id) {
                var card = el.querySelector('[data-card-id="' + id + '"]');
                if (card) el.appendChild(card);
            });
        } catch (e) {}
    }

    Sortable.create(el, {
        animation: 150,
        ghostClass: 'cost-card-ghost',
        chosenClass: 'cost-card-chosen',
        handle: '.cost-card-drag-handle',
        onEnd: function () {
            var ids = Array.from(el.children)
                .map(function (c) { return c.dataset.cardId; })
                .filter(Boolean);
            dotnetRef.invokeMethodAsync('SaveCardOrder', prefKey, ids);
        }
    });
};

// ── Global navigation loading bar ────────────────────────────────────────────
(function () {
    var bar   = null;
    var fill  = null;
    var timer = null;
    var prog  = 0;

    function getBar() {
        if (!bar) { bar = document.getElementById('nav-loading-bar'); fill = document.getElementById('nav-loading-bar__fill'); }
        return bar;
    }

    function start() {
        var b = getBar(); if (!b) return;
        clearTimeout(timer);
        prog = 20;
        fill.style.width = prog + '%';
        b.classList.remove('is-done');
        b.classList.add('is-loading');
        // Fake progress: creep toward 85% while waiting
        timer = setInterval(function () {
            if (prog < 85) { prog += (85 - prog) * 0.08; fill.style.width = prog + '%'; }
        }, 200);
    }

    function done() {
        var b = getBar(); if (!b) return;
        clearInterval(timer);
        b.classList.add('is-done');
        setTimeout(function () { b.classList.remove('is-loading', 'is-done'); fill.style.width = '0%'; }, 350);
    }

    function isInternal(href) {
        if (!href) return false;
        if (href.startsWith('#') || href.startsWith('javascript') || href.startsWith('mailto')) return false;
        try { var url = new URL(href, window.location.origin); return url.origin === window.location.origin; }
        catch (e) { return true; }
    }

    // Show on internal link clicks
    document.addEventListener('mousedown', function (e) {
        var a = e.target.closest('a[href]');
        if (a && isInternal(a.getAttribute('href')) && !a.getAttribute('download') && !a.getAttribute('target')) {
            start();
        }
    });

    // Show on button/submit clicks (forms, admin actions)
    document.addEventListener('mousedown', function (e) {
        var btn = e.target.closest('button:not([type="button"]):not(.admin-btn--ghost):not(.admin-btn--danger)');
        if (btn && !btn.closest('form[method="get"]')) {
            start();
            // Auto-hide after 4s in case nothing navigates
            setTimeout(done, 4000);
        }
    });

    // Hide when Blazor enhanced navigation completes
    document.addEventListener('blazor:navigated', done);
    if (window.Blazor) { try { window.Blazor.addEventListener('enhancedload', done); } catch (e) {} }
    window.addEventListener('load', done);

    // Expose for manual control
    window.navLoading = { start: start, done: done };
})();

// ── PDF download helper ───────────────────────────────────────────────────────
window.downloadFileFromBytes = function (filename, mimeType, bytes) {
    var blob = new Blob([new Uint8Array(bytes)], { type: mimeType });
    var url  = URL.createObjectURL(blob);
    var a    = document.createElement('a');
    a.href = url; a.download = filename; a.click();
    URL.revokeObjectURL(url);
};

// ── Select wrap ───────────────────────────────────────────────────────────────
// Event delegation — works regardless of when Blazor renders the elements
document.addEventListener('mousedown', function (e) {
    var sel = e.target.closest('.admin-select-wrap select');
    if (!sel) return;
    var wrap = sel.closest('.admin-select-wrap');
    wrap.classList.toggle('is-open');
});

document.addEventListener('change', function (e) {
    var sel = e.target.closest('.admin-select-wrap select');
    if (!sel) return;
    sel.closest('.admin-select-wrap').classList.remove('is-open');
});

document.addEventListener('blur', function (e) {
    var sel = e.target.closest('.admin-select-wrap select');
    if (!sel) return;
    sel.closest('.admin-select-wrap').classList.remove('is-open');
}, true);

// ── Reading progress bar ───────────────────────────────────────────────────────
window.initReadingProgress = function () {
    var bar = document.getElementById('reading-progress-bar');
    if (!bar) return;

    window._readingProgressHandler = function () {
        var content = document.querySelector('.blog-post__content');
        if (!content || !bar) return;
        var top = content.getBoundingClientRect().top + window.scrollY;
        var height = content.offsetHeight;
        var scrolled = window.scrollY - top;
        var pct = Math.min(100, Math.max(0, (scrolled / height) * 100));
        bar.style.width = pct + '%';
    };

    window.addEventListener('scroll', window._readingProgressHandler, { passive: true });
    window._readingProgressHandler(); // initial call
};

window.destroyReadingProgress = function () {
    if (window._readingProgressHandler) {
        window.removeEventListener('scroll', window._readingProgressHandler);
        window._readingProgressHandler = null;
    }
    var bar = document.getElementById('reading-progress-bar');
    if (bar) bar.style.width = '0%';
};
