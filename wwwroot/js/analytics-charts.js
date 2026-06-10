window.analyticsCharts = (() => {
    const charts = {};

    const _transparentBg = {
        id: 'transparentBg',
        beforeDraw: (chart) => {
            const ctx = chart.canvas.getContext('2d');
            ctx.save();
            ctx.clearRect(0, 0, chart.width, chart.height);
            ctx.restore();
        }
    };

    function isDark() {
        return document.documentElement.getAttribute('data-theme') !== 'light';
    }

    function gridColor() {
        return isDark() ? 'rgba(255,255,255,0.06)' : 'rgba(0,0,0,0.07)';
    }

    function tickColor() {
        return isDark() ? '#6b7280' : '#94a3b8';
    }

    function destroy(id) {
        if (charts[id]) {
            charts[id].destroy();
            delete charts[id];
        }
    }

    function renderLine(id, labels, data) {
        destroy(id);
        const canvas = document.getElementById(id);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');

        const gradient = ctx.createLinearGradient(0, 0, 0, canvas.offsetHeight || 220);
        gradient.addColorStop(0,   'rgba(124,106,247,0.40)');
        gradient.addColorStop(0.6, 'rgba(124,106,247,0.10)');
        gradient.addColorStop(1,   'rgba(124,106,247,0.00)');

        charts[id] = new Chart(canvas, {
            plugins: [_transparentBg],
            type: 'line',
            data: {
                labels,
                datasets: [{
                    label: 'Page views',
                    data,
                    borderColor: '#7c6af7',
                    borderWidth: 2,
                    backgroundColor: gradient,
                    fill: true,
                    tension: 0.45,
                    pointRadius: 3,
                    pointBackgroundColor: '#7c6af7',
                    pointBorderColor: isDark() ? '#141414' : '#ffffff',
                    pointBorderWidth: 2,
                    pointHoverRadius: 6,
                    pointHoverBorderWidth: 2,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: isDark() ? '#1a1a1a' : '#ffffff',
                        borderColor:     isDark() ? '#2a2a2a' : '#e2e8f0',
                        borderWidth: 1,
                        titleColor: isDark() ? '#e5e7eb' : '#0f172a',
                        bodyColor: '#7c6af7',
                        padding: 10,
                        cornerRadius: 8,
                    }
                },
                scales: {
                    x: {
                        ticks: { color: tickColor(), maxTicksLimit: 10, maxRotation: 0, font: { size: 11 } },
                        grid:  { color: gridColor() }
                    },
                    y: {
                        ticks: { color: tickColor(), precision: 0, font: { size: 11 } },
                        grid:  { color: gridColor() },
                        beginAtZero: true
                    }
                }
            }
        });
    }

    // accent: hex color, e.g. '#22c9b7'
    function renderBar(id, labels, data, accent) {
        destroy(id);
        const canvas = document.getElementById(id);
        if (!canvas) return;
        const color = accent || '#7c6af7';

        charts[id] = new Chart(canvas, {
            plugins: [_transparentBg],
            type: 'bar',
            data: {
                labels,
                datasets: [{
                    data,
                    backgroundColor: color + (isDark() ? 'b3' : 'cc'),
                    hoverBackgroundColor: color,
                    borderRadius: 5,
                    borderSkipped: false,
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: isDark() ? '#1a1a1a' : '#ffffff',
                        borderColor:     isDark() ? '#2a2a2a' : '#e2e8f0',
                        borderWidth: 1,
                        titleColor: isDark() ? '#e5e7eb' : '#0f172a',
                        bodyColor: color,
                        padding: 10,
                        cornerRadius: 8,
                    }
                },
                scales: {
                    x: {
                        ticks: { color: tickColor(), precision: 0, font: { size: 11 } },
                        grid:  { color: gridColor() },
                        beginAtZero: true
                    },
                    y: {
                        ticks: { color: isDark() ? '#c5c6d0' : '#475569', font: { size: 12 } },
                        grid:  { display: false }
                    }
                }
            }
        });
    }

    return { renderLine, renderBar, destroy };
})();
