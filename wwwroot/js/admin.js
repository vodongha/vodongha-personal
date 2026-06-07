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
