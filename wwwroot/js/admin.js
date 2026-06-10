// ── File input trigger ────────────────────────────────────────────────────────
window.clickFileInput = (inputId) => {
    document.getElementById(inputId)?.click();
};

// ── Sortable cards ────────────────────────────────────────────────────────────
window.initSortableCards = (gridId, dotnetRef, prefKey) => {
    const el = document.getElementById(gridId);
    if (!el || !window.Sortable) return;

    const saved = el.dataset.savedOrder;
    if (saved) {
        try {
            const ids = JSON.parse(saved);
            ids.forEach(id => {
                const card = el.querySelector(`[data-card-id="${id}"]`);
                if (card) el.appendChild(card);
            });
        } catch { /* ignore invalid JSON */ }
    }

    Sortable.create(el, {
        animation: 150,
        ghostClass: 'cost-card-ghost',
        chosenClass: 'cost-card-chosen',
        handle: '.cost-card-drag-handle',
        onEnd: () => {
            const ids = Array.from(el.children)
                .map(c => c.dataset.cardId)
                .filter(Boolean);
            dotnetRef.invokeMethodAsync('SaveCardOrder', prefKey, ids);
        }
    });
};

// ── Mobile menu — resize-to-desktop redirect ──────────────────────────────────
window.addDesktopResizeRedirect = (dotnetRef, breakpoint) => {
    const onResize = () => {
        if (window.innerWidth > breakpoint) {
            dotnetRef.invokeMethodAsync('OnResizedToDesktop');
        }
    };
    window._desktopResizeHandler = onResize;
    window.addEventListener('resize', onResize);
};

window.removeDesktopResizeRedirect = () => {
    if (window._desktopResizeHandler) {
        window.removeEventListener('resize', window._desktopResizeHandler);
        window._desktopResizeHandler = null;
    }
};

// ── Global navigation loading bar ─────────────────────────────────────────────
(() => {
    let bar   = null;
    let fill  = null;
    let timer = null;
    let prog  = 0;

    const getBar = () => {
        if (!bar) {
            bar  = document.getElementById('nav-loading-bar');
            fill = document.getElementById('nav-loading-bar__fill');
        }
        return bar;
    };

    const start = () => {
        const b = getBar(); if (!b) return;
        clearTimeout(timer);
        prog = 20;
        fill.style.width = `${prog}%`;
        b.classList.remove('is-done');
        b.classList.add('is-loading');
        // Fake progress: creep toward 85% while waiting
        timer = setInterval(() => {
            if (prog < 85) { prog += (85 - prog) * 0.08; fill.style.width = `${prog}%`; }
        }, 200);
    };

    const done = () => {
        const b = getBar(); if (!b) return;
        clearInterval(timer);
        b.classList.add('is-done');
        setTimeout(() => { b.classList.remove('is-loading', 'is-done'); fill.style.width = '0%'; }, 350);
    };

    const isInternal = (href) => {
        if (!href) return false;
        if (href.startsWith('#') || href.startsWith('javascript') || href.startsWith('mailto')) return false;
        try { return new URL(href, window.location.origin).origin === window.location.origin; }
        catch { return true; }
    };

    // Show on internal link clicks
    document.addEventListener('mousedown', (e) => {
        const a = e.target.closest('a[href]');
        if (a && isInternal(a.getAttribute('href')) && !a.getAttribute('download') && !a.getAttribute('target')) {
            start();
        }
    });

    // Show on button/submit clicks (forms, admin actions)
    document.addEventListener('mousedown', (e) => {
        const btn = e.target.closest('button:not([type="button"]):not(.admin-btn--ghost):not(.admin-btn--danger)');
        if (btn && !btn.closest('form[method="get"]')) {
            start();
            // Auto-hide after 4s in case nothing navigates
            setTimeout(done, 4000);
        }
    });

    // Hide when Blazor enhanced navigation completes
    document.addEventListener('blazor:navigated', done);
    if (window.Blazor) { try { window.Blazor.addEventListener('enhancedload', done); } catch { /* API may not exist in all versions */ } }
    window.addEventListener('load', done);

    // Expose for manual control
    window.navLoading = { start, done };
})();

// ── PDF download helper ───────────────────────────────────────────────────────
window.downloadFileFromBytes = (filename, mimeType, bytes) => {
    const blob = new Blob([new Uint8Array(bytes)], { type: mimeType });
    const url  = URL.createObjectURL(blob);
    const a    = document.createElement('a');
    a.href = url; a.download = filename; a.click();
    URL.revokeObjectURL(url);
};

// ── Select wrap ───────────────────────────────────────────────────────────────
// Event delegation — works regardless of when Blazor renders the elements
document.addEventListener('mousedown', (e) => {
    const sel = e.target.closest('.admin-select-wrap select');
    if (!sel) return;
    sel.closest('.admin-select-wrap').classList.toggle('is-open');
});

document.addEventListener('change', (e) => {
    const sel = e.target.closest('.admin-select-wrap select');
    if (!sel) return;
    sel.closest('.admin-select-wrap').classList.remove('is-open');
});

document.addEventListener('blur', (e) => {
    const sel = e.target.closest('.admin-select-wrap select');
    if (!sel) return;
    sel.closest('.admin-select-wrap').classList.remove('is-open');
}, true);

// ── Table of Contents ─────────────────────────────────────────────────────────
window.initToc = (dotnetRef, headingIds) => {
    const content = document.querySelector('.blog-post__content');
    if (!content) return;

    // 1. Inject id attributes into h2/h3 elements to match the TOC anchors
    const headings = content.querySelectorAll('h2, h3');
    headings.forEach(h => {
        const id = h.textContent.trim()
            .toLowerCase()
            .replace(/[^a-z0-9\s-]/g, '')
            .trim()
            .replace(/\s+/g, '-');
        if (id) { h.id = id; }
    });

    // 2. IntersectionObserver to track active heading
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                dotnetRef.invokeMethodAsync('SetActiveHeading', entry.target.id);
            }
        });
    }, { rootMargin: '-20% 0px -70% 0px', threshold: 0 });

    headings.forEach(h => { if (h.id) observer.observe(h); });
    window._tocObserver = observer;
};

window.destroyToc = () => {
    window._tocObserver?.disconnect();
    window._tocObserver = null;
};

// ── Reading progress bar ──────────────────────────────────────────────────────
window.initReadingProgress = () => {
    const bar = document.getElementById('reading-progress-bar');
    if (!bar) return;

    window._readingProgressHandler = () => {
        const content = document.querySelector('.blog-post__content');
        if (!content) return;
        const top     = content.getBoundingClientRect().top + window.scrollY;
        const height  = content.offsetHeight;
        const scrolled = window.scrollY - top;
        const pct = Math.min(100, Math.max(0, (scrolled / height) * 100));
        bar.style.width = `${pct}%`;
    };

    window.addEventListener('scroll', window._readingProgressHandler, { passive: true });
    window._readingProgressHandler(); // initial call
};

// ── Back to top button ────────────────────────────────────────────────────────
(() => {
    let btn = null;
    const getBtn = () => btn || (btn = document.getElementById('back-to-top'));
    window.addEventListener('scroll', () => {
        getBtn()?.classList.toggle('is-visible', window.scrollY > 400);
    }, { passive: true });
})();

// ── Theme toggle ──────────────────────────────────────────────────────────────
// Reads the *actual* data-theme on <html> and flips it.
// Returns the new theme string so the Blazor component can update its icon state.
// Using the DOM as source-of-truth avoids race conditions where the C# _theme
// field hasn't been synced yet when the user clicks the toggle button.
window.toggleTheme = () => {
    const current = document.documentElement.getAttribute('data-theme') ?? 'dark';
    const next = current === 'light' ? 'dark' : 'light';
    window.setTheme(next);
    try { localStorage.setItem('theme', next); } catch { /* storage blocked */ }
    return next;
};

window.setTheme = (theme) => {
    const t = theme ?? 'dark';
    document.documentElement.setAttribute('data-theme', t);
    // Persist as a cookie so the server can pre-render data-theme on <html>
    // on the next request — eliminates the dark-flash on reload entirely.
    document.cookie = `theme=${t};path=/;max-age=31536000;samesite=lax`;
    const meta = document.querySelector('meta[name="color-scheme"]');
    meta?.setAttribute('content', t === 'light' ? 'light' : 'dark');
};

// Returns the stored user preference, or falls back to the OS/system preference.
// Used by Blazor components on first render to sync the toggle icon without
// having to pass the same logic down from every page.
window.getUserTheme = () => {
    // Cookie is the authoritative source (also readable server-side).
    const cookie = document.cookie.split(';')
        .map(c => c.trim())
        .find(c => c.startsWith('theme='));
    if (cookie) {
        const val = cookie.split('=')[1];
        if (val === 'light' || val === 'dark') { return val; }
    }
    // Fallback: localStorage (legacy), then OS preference.
    const stored = localStorage.getItem('theme');
    if (stored === 'light' || stored === 'dark') { return stored; }
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
};

// ── Code block copy buttons ───────────────────────────────────────────────────
window.initCodeCopy = (copyLabel, copiedLabel) => {
    const content = document.querySelector('.blog-post__content');
    if (!content) return;

    content.querySelectorAll('pre').forEach(pre => {
        if (pre.querySelector('.code-copy-btn')) return; // already added
        const btn = document.createElement('button');
        btn.className = 'code-copy-btn';
        btn.setAttribute('type', 'button');
        btn.setAttribute('aria-label', copyLabel ?? 'Copy');
        btn.innerHTML = `<i class="bi bi-clipboard"></i> ${copyLabel ?? 'Copy'}`;
        pre.style.position = 'relative';
        pre.appendChild(btn);

        btn.addEventListener('click', () => {
            const code = pre.querySelector('code');
            const text = code ? code.innerText : pre.innerText;
            navigator.clipboard.writeText(text).then(() => {
                btn.innerHTML = `<i class="bi bi-check2"></i> ${copiedLabel ?? 'Copied!'}`;
                btn.classList.add('is-copied');
                setTimeout(() => {
                    btn.innerHTML = `<i class="bi bi-clipboard"></i> ${copyLabel ?? 'Copy'}`;
                    btn.classList.remove('is-copied');
                }, 2000);
            });
        });
    });
};

window.destroyCodeCopy = () => {
    document.querySelectorAll('.code-copy-btn').forEach(btn => btn.remove());
};

// ── Clipboard copy ─────────────────────────────────────────────────────────────
window.copyToClipboard = (text) => navigator.clipboard.writeText(text);

window.destroyReadingProgress = () => {
    if (window._readingProgressHandler) {
        window.removeEventListener('scroll', window._readingProgressHandler);
        window._readingProgressHandler = null;
    }
    const bar = document.getElementById('reading-progress-bar');
    if (bar) bar.style.width = '0%';
};
