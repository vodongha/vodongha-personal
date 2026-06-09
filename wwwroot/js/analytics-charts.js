window.analyticsCharts = (() => {
    const charts = {};

    function destroy(id) {
        if (charts[id]) {
            charts[id].destroy();
            delete charts[id];
        }
    }

    function renderLine(id, labels, data) {
        destroy(id);
        const ctx = document.getElementById(id);
        if (!ctx) return;
        charts[id] = new Chart(ctx, {
            type: 'line',
            data: {
                labels,
                datasets: [{
                    label: 'Page views',
                    data,
                    borderColor: '#7c6af7',
                    backgroundColor: 'rgba(124,106,247,0.12)',
                    fill: true,
                    tension: 0.4,
                    pointRadius: 3,
                    pointHoverRadius: 5,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    x: {
                        ticks: { color: '#8b8fa8', maxTicksLimit: 10, maxRotation: 0 },
                        grid: { color: 'rgba(255,255,255,0.05)' }
                    },
                    y: {
                        ticks: { color: '#8b8fa8', precision: 0 },
                        grid: { color: 'rgba(255,255,255,0.05)' },
                        beginAtZero: true
                    }
                }
            }
        });
    }

    function renderBar(id, labels, data) {
        destroy(id);
        const ctx = document.getElementById(id);
        if (!ctx) return;
        charts[id] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels,
                datasets: [{
                    data,
                    backgroundColor: 'rgba(124,106,247,0.7)',
                    borderRadius: 4,
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    x: {
                        ticks: { color: '#8b8fa8', precision: 0 },
                        grid: { color: 'rgba(255,255,255,0.05)' },
                        beginAtZero: true
                    },
                    y: {
                        ticks: { color: '#c5c6d0', font: { size: 12 } },
                        grid: { display: false }
                    }
                }
            }
        });
    }

    return { renderLine, renderBar, destroy };
})();
