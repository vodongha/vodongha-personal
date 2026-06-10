window.dashboardCharts = (() => {
    const _charts = {};

    function isDark() {
        return (document.documentElement.getAttribute('data-theme') || 'dark') === 'dark';
    }
    function gridColor()  { return isDark() ? 'rgba(255,255,255,0.07)' : 'rgba(0,0,0,0.07)'; }
    function tickColor()  { return isDark() ? '#9ca3af' : '#6b7280'; }
    function tooltipBg()  { return isDark() ? '#1f2937' : '#ffffff'; }
    function tooltipTxt() { return isDark() ? '#f9fafb' : '#111827'; }

    const CATEGORY_COLORS = [
        '#6ee7b7', '#60a5fa', '#f59e0b', '#f472b6', '#a78bfa',
        '#34d399', '#fb923c', '#38bdf8', '#e879f9', '#facc15'
    ];

    function destroy(id) {
        if (_charts[id]) { _charts[id].destroy(); delete _charts[id]; }
    }

    function renderDonut(id, labels, data) {
        destroy(id);
        const ctx = document.getElementById(id);
        if (!ctx) return;
        _charts[id] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels,
                datasets: [{
                    data,
                    backgroundColor: CATEGORY_COLORS.slice(0, labels.length),
                    borderWidth: 2,
                    borderColor: isDark() ? '#111827' : '#ffffff',
                    hoverOffset: 6
                }]
            },
            options: {
                cutout: '62%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            color: tickColor(),
                            padding: 14,
                            font: { size: 12 },
                            boxWidth: 12,
                            boxHeight: 12
                        }
                    },
                    tooltip: {
                        backgroundColor: tooltipBg(),
                        titleColor: tooltipTxt(),
                        bodyColor: tooltipTxt(),
                        callbacks: {
                            label: ctx => ` ${ctx.parsed} skills`
                        }
                    }
                }
            }
        });
    }

    function renderHBar(id, labels, data) {
        destroy(id);
        const ctx = document.getElementById(id);
        if (!ctx) return;
        _charts[id] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels,
                datasets: [{
                    data,
                    backgroundColor: '#6ee7b7cc',
                    borderColor: '#6ee7b7',
                    borderWidth: 1,
                    borderRadius: 4
                }]
            },
            options: {
                indexAxis: 'y',
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: tooltipBg(),
                        titleColor: tooltipTxt(),
                        bodyColor: tooltipTxt(),
                        callbacks: { label: ctx => ` ${ctx.parsed.x} views` }
                    }
                },
                scales: {
                    x: {
                        grid: { color: gridColor() },
                        ticks: { color: tickColor(), font: { size: 11 } },
                        beginAtZero: true
                    },
                    y: {
                        grid: { display: false },
                        ticks: {
                            color: tickColor(),
                            font: { size: 11 },
                            callback: function(val) {
                                const label = this.getLabelForValue(val);
                                return label.length > 28 ? label.substring(0, 26) + '…' : label;
                            }
                        }
                    }
                }
            }
        });
    }

    function renderLine(id, labels, data) {
        destroy(id);
        const ctx = document.getElementById(id);
        if (!ctx) return;
        const gradient = ctx.getContext('2d').createLinearGradient(0, 0, 0, 140);
        gradient.addColorStop(0, isDark() ? 'rgba(110,231,183,0.35)' : 'rgba(16,185,129,0.25)');
        gradient.addColorStop(1, 'rgba(0,0,0,0)');

        _charts[id] = new Chart(ctx, {
            type: 'line',
            data: {
                labels,
                datasets: [{
                    data,
                    fill: true,
                    backgroundColor: gradient,
                    borderColor: '#6ee7b7',
                    borderWidth: 2,
                    pointRadius: 3,
                    pointBackgroundColor: '#6ee7b7',
                    tension: 0.4
                }]
            },
            options: {
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: tooltipBg(),
                        titleColor: tooltipTxt(),
                        bodyColor: tooltipTxt(),
                        callbacks: { label: ctx => ` ${ctx.parsed.y} views` }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { color: tickColor(), font: { size: 11 }, maxTicksLimit: 7 }
                    },
                    y: {
                        grid: { color: gridColor() },
                        ticks: { color: tickColor(), font: { size: 11 } },
                        beginAtZero: true
                    }
                }
            }
        });
    }

    function onThemeChange() {
        Object.keys(_charts).forEach(id => {
            const chart = _charts[id];
            if (!chart) return;
            const ds = chart.data.datasets[0];
            if (chart.config.type === 'doughnut') {
                ds.borderColor = isDark() ? '#111827' : '#ffffff';
            }
            chart.options.plugins.tooltip.backgroundColor = tooltipBg();
            chart.options.plugins.tooltip.titleColor      = tooltipTxt();
            chart.options.plugins.tooltip.bodyColor       = tooltipTxt();
            if (chart.options.scales) {
                ['x','y'].forEach(axis => {
                    if (chart.options.scales[axis]) {
                        if (chart.options.scales[axis].grid) chart.options.scales[axis].grid.color = gridColor();
                        if (chart.options.scales[axis].ticks) chart.options.scales[axis].ticks.color = tickColor();
                    }
                });
            }
            if (chart.options.plugins.legend?.labels) {
                chart.options.plugins.legend.labels.color = tickColor();
            }
            chart.update();
        });
    }

    return { renderDonut, renderHBar, renderLine, destroy, onThemeChange };
})();
