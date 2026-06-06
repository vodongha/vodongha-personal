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
