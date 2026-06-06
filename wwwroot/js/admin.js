function bindSelectArrows() {
    document.querySelectorAll('.admin-select-wrap select').forEach(function (sel) {
        if (sel._arrowBound) return;
        sel._arrowBound = true;

        sel.addEventListener('mousedown', function () {
            var wrap = sel.closest('.admin-select-wrap');
            if (wrap.classList.contains('is-open')) {
                wrap.classList.remove('is-open');
            } else {
                wrap.classList.add('is-open');
            }
        });

        sel.addEventListener('change', function () {
            sel.closest('.admin-select-wrap').classList.remove('is-open');
        });

        sel.addEventListener('blur', function () {
            sel.closest('.admin-select-wrap').classList.remove('is-open');
        });
    });
}

document.addEventListener('DOMContentLoaded', bindSelectArrows);
document.addEventListener('enhancedload', bindSelectArrows);
