// ── File input trigger (replaces eval-based click) ───────────────────────────
window.clickFileInput = function (inputId) {
    var el = document.getElementById(inputId);
    if (el) { el.click(); }
};

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

// ── Mobile menu — resize-to-desktop redirect ──────────────────────────────────
window.addDesktopResizeRedirect = function (dotnetRef, breakpoint) {
    function onResize() {
        if (window.innerWidth > breakpoint) {
            dotnetRef.invokeMethodAsync('OnResizedToDesktop');
        }
    }
    window._desktopResizeHandler = onResize;
    window.addEventListener('resize', onResize);
};

window.removeDesktopResizeRedirect = function () {
    if (window._desktopResizeHandler) {
        window.removeEventListener('resize', window._desktopResizeHandler);
        window._desktopResizeHandler = null;
    }
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

// ── Table of Contents ─────────────────────────────────────────────────────────
window.initToc = function (dotnetRef, headingIds) {
    var content = document.querySelector('.blog-post__content');
    if (!content) return;

    // 1. Inject id attributes into h2/h3 elements to match the TOC anchors
    var headings = content.querySelectorAll('h2, h3');
    headings.forEach(function (h) {
        var text = h.textContent.trim();
        var id = text.toLowerCase()
            .replace(/[^a-z0-9\s-]/g, '')
            .trim()
            .replace(/\s+/g, '-');
        if (id) { h.id = id; }
    });

    // 2. IntersectionObserver to track active heading
    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                dotnetRef.invokeMethodAsync('SetActiveHeading', entry.target.id);
            }
        });
    }, { rootMargin: '-20% 0px -70% 0px', threshold: 0 });

    headings.forEach(function (h) { if (h.id) observer.observe(h); });
    window._tocObserver = observer;
};

window.destroyToc = function () {
    if (window._tocObserver) {
        window._tocObserver.disconnect();
        window._tocObserver = null;
    }
};

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

// ── Back to top button ────────────────────────────────────────────────────────
(function () {
    var btn = null;
    function getBtn() { return btn || (btn = document.getElementById('back-to-top')); }
    window.addEventListener('scroll', function () {
        var b = getBtn();
        if (!b) return;
        b.classList.toggle('is-visible', window.scrollY > 400);
    }, { passive: true });
})();

// ── Theme toggle ──────────────────────────────────────────────────────────────
// Reads the *actual* data-theme on <html> and flips it.
// Returns the new theme string so the Blazor component can update its icon state.
// Using the DOM as source-of-truth avoids race conditions where the C# _theme
// field hasn't been synced yet when the user clicks the toggle button.
window.toggleTheme = function () {
    var current = document.documentElement.getAttribute('data-theme') || 'dark';
    var next = current === 'light' ? 'dark' : 'light';
    window.setTheme(next);
    try { localStorage.setItem('theme', next); } catch (e) {}
    return next;
};

window.setTheme = function (theme) {
    var t = theme || 'dark';
    document.documentElement.setAttribute('data-theme', t);
    // Persist as a cookie so the server can pre-render data-theme on <html>
    // on the next request — eliminates the dark-flash on reload entirely.
    document.cookie = 'theme=' + t + ';path=/;max-age=31536000;samesite=lax';
    var meta = document.querySelector('meta[name="color-scheme"]');
    if (meta) { meta.setAttribute('content', t === 'light' ? 'light' : 'dark'); }
};

// Returns the stored user preference, or falls back to the OS/system preference.
// Used by Blazor components on first render to sync the toggle icon without
// having to pass the same logic down from every page.
window.getUserTheme = function () {
    // Cookie is the authoritative source (also readable server-side).
    var cookie = document.cookie.split(';').map(function(c){return c.trim();})
        .find(function(c){return c.startsWith('theme=');});
    if (cookie) {
        var val = cookie.split('=')[1];
        if (val === 'light' || val === 'dark') { return val; }
    }
    // Fallback: localStorage (legacy), then OS preference.
    var stored = localStorage.getItem('theme');
    if (stored === 'light' || stored === 'dark') { return stored; }
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
};

// ── Code block copy buttons ───────────────────────────────────────────────────
window.initCodeCopy = function (copyLabel, copiedLabel) {
    var content = document.querySelector('.blog-post__content');
    if (!content) return;

    content.querySelectorAll('pre').forEach(function (pre) {
        if (pre.querySelector('.code-copy-btn')) return; // already added
        var btn = document.createElement('button');
        btn.className = 'code-copy-btn';
        btn.setAttribute('type', 'button');
        btn.setAttribute('aria-label', copyLabel || 'Copy');
        btn.innerHTML = '<i class="bi bi-clipboard"></i> ' + (copyLabel || 'Copy');
        pre.style.position = 'relative';
        pre.appendChild(btn);

        btn.addEventListener('click', function () {
            var code = pre.querySelector('code');
            var text = code ? code.innerText : pre.innerText;
            navigator.clipboard.writeText(text).then(function () {
                btn.innerHTML = '<i class="bi bi-check2"></i> ' + (copiedLabel || 'Copied!');
                btn.classList.add('is-copied');
                setTimeout(function () {
                    btn.innerHTML = '<i class="bi bi-clipboard"></i> ' + (copyLabel || 'Copy');
                    btn.classList.remove('is-copied');
                }, 2000);
            });
        });
    });
};

window.destroyCodeCopy = function () {
    document.querySelectorAll('.code-copy-btn').forEach(function (btn) { btn.remove(); });
};

// ── Clipboard copy ─────────────────────────────────────────────────────────────
window.copyToClipboard = function (text) {
    return navigator.clipboard.writeText(text);
};

window.destroyReadingProgress = function () {
    if (window._readingProgressHandler) {
        window.removeEventListener('scroll', window._readingProgressHandler);
        window._readingProgressHandler = null;
    }
    var bar = document.getElementById('reading-progress-bar');
    if (bar) bar.style.width = '0%';
};
